using SoftHand;
using UnityEngine;

public class OVRHandMover : MonoBehaviour
{
    //[SerializeField] OVRHandsManager _hands;
    [SerializeField] Transform _leftHandTransform, _rightHandTransform;

    private void Update()
    {
       // if (!_hands) return;

        if (_leftHandTransform && HandsCore.GetHandTrackingDataProvider(Enums.HandTrackingDataProvider.Oculus).IsReliable(Enums.Handedness.Left))
        {
            Pose pose = HandsCore.GetHandTrackingDataProvider(Enums.HandTrackingDataProvider.Oculus).GetLastReliableRootPose(Enums.Handedness.Left);
            _leftHandTransform.position = pose.position;
            _leftHandTransform.rotation = pose.rotation;

        }

        if (_rightHandTransform && HandsCore.GetHandTrackingDataProvider(Enums.HandTrackingDataProvider.Oculus).IsReliable(Enums.Handedness.Right))
        {
            Pose pose = HandsCore.GetHandTrackingDataProvider(Enums.HandTrackingDataProvider.Oculus).GetLastReliableRootPose(Enums.Handedness.Right);
            _rightHandTransform.position = pose.position;
            _rightHandTransform.rotation = pose.rotation;
        }
    }
}
