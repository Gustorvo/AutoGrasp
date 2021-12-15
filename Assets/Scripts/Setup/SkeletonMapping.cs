using System.Collections.Generic;
using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand
{
    public class SkeletonMapping : MonoBehaviour
    {
        [SerializeField]
        protected Handedness _skeletonType = Handedness.None;

        public List<Transform> CustomBones { get { return _customBones_V2; } }
        public bool IsInitialized => _customBones_V2.Count > 0;
        public Handedness SkeletonType => _skeletonType;
        public BoneId CurrentStartBoneId => BoneId.Hand_Start;
        public BoneId CurrentEndBoneId => BoneId.Hand_End;


        [HideInInspector, SerializeField]
        private List<Transform> _customBones_V2 = new List<Transform>(new Transform[(int)BoneId.Hand_End]);

        //#if UNITY_EDITOR

        private static readonly string[] _fbxHandSidePrefix = { "l_", "r_" };
        private static readonly string _fbxHandBonePrefix = "b_";
        // private static readonly string _boneOffsetPrefix = "off_";
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


        public static string FbxBoneNameFromBoneIndex(Handedness skeletonType, BoneId bi)
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
        //#endif  
        public static int GetNumOfJointsInFinger(int fingerId)
        {
            switch (fingerId)
            {
                case 0: // thumb
                case 4: // pinky
                    return 4;
                case 1: // index
                case 2: // middle
                case 3: // ring
                    return 3;

                default:
                    return 0;
            }
        }
        public static Finger GetFinger(BoneId bi)
        {
            switch ((int)bi)
            {
                case int n when n == 20 || n >= 2 && n <= 5:
                    return Finger.Thumb;
                case int n when n == 21 || n >= 6 && n <= 8:
                    return Finger.Index;
                case int n when n == 22 || n >= 9 && n <= 11:
                    return Finger.Middle;
                case int n when n == 23 || n >= 12 && n <= 14:
                    return Finger.Ring;
                case int n when n == 24 || n >= 15 && n <= 18:
                    return Finger.Pinky;

                default: return Finger.Invalid;
            }
        }

        public static int GetFingerIndexFromBoneId(int bi)
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
            int fingerIndex = GetFingerIndexFromBoneId(bi);
            if (fingerIndex <= 4)
                return fingerIndex + (int)BoneId.Hand_MaxSkinnable;
            return fingerIndex;
        }

        public static string GetJointName(Finger finger, int index)
        {
            int[] joints = new int[5] { 4, 3, 3, 3, 4 }; // thumb-3, index-3, middle-3, ring-3, pinky-4
            int sum = index; // sum is index of the joint
            for (int i = 0; i < (int)finger; i++)
            {
                sum += joints[i];
            }
            int OVRIndex = sum + (int)BoneId.Hand_Thumb0; // sum + 2 
            return ((BoneId)OVRIndex).ToString();

        }

#if UNITY_EDITOR
        [ContextMenu("Automap bones by name")]
        public void TryAutoMapBonesByName()
        {
            BoneId start = CurrentStartBoneId;
            BoneId end = CurrentEndBoneId;
            Handedness skeletonType = SkeletonType;
            if (start != BoneId.Invalid && end != BoneId.Invalid)
            {
                for (int bi = (int)start; bi < (int)end; ++bi)
                {
                    string fbxBoneName = FbxBoneNameFromBoneIndex(skeletonType, (BoneId)bi);
                    Transform t = transform.FindChildRecursive(fbxBoneName);

                    if (t != null)
                    {
                        _customBones_V2[(int)bi] = t;
                    }
                }
            }
        }

        [ContextMenu("Setup capsule colliders")]
        public void InitializeCapsules()
        {
            BoneId start = BoneId.Hand_Thumb0;
            BoneId end = BoneId.Hand_MaxSkinnable;
            if (start != BoneId.Invalid && end != BoneId.Invalid)
            {
                for (int i = (int)start; i < (int)end; ++i)
                {
                    float radius = 0.007f;
                    Transform nextBoneTransform = CustomBones[i + 1];
                    int curFingerIndex = GetFingerIndexFromBoneId(i);
                    int nextBoneFingerIndex = GetFingerIndexFromBoneId(i + 1);
                    if (curFingerIndex != nextBoneFingerIndex)
                        nextBoneTransform = CustomBones[GetFingerTipIndexFromBoneId(i)];
                    GameObject go = CustomBones[i].gameObject;

                    CapsuleCollider capsule = go.TryGetComponent<CapsuleCollider>(out capsule) ? capsule : UnityEditor.Undo.AddComponent<CapsuleCollider>(go);
                    capsule.direction = 0;
                    capsule.radius = radius;
                    capsule.height = (CustomBones[i].position - nextBoneTransform.position).magnitude + capsule.radius;
                    capsule.center = new Vector3(capsule.height / 2f - capsule.radius, 0f, 0f);
                    if (SkeletonType == Handedness.Left)
                        capsule.center *= -1;
                }
            }
        }

        [ContextMenu("Add (if missing) Art body components to all bones")]
        public void InitializeArticulationBodies()
        {
            BoneId start = BoneId.Hand_Thumb0;
            BoneId end = BoneId.Hand_MaxSkinnable;
            if (start != BoneId.Invalid && end != BoneId.Invalid)
            {
                for (int i = (int)start; i < (int)end; ++i)
                {
                    GameObject go = CustomBones[i].gameObject;
                    if (go != null && go.GetComponent<ArticulationBody>() == null)
                    {
                        UnityEditor.Undo.AddComponent<ArticulationBody>(go);
                    }
                }
            }
        }

#endif
    }
}


