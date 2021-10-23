using SoftHand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRHandMover : MonoBehaviour
{
    //[SerializeField] OVRHandsManager _hands;
    [SerializeField] Transform _leftHandTransform, _rightHandTransform;

    private void Update()
    {
       // if (!_hands) return;

        if (_leftHandTransform && HandTrackingDataProvider.Instance.IsHandReliable(Enums.Handedness.Left))
        {
            Pose pose = HandTrackingDataProvider.Instance.GetPalmPose(Enums.Handedness.Left);
            _leftHandTransform.position = pose.position;
            _leftHandTransform.rotation = pose.rotation;

        }

        if (_rightHandTransform && HandTrackingDataProvider.Instance.IsHandReliable(Enums.Handedness.Right))
        {
            Pose pose = HandTrackingDataProvider.Instance.GetPalmPose(Enums.Handedness.Right);
            _rightHandTransform.position = pose.position;
            _rightHandTransform.rotation = pose.rotation;
        }
    }
}
