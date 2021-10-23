using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using static OVRSkeleton;
using static SoftHand.Enums;

[DefaultExecutionOrder(-70)]

public class HandTrackingDataProvider : MonoBehaviour
{
    [SerializeField] OVRHand _leftHand = null;
    [SerializeField] OVRHand _rightHand = null;
    public int NumberOfBones => _numOfBones;

    public static HandTrackingDataProvider Instance { get; private set; }

    private OVRHand[] _hands = new OVRHand[(int)OVRHand.Hand.HandRight + 1];
    private int _numOfBones = (int)OVRPlugin.BoneId.Hand_MaxSkinnable - (int)OVRPlugin.BoneId.Hand_Thumb0; // should be 17 bones total (19-2)
    private Quaternion[][] _boneRotations = new Quaternion[2][];
    private Pose[] _palmPoses = new Pose[2];
    private IOVRSkeletonDataProvider _leftHandDataProvider, _rightHandDataProvider;
    private SkeletonPoseData _leftHandPoseData, _rightHandPoseData;
    private readonly Quaternion _wristFixupRotation = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);



    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        Assert.IsNotNull(_leftHand);
        Assert.IsNotNull(_rightHand);

        _hands[0] = _leftHand;
        _hands[1] = _rightHand;
        _leftHandDataProvider = _leftHand.GetComponent<IOVRSkeletonDataProvider>();
        _rightHandDataProvider = _rightHand.GetComponent<IOVRSkeletonDataProvider>();
        _boneRotations[0] = new Quaternion[_numOfBones];
        _boneRotations[1] = new Quaternion[_numOfBones];
    }

    private void Update()
    {
        FetchHandPoseDataFromOVR();
    }

    private void FetchHandPoseDataFromOVR()
    {
        _leftHandPoseData = _leftHandDataProvider.GetSkeletonPoseData();
        _rightHandPoseData = _rightHandDataProvider.GetSkeletonPoseData();

        ExtractBonePosesFromHandTrackingData(_leftHandPoseData, ref _boneRotations[(int)Handedness.Left], ref _palmPoses[(int)Handedness.Left]);
        ExtractBonePosesFromHandTrackingData(_rightHandPoseData, ref _boneRotations[(int)Handedness.Right], ref _palmPoses[(int)Handedness.Right]);
    }

    private void ExtractBonePosesFromHandTrackingData(SkeletonPoseData data, ref Quaternion[] boneRotations, ref Pose wristPose)
    {
        if (!data.IsDataValid) return;
        // get wrist position and rotation
        wristPose.rotation = data.RootPose.Orientation.FromFlippedZQuatf() * _wristFixupRotation;
        wristPose.position = data.RootPose.Position.FromFlippedZVector3f();

        //get finger bones rotations
        int firstBone = (int)OVRPlugin.BoneId.Hand_Thumb0;
        int lastBone = (int)OVRPlugin.BoneId.Hand_Pinky3;

        for (int i = firstBone; i < lastBone + 1; ++i)
        {
            boneRotations[i - firstBone] = data.BoneRotations[i].FromFlippedXQuatf();
        }
    }

    public bool IsHandReliable(Handedness hand)
    {
        return _hands[(int)hand].IsTracked && _hands[(int)hand].HandConfidence == OVRHand.TrackingConfidence.High;
    }


    public Quaternion[] GetBoneRotaions(Handedness hand)
    {
        return _boneRotations[(int)hand];
    }

    public Pose GetPalmPose(Handedness hand)
    {
        return _palmPoses[(int)hand];
    }

    internal OVRHand.TrackingConfidence GetFingerConfidence(Handedness handedness, OVRHand.HandFinger type)
    {
        return _hands[(int)handedness].GetFingerConfidence(type);
    }
}
