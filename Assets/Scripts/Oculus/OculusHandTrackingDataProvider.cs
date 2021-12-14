using UnityEngine;
using UnityEngine.Assertions;
using static SoftHand.Enums;
using static OVRSkeleton;
using SoftHand.Interfaces;
using SoftHand.Core;
using System;
using System.Collections.Generic;

namespace SoftHand.Oculus
{
    //public abstract class HandTrackingBase : MonoBehaviour, IHandTrackingDataProvider
    //{
    //    public abstract TrackingConfidence GetFingerConfidence(Handedness handedness, Finger finger);
    //    public abstract Quaternion[] GetJointsRotations(Handedness hand);
    //    public abstract int GetNumberOfJoints();
    //    public abstract Pose GetRootPose(Handedness hand);
    //    public abstract bool IsHandReliable(Handedness hand);
    //}

    public class OculusHandTrackingDataProvider : HandTrackingBase, IHandTrackingDataProvider
    {
        [SerializeField] GameObject _jointVizPrefab;

        [SerializeField] OVRHand _leftHand = null;
        [SerializeField] OVRHand _rightHand = null;
        [SerializeField] OVRSkeleton _leftHandSkeleton = null;
        [SerializeField] OVRSkeleton _rightHandSkeleton = null;
        private int _numOfBones => GetNumberOfJoints();
        private const int NUM_OF_HANDS = 2;

        public override bool IsInitialized { get; set; }
        public override HandTrackingDataProvider Type => HandTrackingDataProvider.Oculus;

        public bool JointPositionsProvided { get; } = true; // via OVRSkeleton.cs
        public bool JointRotationsProvided { get; } = true;

        private int _firstBone = (int)OVRPlugin.BoneId.Hand_Thumb0;
        private int _lastBone = (int)OVRPlugin.BoneId.Hand_Pinky3;

        // [0]- left hand, [1]- right hand

        private OVRHand[] _hands = new OVRHand[NUM_OF_HANDS];
        private OVRSkeleton[] _handSkeleton = new OVRSkeleton[NUM_OF_HANDS];
        private Pose[][] _bonePoses = new Pose[NUM_OF_HANDS][];
        private Pose[] _wristPose = new Pose[NUM_OF_HANDS];
        private float[] _rootScale = new float[NUM_OF_HANDS] { 1f, 1f };
        private IOVRSkeletonDataProvider[] _handDataProvider = new IOVRSkeletonDataProvider[NUM_OF_HANDS];
        private SkeletonPoseData[] _handPoseData = new SkeletonPoseData[NUM_OF_HANDS];
        private readonly Quaternion _wristFixupRotation = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);
        private Pose[][] _pastFrames = new Pose[NUM_OF_HANDS][]; // 'history' frames
        private int _pastFrameCounter = 0;
        private Pose[,] _lastReliableFrame = new Pose[NUM_OF_HANDS, 1];

        protected OVRPlugin.Skeleton2 _skeleton = new OVRPlugin.Skeleton2();
        protected List<OVRBoneData> _bones;
        public IList<OVRBoneData> Bones { get; protected set; }
        //private bool IsInitialized = false;

        private void Start()
        {
            if (ShouldInitialize())
            {
                Initialize();
            }
        }

        private void Awake()
        {
            _bones = new List<OVRBoneData>();
            Bones = _bones.AsReadOnly();
            //if (Instance && Instance != this)
            //{
            //    Destroy(this);
            //    return;
            //}
            //Instance = this;

            Assert.IsNotNull(_leftHand);
            Assert.IsNotNull(_rightHand);
            // Assert.IsNotNull(_leftHandSkeleton);
            //  Assert.IsNotNull(_rightHandSkeleton);

            _hands[0] = _leftHand;
            _hands[1] = _rightHand;
            _handSkeleton[0] = _leftHandSkeleton;
            _handSkeleton[1] = _rightHandSkeleton;

            _handDataProvider[0] = _leftHand.GetComponent<IOVRSkeletonDataProvider>();
            _handDataProvider[1] = _rightHand.GetComponent<IOVRSkeletonDataProvider>();
            // _boneRotations[0] = new Quaternion[_numOfBones];
            //  _boneRotations[1] = new Quaternion[_numOfBones];

            _pastFrames[0] = new Pose[512];
            _pastFrames[1] = new Pose[512];

        }
        private void Initialize()
        {
            if (OVRPlugin.GetSkeleton2(OVRPlugin.SkeletonType.HandRight, ref _skeleton))
            {
                _wristPose[0] = new Pose(Vector3.zero, Quaternion.identity /* _wristFixupRotation*/);
                _wristPose[1] = new Pose(Vector3.zero, Quaternion.identity);// * _wristFixupRotation);
                InitializeBones();
                IsInitialized = true;
            }
        }
        private bool ShouldInitialize()
        {
            if (IsInitialized)
            {
                return false;
            }

#if UNITY_EDITOR
            return OVRInput.IsControllerConnected(OVRInput.Controller.Hands);
#else
			return true;
#endif

        }

        List<GameObject> _jointVizList = new List<GameObject>();
        // private GameObject _bonesGO;

        private void InitializeBones()
        {
            _bonePoses[0] = new Pose[_skeleton.NumBones];
            _bonePoses[1] = new Pose[_skeleton.NumBones];

            //_bonesGO = new GameObject("Bones");
            //_bonesGO.transform.SetParent(transform, false);
            //_bonesGO.transform.localPosition = Vector3.zero;
            //_bonesGO.transform.localRotation = Quaternion.identity;


            if (_bones == null || _bones.Count != _skeleton.NumBones)
            {
                _bones = new List<OVRBoneData>(new OVRBoneData[_skeleton.NumBones]);
                Bones = _bones.AsReadOnly();
            }

            // pre-populate bones list before attempting to apply bone hierarchy
            for (int i = 0; i < _bones.Count; ++i)
            {
                OVRBoneData bone = _bones[i] ?? (_bones[i] = new OVRBoneData());
                bone.Id = (OVRSkeleton.BoneId)_skeleton.Bones[i].Id;
                bone.ParentBoneIndex = _skeleton.Bones[i].ParentBoneIndex;

                Pose pose = new Pose(
                _skeleton.Bones[i].Pose.Position.FromFlippedXVector3f(),
               _skeleton.Bones[i].Pose.Orientation.FromFlippedXQuatf());

                Pose parentPose;
                var index = (OVRPlugin.BoneId)_bones[i].ParentBoneIndex;
                if (index == OVRPlugin.BoneId.Invalid || index == 0)
                {
                    //  parentPose = new Pose(_bonesGO.transform.position, _bonesGO.transform.rotation);
                    parentPose = _wristPose[1];
                    if (i < 2)
                    {
                        pose = parentPose;
                    }
                }
                else
                {
                    parentPose = _bones[_bones[i].ParentBoneIndex].BonePose;
                }
                Pose childPose = GetChildPoseRelativeToParent2(parentPose, pose);
                _bones[i].Setup(childPose, parentPose, true);
                //add viz
                var joint = Instantiate(_jointVizPrefab, childPose.position, childPose.rotation);
                joint.name = BoneLabelFromBoneId((SkeletonType)OVRPlugin.SkeletonType.HandRight, bone.Id);
                _jointVizList.Add(joint);

            }

            #region remove
            //for (int i = 0; i < _bones.Count; ++i)
            //{
            //    if ((OVRPlugin.BoneId)_bones[i].ParentBoneIndex == OVRPlugin.BoneId.Invalid)
            //    {
            //        Pose parentPose = new Pose(_bonesGO.transform.position, _bonesGO.transform.rotation);
            //        Pose childPose = GetChildPoseRelativeToParent2(parentPose, _bones[i].LocalBonePose);
            //        _bones[i].Setup(childPose, parentPose);
            //    }
            //    else
            //    {
            //        Pose parentPose = _bones[_bones[i].ParentBoneIndex].LocalBonePose;
            //        _bones[i].LocalBonePose = GetChildPoseRelativeToParent2(parentPose, _bones[i].LocalBonePose);
            //    }
            //    var joint = Instantiate(_jointVizPrefab, _bones[i].LocalBonePose.position, _bones[i].LocalBonePose.rotation);
            //    joint.name = BoneLabelFromBoneId((SkeletonType)OVRPlugin.SkeletonType.HandRight, _bones[i].Id);
            //    _jointVizList.Add(joint);
            //}
            #endregion // remve regio
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (ShouldInitialize())
            {
                Initialize();
            }
#endif
            FetchHandTrackingDataFromOVR();
            _pastFrameCounter++;
            if (_pastFrameCounter >= _pastFrames[0].Length)
            {
                _pastFrameCounter = 0;
            }
        }

        private void FetchHandTrackingDataFromOVR()
        {
            for (int i = 0; i < 2; i++)
            {
                _handPoseData[i] = _handDataProvider[i].GetSkeletonPoseData();
                //   _handSkeleton[i]
                ExtractBonePosesFromHandTrackingData(_handPoseData[i], ref _bonePoses[i], ref _wristPose[i], ref _rootScale[i], ref _lastReliableFrame[i, 0], ref _pastFrames[i]);
            }

            UpdateVizDebug();
        }
        private void ExtractBonePosesFromHandTrackingData(SkeletonPoseData data, ref Pose[] bonePoses, ref Pose wristPose, ref float rootScale, ref Pose lastReliablePose, ref Pose[] history)
        {
            if (!data.IsDataValid) return;
            // get wrist position and rotation
            wristPose.rotation = data.RootPose.Orientation.FromFlippedZQuatf() * _wristFixupRotation;
            wristPose.position = data.RootPose.Position.FromFlippedZVector3f();
            rootScale = data.RootScale;

            // history
            history[_pastFrameCounter] = wristPose;

            // store laste reliables
            if (data.IsDataHighConfidence)
            {
                lastReliablePose = wristPose;
            }

            //get finger bones rotations
            //for (int i = _firstBone; i < _lastBone + 1; ++i)
            for (int i = 0; i < bonePoses.Length; i++)
            {
                //  bonePoses[i - _firstBone].position = data. //??? Oculus hand tracking API doesn't provide us with bone positions
                bonePoses[i].rotation = data.BoneRotations[i].FromFlippedXQuatf();
            }
        }


        private void UpdateVizDebug()
        {
            if (!IsInitialized) return;

            for (int i = 0; i < _bones.Count; ++i)
            {
                var index = (OVRPlugin.BoneId)_bones[i].ParentBoneIndex;
                if (index == OVRPlugin.BoneId.Invalid || index == 0)
                {
                    Pose parentPose = _wristPose[1];
                    _bones[i].UpdatePose(_bonePoses[1][i].rotation, parentPose, _rootScale[1], true);
                }
                else
                {
                    Pose parentPose = _bones[_bones[i].ParentBoneIndex].BonePose;
                    _bones[i].UpdatePose(_bonePoses[1][i].rotation, parentPose, _rootScale[1], false);
                }
            }
            // debug update joint viz
            for (int i = 0; i < _bones.Count; ++i)
            {
                _jointVizList[i].transform.position = _bones[i].BonePose.position;
                _jointVizList[i].transform.rotation = _bones[i].BonePose.rotation;
            }
        }


        private Pose GetChildPoseRelativeToParent(Pose parentPose, Pose childPose)
        {
            Pose worldChildPose = new Pose(LocalToWorld(childPose.position, parentPose, Vector3.one), parentPose.rotation * childPose.rotation);
            Matrix4x4 parentMatrix;
            Vector3 startParentPosition = parentPose.position;
            Quaternion startParentRotationQ = parentPose.rotation;
            Vector3 startParentScale = Vector3.one;

            Vector3 startChildPosition = worldChildPose.position;
            Quaternion startChildRotationQ = worldChildPose.rotation;
            Vector3 startChildScale = Vector3.one;
            startChildPosition = DivideVectors(Quaternion.Inverse(parentPose.rotation) * (startChildPosition - startParentPosition), startParentScale);
            parentMatrix = Matrix4x4.TRS(parentPose.position, parentPose.rotation, Vector3.one);
            Vector3 position = parentMatrix.MultiplyPoint3x4(startChildPosition);
            Quaternion rotation = (parentPose.rotation * Quaternion.Inverse(startParentRotationQ)) * startChildRotationQ;
            return new Pose(position, rotation);
        }

        private Pose GetChildPoseRelativeToParent2(Pose parentPose, Pose childPose)
        {
            return new Pose(LocalToWorld(childPose.position, parentPose, Vector3.one), LocalToWorld(childPose.rotation, parentPose.rotation));
        }
        Vector3 DivideVectors(Vector3 num, Vector3 den) => new Vector3(num.x / den.x, num.y / den.y, num.z / den.z);
        Vector3 LocalToWorld(Vector3 childPosition, Pose parentPose, Vector3 parentScale) => parentPose.position + parentPose.rotation * (Vector3.Scale(childPosition, parentScale));
        Quaternion LocalToWorld(Quaternion childRot, Quaternion parentRot) => parentRot * childRot;





        #region Interface implementation

        public bool IsHandReliable(Handedness hand)
        {
            return _hands[(int)hand].IsTracked && _hands[(int)hand].HandConfidence == OVRHand.TrackingConfidence.High;
        }


        public Pose[] GetJointsPoses(Handedness hand)
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

    public class OVRBoneData
    {
        public OVRSkeleton.BoneId Id { get; set; }
        public short ParentBoneIndex { get; set; }
        public Pose BonePose { get; private set; }
        public float DistanceToParent { get; private set; }
        public Vector3 LocalPosition { get; private set; }
        private bool IsRight { get; set; }


        public void UpdatePose(Quaternion childLocalRotation, Pose parentWorldPose, float rootScale, bool wristIsParent)
        {
            Vector3 newWorldPosition;
            if (wristIsParent)
            {
                newWorldPosition = parentWorldPose.position + parentWorldPose.rotation * LocalPosition * rootScale;
            }
            else
            {
                Vector3 localForward = IsRight ? parentWorldPose.right : -parentWorldPose.right;
                newWorldPosition = parentWorldPose.position + localForward * DistanceToParent * rootScale;
            }
            Quaternion newWorldRotation = parentWorldPose.rotation * childLocalRotation;
            BonePose = new Pose(newWorldPosition, newWorldRotation);
        }

        internal void Setup(Pose childPose, Pose parentPose, bool isRightHand)
        {
            DistanceToParent = Vector3.Distance(childPose.position, parentPose.position);
            BonePose = childPose;
            LocalPosition = childPose.position - parentPose.position;
            IsRight = isRightHand;
        }
    }
}