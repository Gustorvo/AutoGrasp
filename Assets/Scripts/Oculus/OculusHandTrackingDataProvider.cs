using UnityEngine;
using UnityEngine.Assertions;
using static SoftHand.Enums;
using static OVRSkeleton;
using System.Collections.Generic;
using NaughtyAttributes;

namespace SoftHand
{

    public class OculusHandTrackingDataProvider : HandTrackingBase, IHandTrackingDataProvider
    {
        [SerializeField] bool _debugBones = false;
        [SerializeField, ShowIf("_debugBones")] GameObject _debugBonesPrefab;

        [SerializeField] OVRHand _leftHand = null;
        [SerializeField] OVRHand _rightHand = null;

        private List<GameObject>[] _debugBoneList = new List<GameObject>[NUM_OF_HANDS];
        private const int NUM_OF_HANDS = 2;

        public bool IsInitialized { get; private set; }
        List<bool> _handInitialized = new List<bool>(NUM_OF_HANDS) { false, false };
        public override HandTrackingDataProvider Type => HandTrackingDataProvider.Oculus;

        public bool JointPositionsProvided { get; } = true;
        public bool JointRotationsProvided { get; } = true;        

        // [0]- left hand, [1]- right hand

        private OVRHand[] _hands = new OVRHand[NUM_OF_HANDS];
        private Pose[][] _bonePoses = new Pose[NUM_OF_HANDS][];
        private OVRBoneTrackingData[][] _boneTrackingData = new OVRBoneTrackingData[NUM_OF_HANDS][];
        private Pose[] _wristPose = new Pose[NUM_OF_HANDS];
        private IOVRSkeletonDataProvider[] _handDataProvider = new IOVRSkeletonDataProvider[NUM_OF_HANDS];
        private SkeletonPoseData[] _handPoseData = new SkeletonPoseData[NUM_OF_HANDS];
        private readonly Quaternion _wristFixupRotation = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);
        private Pose[][] _handHistory = new Pose[NUM_OF_HANDS][]; // 'history' frames
        private const int HISTORY_CAPACITY = 512;
        private int _pastFrameCounter = 0;
        private Pose[,] _lastReliableFrame = new Pose[NUM_OF_HANDS, 1];

        protected OVRPlugin.Skeleton2[] _skeletons = new OVRPlugin.Skeleton2[NUM_OF_HANDS];


        private void Start()
        {
            if (ShouldInitialize())
            {
                Initialize();
            }
        }

        private void Awake()
        {
            Assert.IsNotNull(_leftHand);
            Assert.IsNotNull(_rightHand);
            if (_debugBones) Assert.IsNotNull(_debugBonesPrefab);

            _hands[0] = _leftHand;
            _hands[1] = _rightHand;

            _handDataProvider[0] = _leftHand.GetComponent<IOVRSkeletonDataProvider>();
            _handDataProvider[1] = _rightHand.GetComponent<IOVRSkeletonDataProvider>();

            _handHistory[0] = new Pose[HISTORY_CAPACITY];
            _handHistory[1] = new Pose[HISTORY_CAPACITY];
        }
        private void Initialize()
        {
            for (int i = 0; i < NUM_OF_HANDS; i++)
            {
                if (_handInitialized[i] == true) continue;

                if (OVRPlugin.GetSkeleton2((OVRPlugin.SkeletonType)i, ref _skeletons[i]))
                {
                    _wristPose[i] = new Pose(Vector3.zero, Quaternion.identity);
                    InitializeBones(i, _skeletons[i]);
                    _handInitialized[i] = true;
                }
            }
            IsInitialized = _handInitialized.TrueForAll(i => i == true);
        }
        private bool ShouldInitialize()
        {
            if (IsInitialized)
            {
                return false;
            }

//#if UNITY_EDITOR
            return OVRInput.IsControllerConnected(OVRInput.Controller.Hands);
//#else
//			return true;
//#endif
        }

        private void InitializeBones(int handIndex, OVRPlugin.Skeleton2 skeleton)
        {
            _bonePoses[handIndex] = new Pose[skeleton.NumBones];
            _boneTrackingData[handIndex] = new OVRBoneTrackingData[skeleton.NumBones];
            OVRBoneTrackingData[] bones = _boneTrackingData[handIndex];
            _debugBoneList[handIndex] = new List<GameObject>();
            bool isRightHand = handIndex == 1;
            for (int i = 0; i < bones.Length; ++i)
            {
                OVRBoneTrackingData bone = bones[i] ?? (bones[i] = new OVRBoneTrackingData());
                bone.Id = (OVRSkeleton.BoneId)skeleton.Bones[i].Id;
                bone.ParentBoneIndex = skeleton.Bones[i].ParentBoneIndex;

                Pose pose = new Pose(
                skeleton.Bones[i].Pose.Position.FromFlippedXVector3f(),
                skeleton.Bones[i].Pose.Orientation.FromFlippedXQuatf());

                Pose parentPose;
                var index = (OVRPlugin.BoneId)bones[i].ParentBoneIndex;
                if (index == OVRPlugin.BoneId.Invalid || index == 0)
                {
                    parentPose = _wristPose[handIndex];
                    if (i < 2) // Hand_Start and Hand_ForearmStub are not trackable and therefore have the same pose as the root (wrist)
                    {
                        pose = parentPose;
                    }
                }
                else
                {
                    parentPose = bones[bones[i].ParentBoneIndex].BonePose;
                }
                Pose childPose = GetChildPoseRelativeToParent(parentPose, pose);
                bones[i].Setup(childPose, parentPose, isRightHand);

                // visualize/debug joints
                if (_debugBones && _debugBonesPrefab != null)
                {
                    var joint = Instantiate(_debugBonesPrefab, childPose.position, childPose.rotation);
                    string handPrefix = handIndex == 1 ? "R" : "L";
                    joint.name = handPrefix + BoneLabelFromBoneId((SkeletonType)handIndex, bone.Id);
                    _debugBoneList[handIndex].Add(joint);
                }
            }
            _boneTrackingData[handIndex] = bones;           
        }

        private void Update()
        {
            if (ShouldInitialize())
            {
                Initialize();
            }

            if (!IsInitialized) return;
            FetchHandTrackingDataFromOVR();
           
            // histoty pool loop
            _pastFrameCounter++;
            if (_pastFrameCounter >= HISTORY_CAPACITY)
            {
                _pastFrameCounter = 0;
            }
        }

        private void FetchHandTrackingDataFromOVR()
        {
            for (int i = 0; i < 2; i++)
            {
                _handPoseData[i] = _handDataProvider[i].GetSkeletonPoseData();
                if (TryExtractBonePosesFromHandTrackingData(_handPoseData[i], ref _boneTrackingData[i], ref _bonePoses[i], ref _wristPose[i], ref _lastReliableFrame[i, 0], ref _handHistory[i]))
                {
                    DebugBones(_boneTrackingData[i], i);
                }
            }      
        }
        private bool TryExtractBonePosesFromHandTrackingData(SkeletonPoseData data, ref OVRBoneTrackingData[] bones, ref Pose[] bonePoses, ref Pose wristPose, ref Pose lastReliablePose, ref Pose[] history)
        {
            if (!data.IsDataValid) return false;
            // get wrist position and rotation
            wristPose.rotation = data.RootPose.Orientation.FromFlippedZQuatf() * _wristFixupRotation;
            wristPose.position = data.RootPose.Position.FromFlippedZVector3f();

            // history
            history[_pastFrameCounter] = wristPose;

            // store latest reliables
            if (data.IsDataHighConfidence)
            {
                lastReliablePose = wristPose;
            }

            // calculate position and rotaion
            for (int i = 0; i < bones.Length; ++i)
            {
                var index = (OVRPlugin.BoneId)bones[i].ParentBoneIndex;
                Quaternion rotation = data.BoneRotations[i].FromFlippedXQuatf();
                if (index == OVRPlugin.BoneId.Invalid || index == 0)
                {
                    bones[i].UpdatePose(rotation, wristPose, data.RootScale, true);
                }
                else
                {
                    Pose parentPose = bones[bones[i].ParentBoneIndex].BonePose;
                    bones[i].UpdatePose(rotation, parentPose, data.RootScale, false);
                }
            }

            // copy pose data to array for convenience           
            for (int i = 0; i < bonePoses.Length; i++)
            {
                bonePoses[i].position = bones[i].BonePose.position;
                bonePoses[i].rotation = bones[i].LocalBonePose.rotation;
            }
            return true;
        }


        private void DebugBones(OVRBoneTrackingData[] bones, int hand)
        {
            if (!_debugBones || _debugBoneList[hand].Count == 0)
                return;

            for (int i = 0; i < bones.Length; ++i)
            {
                _debugBoneList[hand][i].transform.position = bones[i].BonePose.position;
                _debugBoneList[hand][i].transform.rotation = bones[i].BonePose.rotation;
            }
        }

        private Pose GetChildPoseRelativeToParent(Pose parentPose, Pose childPose)
        {
            return new Pose(LocalToWorld(childPose.position, parentPose, Vector3.one), LocalToWorld(childPose.rotation, parentPose.rotation));
        }    
        Vector3 LocalToWorld(Vector3 childPosition, Pose parentPose, Vector3 parentScale) => parentPose.position + parentPose.rotation * (Vector3.Scale(childPosition, parentScale));
        Quaternion LocalToWorld(Quaternion childRot, Quaternion parentRot) => parentRot * childRot;


        #region Interface implementation

        public bool IsReliable(Handedness hand)
        {
            return _handInitialized[(int)hand] && _hands[(int)hand].IsTracked && _hands[(int)hand].HandConfidence == OVRHand.TrackingConfidence.High;
        }


        public Pose[] GetBonesPoses(Handedness hand)
        {
            return _bonePoses[(int)hand];
        }

        public Pose GetRootPose(Handedness hand)
        {
            return _wristPose[(int)hand];
        }

        public TrackingConfidence GetFingerConfidence(Handedness handedness, Finger finger)
        {
            return (TrackingConfidence)_hands[(int)handedness].GetFingerConfidence((OVRHand.HandFinger)finger);
        }

        public int GetNumberOfJoints()
        {
            return (int)OVRPlugin.BoneId.Hand_MaxSkinnable - (int)OVRPlugin.BoneId.Hand_Thumb0; // should be 17 bones (for the fbx model to work)
        }

        public Pose GetLastReliableRootPose(Handedness hand)
        {
            return _lastReliableFrame[(int)hand, 0];
        }
        #endregion // interface implementation
    }    
}