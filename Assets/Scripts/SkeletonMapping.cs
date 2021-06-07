namespace SoftHand
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;
    using static OVRSkeleton;
    [DefaultExecutionOrder(-90)]
    public class SkeletonMapping : MonoBehaviour
    {
        [SerializeField]
        OVRCustomSkeleton _ovrSkeleton;
        [SerializeField]
        Transform _rootPose;
        [SerializeField]
        private bool _applyBoneTranslations = true;
        [SerializeField]
        protected SkeletonType _skeletonType = OVRSkeleton.SkeletonType.None;
        public IList<OVRBone> Bones { get; protected set; }

        //[HideInInspector]
        [SerializeField]
        private List<Transform> _customBones_V2 = new List<Transform>(new Transform[(int)BoneId.Max]);
        public SkeletonType SkeletonType => _skeletonType;
        public BoneId CurrentStartBoneId => _ovrSkeleton.GetCurrentStartBoneId() == BoneId.Hand_Start ? BoneId.Hand_Thumb0 : BoneId.Invalid;
        public BoneId CurrentEndBoneId => _ovrSkeleton.GetCurrentEndBoneId();// == BoneId.Hand_End ? BoneId.Hand_End : BoneId.Invalid;
        public OVRCustomSkeleton OVRSkeletonProvider => _ovrSkeleton;
        public Transform RootPose => _rootPose;
        //public List<ArtBody> _artBodies = new List<ArtBody>();

        private const int N_FINGERS = 5;
        private const int N_ACTIVE_BONES = 3;

        private ArticulationBody _palmBody;
        // private List<ArticulationBody> _articulationBodies;

        public List<bool> invertedVectorList { get; private set; }
        public List<bool> flippedYVectorList { get; private set; }

        public Dictionary<int, int> LookupDic { get; private set; } // bone index - art index table
        private List<ArticulationBody> _bodies;

        private BoxCollider _palmCollider;
        private CapsuleCollider[] _capsuleColliders;
        private int _lastFrameTeleport = 0;
        private bool _ghosted = false;
        private int _layerMask;
        [SerializeField]
        private SkinnedMeshRenderer _handRenderer;

        [Range(0.1f, 10f)]
        public float _strength = 1f;

        [SerializeField]
        [Tooltip("The mass of each finger bone; the palm will be 3x this.")]
        private float _perBoneMass = 3.0f;

        [SerializeField]
        [Tooltip("The physics material that the hand uses.")]
        private PhysicMaterial _material = null;

#if UNITY_EDITOR

        private static readonly string[] _fbxHandSidePrefix = { "l_", "r_" };
        private static readonly string _fbxHandBonePrefix = "b_";
        private static readonly string _boneOffsetPrefix = "off_";

        private static readonly string[] _fbxHandBoneNames =
        {
        "wrist",
        "forearm_stub",
        "thumb0",
        "thumb1",
        "thumb2",
        "thumb3",
        "index1",
        "index2",
        "index3",
        "middle1",
        "middle2",
        "middle3",
        "ring1",
        "ring2",
        "ring3",
        "pinky0",
        "pinky1",
        "pinky2",
        "pinky3"
    };

        private static readonly string[] _fbxHandFingerNames =
        {
        "thumb",
        "index",
        "middle",
        "ring",
        "pinky"
    };
#endif

        public List<Transform> CustomBones { get { return _customBones_V2; } }
        internal List<OVRBone> bones;
        protected OVRPlugin.Skeleton2 _skeleton = new OVRPlugin.Skeleton2();

        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            InitializeCapsules();
           // InitializeArticulationBodies();
            
        }
      

#if UNITY_EDITOR

        //[ContextMenu("Fix bone Hierarchy")]
        private void FixBoneHierarchy()
        {
            // itterate throught each bone and make it a child of an empty GO which has rotatin += (0, -90, 90)
            // name this empty GO as a child bone with a prefix "off_"
            BoneId start = CurrentStartBoneId;
            BoneId end = CurrentEndBoneId;
            SkeletonType skeletonType = SkeletonType;
            if (start != BoneId.Invalid && end != BoneId.Invalid)
            {
                for (int bi = (int)start; bi < (int)end; ++bi)
                {
                    string fbxBoneName = FbxBoneNameFromBoneId(skeletonType, (BoneId)bi);
                    Transform t = transform.FindChildRecursive(fbxBoneName);

                    if (t != null)
                    {
                        //copy bone rotation
                        Quaternion rot = t.localRotation; // * Quaternion.AngleAxis(90, Vector3.down) * Quaternion.AngleAxis(90, Vector3.forward);
                        rot *= Quaternion.Euler(0, -90, 90); // apply needed rotation
                        GameObject go = Instantiate(new GameObject("Offsetter"), t.position, rot, t.parent);  // create parent Go with applied rotation
                        t.SetParent(go.transform, true); // set bone as child of created parent
                        _customBones_V2[(int)bi] = go.transform;
                    }
                }
            }
        }
        public void TryAutoMapBonesByName()
        {
            BoneId start = CurrentStartBoneId;
            BoneId end = CurrentEndBoneId;
            SkeletonType skeletonType = SkeletonType;
            if (start != BoneId.Invalid && end != BoneId.Invalid)
            {
                for (int bi = (int)start; bi < (int)end; ++bi)
                {
                    string fbxBoneName = FbxBoneNameFromBoneId(skeletonType, (BoneId)bi);
                    Transform t = transform.FindChildRecursive(fbxBoneName);

                    if (t != null)
                    {
                        _customBones_V2[(int)bi] = t;
                    }
                }
            }
        }

        private bool TryGetBoneCapsule(int boneId, out OVRPlugin.BoneCapsule capsule)
        {
            capsule = new OVRPlugin.BoneCapsule();
            if (bones == null)
                return false;
            OVRBone bone = bones[boneId];
            capsule = _skeleton.BoneCapsules.FirstOrDefault(b => b.BoneIndex == (int)bone.Id);
            return capsule.BoneIndex != -1;
        }

        private static int GetFingerIndexFromBoneId(int bi)
        {
            switch (bi)
            {
                case int n when (n <= (int)BoneId.Hand_Thumb3):
                    return 0;// thumb 0
                case int n when (n <= (int)BoneId.Hand_Index3):
                    return 1; // index 1
                case int n when (n <= (int)BoneId.Hand_Middle3):
                    return 2; // middle 2
                case int n when (n <= (int)BoneId.Hand_Ring3):
                    return 3;// ring 3
                case int n when (n <= (int)BoneId.Hand_Pinky3):
                    return 4;  // pinky 4

                default:
                    return (int)bi; // finger tip
            }
        }

        private static int GetFingerTipIndexFromBoneId(int bi)
        {
            int fingerIdex = GetFingerIndexFromBoneId(bi);
            if (fingerIdex <= 4)
                return fingerIdex + (int)BoneId.Hand_MaxSkinnable;
            return fingerIdex;
        }

        private static string FbxBoneNameFromBoneId(SkeletonType skeletonType, BoneId bi)
        {
            {
                if (bi >= BoneId.Hand_ThumbTip && bi <= BoneId.Hand_PinkyTip)
                {
                    return _fbxHandSidePrefix[(int)skeletonType] + _fbxHandFingerNames[(int)bi - (int)BoneId.Hand_ThumbTip] + "_finger_tip_marker";
                }
                else
                {
                    return _fbxHandBonePrefix + _fbxHandSidePrefix[(int)skeletonType] + _fbxHandBoneNames[(int)bi];
                }
            }
        }
#endif

        private void Start()
        {
            if (ShouldInitialize())
            {
                Initialize();
            }
        }

        internal bool ShouldInitialize()
        {
            if (IsInitialized)
            {
                return false;
            }

            if (_skeletonType == SkeletonType.None)
            {
                return false;
            }
            else if (_skeletonType == SkeletonType.HandLeft || _skeletonType == SkeletonType.HandRight)
            {
#if UNITY_EDITOR
                return OVRInput.IsControllerConnected(OVRInput.Controller.Hands);
#else
			return true;
#endif
            }
            else
            {
                return true;
            }
        }
        internal void Initialize()
        {
            if (OVRPlugin.GetSkeleton2((OVRPlugin.SkeletonType)_skeletonType, ref _skeleton))
            {
                InitializeBones();
                InitializeCapsules();
                InitializeMaterial();
               // InitializeArticulationBodies();               

                IsInitialized = true;
            }
        }

        private void InitializeMaterial()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                return;

            // We also require a material for friction to be able to work.
            if (_material == null || _material.bounciness != 0.0f || _material.bounceCombine != PhysicMaterialCombine.Minimum)
            {
                UnityEditor.EditorUtility.DisplayDialog("Collision Error!",
                                                        "An InteractionBrushHand must have a material with 0 bounciness "
                                                        + "and a bounceCombine of Minimum.  Name:" + gameObject.name,
                                                        "Ok");
                Debug.Break();
            }
#endif
        }

        internal void InitializeBones()
        {
            bool flipX = (_skeletonType == OVRSkeleton.SkeletonType.HandLeft || _skeletonType == OVRSkeleton.SkeletonType.HandRight);

            if (bones == null || bones.Count != _skeleton.NumBones)
            {
                bones = new List<OVRBone>(new OVRBone[_skeleton.NumBones]);
                Bones = bones.AsReadOnly();
            }

            for (int i = 0; i < bones.Count; ++i)
            {
                OVRBone bone = bones[i] ?? (bones[i] = new OVRBone());
                bone.Id = (OVRSkeleton.BoneId)_skeleton.Bones[i].Id;
                bone.ParentBoneIndex = _skeleton.Bones[i].ParentBoneIndex;
                bone.Transform = _customBones_V2[(int)bone.Id];

                if (bone.Transform == null)
                    continue;

                if (_applyBoneTranslations)
                {
                    bone.Transform.localPosition = flipX ? _skeleton.Bones[i].Pose.Position.FromFlippedXVector3f() : _skeleton.Bones[i].Pose.Position.FromFlippedZVector3f();
                }

                bone.Transform.localRotation = flipX ? _skeleton.Bones[i].Pose.Orientation.FromFlippedXQuatf() : _skeleton.Bones[i].Pose.Orientation.FromFlippedZQuatf();
            }
        }
        [ContextMenu("Make capsule colliders")]
        private void InitializeCapsules()
        {
            // Get the layers that collide with this hand
            //int myLayer = gameObject.layer;
            //for (int i = 0; i < 32; i++)
            //{
            //    if (!Physics.GetIgnoreLayerCollision(myLayer, i))
            //    {
            //        _layerMask = _layerMask | 1 << i;
            //    }
            //}

            //bool flipX = (_skeletonType == SkeletonType.HandLeft || _skeletonType == SkeletonType.HandRight);



            //if (_artBodies == null /*|| _capsules.Count != _skeleton.NumBoneCapsules*/)
            //{
            //    _artBodies = new List<ArtBody>();
            //    //Capsules = _capsules.AsReadOnly();
            //}

            BoneId start = CurrentStartBoneId;
            BoneId end = BoneId.Hand_MaxSkinnable;
            if (start != BoneId.Invalid && end != BoneId.Invalid)
            {
                // if (_capsuleColliders == null)
                //     _capsuleColliders = new CapsuleCollider[(int)end];
                for (int i = (int)start; i < (int)end; ++i)
                {
                    float radi = TryGetBoneCapsule(i, out OVRPlugin.BoneCapsule ca) ? ca.Radius : 0.007f;
                    Transform nextBoneTransform = CustomBones[i + 1];
                    int curFingerIndex = GetFingerIndexFromBoneId(i);
                    int nextBoneFingerIndex = GetFingerIndexFromBoneId(i + 1);
                    if (curFingerIndex != nextBoneFingerIndex)
                        nextBoneTransform = CustomBones[GetFingerTipIndexFromBoneId(i)];
                    GameObject go = CustomBones[i].gameObject;
                    // CapsuleCollider capsule = CustomBones[i].gameObject.AddComponent<CapsuleCollider>();
                    CapsuleCollider capsule = go.TryGetComponent<CapsuleCollider>(out capsule) ? capsule : go.AddComponent<CapsuleCollider>();
                    capsule.direction = 0;
                    capsule.radius = radi;
                    capsule.height = (CustomBones[i].position - nextBoneTransform.position).magnitude + capsule.radius;
                    capsule.material = _material;
                    capsule.center = new Vector3(capsule.height / 2f - capsule.radius, 0f, 0f);
                    if (SkeletonType == SkeletonType.HandLeft)
                        capsule.center *= -1;
                    //capsule.isTrigger = true;
                    //_capsuleColliders[i] = capsule;
                    //go.transform.SetParent(CustomBones[i]);
                    //go.transform.localPosition = Vector3.zero;
                    //go.transform.localRotation = Quaternion.identity;
                }
            }
        }

        [ContextMenu("Make Art bodies")]
        private void InitializeArticulationBodies()
        {
            BoneId start = CurrentStartBoneId;
            BoneId end = BoneId.Hand_MaxSkinnable;
            if (start != BoneId.Invalid && end != BoneId.Invalid)
            {
                flippedYVectorList = new List<bool>();
                invertedVectorList = new List<bool>();
                for (int i = (int)start; i < (int)end; ++i)
                {
                    GameObject go = CustomBones[i].gameObject;
                    if (go != null)
                    {
                        BoneId bi = (BoneId)i;
                        ArticulationBody body = go.TryGetComponent<ArticulationBody>(out body) ? body : go.AddComponent<ArticulationBody>();
                        body.SetupForBone(bi, out bool isVectorInverted, out bool isYFlipped);
                        body.useGravity = false;
                    }

                }

            }
        }

        [ContextMenu("Reset Art bodies")]
        private void ResetArtBodies()
        {
            BoneId start = CurrentStartBoneId;
            BoneId end = BoneId.Hand_MaxSkinnable;
            if (start != BoneId.Invalid && end != BoneId.Invalid)
            {
                _bodies = new List<ArticulationBody>();
                for (int i = (int)start; i < (int)end; ++i)
                {
                    GameObject go = CustomBones[i].gameObject;
                    if (go != null)
                    {
                        BoneId bi = (BoneId)i;
                        ArticulationBody body = go.TryGetComponent<ArticulationBody>(out body) ? body : go.AddComponent<ArticulationBody>();
                        body.ResetAnchorLimitsAndRotaion();
                        body.useGravity = false;
                        _bodies.Add(body);
                        // hack to re-initialize AB by toggling on and off
                        //body.enabled = false;
                        // body.enabled = true;
                    }

                }

            }
        }
    }
}

//public class ArtBody
//{
//    public short BoneIndex { get; set; }
//    public ArticulationBody ArticulationBody { get; set; }
//    public CapsuleCollider CapsuleCollider { get; set; }

//    public ArtBody() { }

//    public ArtBody(short boneIndex, ArticulationBody capsuleRigidBody, CapsuleCollider capsuleCollider)
//    {
//        BoneIndex = boneIndex;
//        ArticulationBody = capsuleRigidBody;
//        CapsuleCollider = capsuleCollider;
//    }
//}

