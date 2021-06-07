namespace SoftHand
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using static OVRPlugin;
    using static OVRSkeleton;

    [DefaultExecutionOrder(-80)]
    public class OVRSkeletonBase : MonoBehaviour
    { 
        [SerializeField]
        protected OVRSkeleton.SkeletonType _skeletonType = OVRSkeleton.SkeletonType.None;
        [SerializeField]
        private IOVRSkeletonDataProvider _dataProvider;

        [SerializeField]
        private bool _updateRootPose = false;
        [SerializeField]
        private bool _updateRootScale = false;
        [SerializeField]
        private bool _enablePhysicsCapsules = false;

        private GameObject _bonesGO;
        private GameObject _bindPosesGO;
        private GameObject _capsulesGO;

        protected List<OVRBone> _bones;
        private List<OVRBone> _bindPoses;
        private List<OVRBoneCapsule> _capsules;

        protected Skeleton2 _skeleton = new OVRPlugin.Skeleton2();
        private readonly Quaternion wristFixupRotation = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);

        public bool IsInitialized { get; private set; }
        public bool IsDataValid { get; private set; }
        public bool IsDataHighConfidence { get; private set; }
        public IList<OVRBone> Bones { get; protected set; }
        public IList<OVRBone> BindPoses { get; private set; }
        public IList<OVRBoneCapsule> Capsules { get; private set; }
        public OVRSkeleton.SkeletonType GetSkeletonType() { return _skeletonType; }
        public int SkeletonChangedCount { get; private set; }

        private void Awake()
        {
            if (_dataProvider == null)
            {
                _dataProvider = GetComponent<IOVRSkeletonDataProvider>();
            }

            _bones = new List<OVRBone>();
            Bones = _bones.AsReadOnly();

            _bindPoses = new List<OVRBone>();
            BindPoses = _bindPoses.AsReadOnly();

            _capsules = new List<OVRBoneCapsule>();
            Capsules = _capsules.AsReadOnly();
        }

        private void Start()
        {
            if (ShouldInitialize())
            {
                Initialize();
            }
        }

        private bool ShouldInitialize()
        {
            if (IsInitialized)
            {
                return false;
            }

            if (_skeletonType == OVRSkeleton.SkeletonType.None)
            {
                return false;
            }
            else if (_skeletonType == OVRSkeleton.SkeletonType.HandLeft || _skeletonType == OVRSkeleton.SkeletonType.HandRight)
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

        private void Initialize()
        {
            if (OVRPlugin.GetSkeleton2((OVRPlugin.SkeletonType)_skeletonType, ref _skeleton))
            {
                InitializeBones();
                InitializeBindPose();
                InitializeCapsules();

                IsInitialized = true;
            }
        }



        protected virtual void InitializeBones()
        {
            bool flipX = (_skeletonType == OVRSkeleton.SkeletonType.HandLeft || _skeletonType == OVRSkeleton.SkeletonType.HandRight);

            if (!_bonesGO)
            {
                _bonesGO = new GameObject("Bones");
                _bonesGO.transform.SetParent(transform, false);
                _bonesGO.transform.localPosition = Vector3.zero;
                _bonesGO.transform.localRotation = Quaternion.identity;
            }

            if (_bones == null || _bones.Count != _skeleton.NumBones)
            {
                _bones = new List<OVRBone>(new OVRBone[_skeleton.NumBones]);
                Bones = _bones.AsReadOnly();
            }

            // pre-populate bones list before attempting to apply bone hierarchy
            for (int i = 0; i < _bones.Count; ++i)
            {
                OVRBone bone = _bones[i] ?? (_bones[i] = new OVRBone());
                bone.Id = (OVRSkeleton.BoneId)_skeleton.Bones[i].Id;
                bone.ParentBoneIndex = _skeleton.Bones[i].ParentBoneIndex;

                Transform trans = bone.Transform ?? (bone.Transform = new GameObject(bone.Id.ToString()).transform);
                trans.localPosition = flipX ? _skeleton.Bones[i].Pose.Position.FromFlippedXVector3f() : _skeleton.Bones[i].Pose.Position.FromFlippedZVector3f();
                trans.localRotation = flipX ? _skeleton.Bones[i].Pose.Orientation.FromFlippedXQuatf() : _skeleton.Bones[i].Pose.Orientation.FromFlippedZQuatf();
            }

            for (int i = 0; i < _bones.Count; ++i)
            {
                if ((OVRSkeleton.BoneId)_bones[i].ParentBoneIndex == OVRSkeleton.BoneId.Invalid)
                {
                    _bones[i].Transform.SetParent(_bonesGO.transform, false);
                }
                else
                {
                    _bones[i].Transform.SetParent(_bones[_bones[i].ParentBoneIndex].Transform, false);
                }
            }
        }

        private void InitializeBindPose()
        {
            if (!_bindPosesGO)
            {
                _bindPosesGO = new GameObject("BindPoses");
                _bindPosesGO.transform.SetParent(transform, false);
                _bindPosesGO.transform.localPosition = Vector3.zero;
                _bindPosesGO.transform.localRotation = Quaternion.identity;
            }

            if (_bindPoses == null || _bindPoses.Count != _bones.Count)
            {
                _bindPoses = new List<OVRBone>(new OVRBone[_bones.Count]);
                BindPoses = _bindPoses.AsReadOnly();
            }

            // pre-populate bones list before attempting to apply bone hierarchy
            for (int i = 0; i < _bindPoses.Count; ++i)
            {
                OVRBone bone = _bones[i];
                OVRBone bindPoseBone = _bindPoses[i] ?? (_bindPoses[i] = new OVRBone());
                bindPoseBone.Id = bone.Id;
                bindPoseBone.ParentBoneIndex = bone.ParentBoneIndex;

                Transform trans = bindPoseBone.Transform ?? (bindPoseBone.Transform = new GameObject(bindPoseBone.Id.ToString()).transform);
                trans.localPosition = bone.Transform.localPosition;
                trans.localRotation = bone.Transform.localRotation;
            }

            for (int i = 0; i < _bindPoses.Count; ++i)
            {
                if ((OVRSkeleton.BoneId)_bindPoses[i].ParentBoneIndex == OVRSkeleton.BoneId.Invalid)
                {
                    _bindPoses[i].Transform.SetParent(_bindPosesGO.transform, false);
                }
                else
                {
                    _bindPoses[i].Transform.SetParent(_bindPoses[_bindPoses[i].ParentBoneIndex].Transform, false);
                }
            }
        }

        private void InitializeCapsules()
        {
            bool flipX = (_skeletonType == OVRSkeleton.SkeletonType.HandLeft || _skeletonType == OVRSkeleton.SkeletonType.HandRight);

            if (_enablePhysicsCapsules)
            {
                if (!_capsulesGO)
                {
                    _capsulesGO = new GameObject("Capsules");
                    _capsulesGO.transform.SetParent(transform, false);
                    _capsulesGO.transform.localPosition = Vector3.zero;
                    _capsulesGO.transform.localRotation = Quaternion.identity;
                }

                if (_capsules == null || _capsules.Count != _skeleton.NumBoneCapsules)
                {
                    _capsules = new List<OVRBoneCapsule>(new OVRBoneCapsule[_skeleton.NumBoneCapsules]);
                    Capsules = _capsules.AsReadOnly();
                }

                for (int i = 0; i < _capsules.Count; ++i)
                {
                    OVRBone bone = _bones[_skeleton.BoneCapsules[i].BoneIndex];
                    OVRBoneCapsule capsule = _capsules[i] ?? (_capsules[i] = new OVRBoneCapsule());
                    capsule.BoneIndex = _skeleton.BoneCapsules[i].BoneIndex;

                    if (capsule.CapsuleRigidbody == null)
                    {
                        capsule.CapsuleRigidbody = new GameObject((bone.Id).ToString() + "_CapsuleRigidbody").AddComponent<Rigidbody>();
                        capsule.CapsuleRigidbody.mass = 1.0f;
                        capsule.CapsuleRigidbody.isKinematic = true;
                        capsule.CapsuleRigidbody.useGravity = false;
                        capsule.CapsuleRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    }

                    GameObject rbGO = capsule.CapsuleRigidbody.gameObject;
                    rbGO.transform.SetParent(_capsulesGO.transform, false);
                    rbGO.transform.position = bone.Transform.position;
                    rbGO.transform.rotation = bone.Transform.rotation;

                    if (capsule.CapsuleCollider == null)
                    {
                        capsule.CapsuleCollider = new GameObject((bone.Id).ToString() + "_CapsuleCollider").AddComponent<CapsuleCollider>();
                        capsule.CapsuleCollider.isTrigger = false;
                    }

                    var p0 = flipX ? _skeleton.BoneCapsules[i].StartPoint.FromFlippedXVector3f() : _skeleton.BoneCapsules[i].StartPoint.FromFlippedZVector3f();
                    var p1 = flipX ? _skeleton.BoneCapsules[i].EndPoint.FromFlippedXVector3f() : _skeleton.BoneCapsules[i].EndPoint.FromFlippedZVector3f();
                    var delta = p1 - p0;
                    var mag = delta.magnitude;
                    var rot = Quaternion.FromToRotation(Vector3.right, delta);
                    capsule.CapsuleCollider.radius = _skeleton.BoneCapsules[i].Radius;
                    capsule.CapsuleCollider.height = mag + _skeleton.BoneCapsules[i].Radius * 2.0f;
                    capsule.CapsuleCollider.direction = 0;
                    capsule.CapsuleCollider.center = Vector3.right * mag * 0.5f;

                    GameObject ccGO = capsule.CapsuleCollider.gameObject;
                    ccGO.transform.SetParent(rbGO.transform, false);
                    ccGO.transform.localPosition = p0;
                    ccGO.transform.localRotation = rot;
                }
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (ShouldInitialize())
            {
                Initialize();
            }
#endif

            if (!IsInitialized || _dataProvider == null)
            {
                IsDataValid = false;
                IsDataHighConfidence = false;

                return;
            }

            var data = _dataProvider.GetSkeletonPoseData();

            IsDataValid = data.IsDataValid;
            if (data.IsDataValid)
            {
                if (SkeletonChangedCount != data.SkeletonChangedCount)
                {
                    SkeletonChangedCount = data.SkeletonChangedCount;
                    IsInitialized = false;
                    Initialize();
                }

                IsDataHighConfidence = data.IsDataHighConfidence;

                if (_updateRootPose)
                {
                    transform.localPosition = data.RootPose.Position.FromFlippedZVector3f();
                    transform.localRotation = data.RootPose.Orientation.FromFlippedZQuatf();
                }

                if (_updateRootScale)
                {
                    transform.localScale = new Vector3(data.RootScale, data.RootScale, data.RootScale);
                }

                for (var i = 0; i < _bones.Count; ++i)
                {
                    if (_bones[i].Transform != null)
                    {
                        if (_skeletonType == OVRSkeleton.SkeletonType.HandLeft || _skeletonType == OVRSkeleton.SkeletonType.HandRight)
                        {
                            _bones[i].Transform.localRotation = data.BoneRotations[i].FromFlippedXQuatf();

                            if (_bones[i].Id == OVRSkeleton.BoneId.Hand_WristRoot)
                            {
                                _bones[i].Transform.localRotation *= wristFixupRotation;
                            }
                        }
                        else
                        {
                            _bones[i].Transform.localRotation = data.BoneRotations[i].FromFlippedZQuatf();
                        }
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (!IsInitialized || _dataProvider == null)
            {
                IsDataValid = false;
                IsDataHighConfidence = false;

                return;
            }

            Update();

            if (_enablePhysicsCapsules)
            {
                var data = _dataProvider.GetSkeletonPoseData();

                IsDataValid = data.IsDataValid;
                IsDataHighConfidence = data.IsDataHighConfidence;

                for (int i = 0; i < _capsules.Count; ++i)
                {
                    OVRBoneCapsule capsule = _capsules[i];
                    var capsuleGO = capsule.CapsuleRigidbody.gameObject;

                    if (data.IsDataValid && data.IsDataHighConfidence)
                    {
                        Transform bone = _bones[(int)capsule.BoneIndex].Transform;

                        if (capsuleGO.activeSelf)
                        {
                            capsule.CapsuleRigidbody.MovePosition(bone.position);
                            capsule.CapsuleRigidbody.MoveRotation(bone.rotation);
                        }
                        else
                        {
                            capsuleGO.SetActive(true);
                            capsule.CapsuleRigidbody.position = bone.position;
                            capsule.CapsuleRigidbody.rotation = bone.rotation;
                        }
                    }
                    else
                    {
                        if (capsuleGO.activeSelf)
                        {
                            capsuleGO.SetActive(false);
                        }
                    }
                }
            }
        }

        public OVRSkeleton.BoneId GetCurrentStartBoneId()
        {
            switch (_skeletonType)
            {
                case OVRSkeleton.SkeletonType.HandLeft:
                case OVRSkeleton.SkeletonType.HandRight:
                    return OVRSkeleton.BoneId.Hand_Start;
                case OVRSkeleton.SkeletonType.None:
                default:
                    return OVRSkeleton.BoneId.Invalid;
            }
        }

        public OVRSkeleton.BoneId GetCurrentEndBoneId()
        {
            switch (_skeletonType)
            {
                case OVRSkeleton.SkeletonType.HandLeft:
                case OVRSkeleton.SkeletonType.HandRight:
                    return OVRSkeleton.BoneId.Hand_End;
                case OVRSkeleton.SkeletonType.None:
                default:
                    return OVRSkeleton.BoneId.Invalid;
            }
        }

        private OVRSkeleton.BoneId GetCurrentMaxSkinnableBoneId()
        {
            switch (_skeletonType)
            {
                case OVRSkeleton.SkeletonType.HandLeft:
                case OVRSkeleton.SkeletonType.HandRight:
                    return OVRSkeleton.BoneId.Hand_MaxSkinnable;
                case OVRSkeleton.SkeletonType.None:
                default:
                    return OVRSkeleton.BoneId.Invalid;
            }
        }

        public int GetCurrentNumBones()
        {
            switch (_skeletonType)
            {
                case OVRSkeleton.SkeletonType.HandLeft:
                case OVRSkeleton.SkeletonType.HandRight:
                    return GetCurrentEndBoneId() - GetCurrentStartBoneId();
                case OVRSkeleton.SkeletonType.None:
                default:
                    return 0;
            }
        }

        public int GetCurrentNumSkinnableBones()
        {
            switch (_skeletonType)
            {
                case OVRSkeleton.SkeletonType.HandLeft:
                case OVRSkeleton.SkeletonType.HandRight:
                    return GetCurrentMaxSkinnableBoneId() - GetCurrentStartBoneId();
                case OVRSkeleton.SkeletonType.None:
                default:
                    return 0;
            }
        }
    }

    
}
