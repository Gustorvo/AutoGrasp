using UnityEngine;

namespace SoftHand
{
    public class OVRBoneTrackingData
    {
        public OVRSkeleton.BoneId Id { get; set; }
        public short ParentBoneIndex { get; set; }
        public Pose BonePose { get; private set; } // world pose
        public Pose LocalBonePose { get; private set; }
        public float DistanceToParent { get; private set; }
        private bool IsRight { get; set; }


        public void UpdatePose(Quaternion childLocalRotation, Pose parentWorldPose, float rootScale, bool wristIsParent)
        {
            Vector3 newWorldPosition;
            if (wristIsParent)
            {
                newWorldPosition = parentWorldPose.position + parentWorldPose.rotation * LocalBonePose.position * rootScale;
            }
            else
            {
                Vector3 localForward = IsRight ? parentWorldPose.right : -parentWorldPose.right;
                newWorldPosition = parentWorldPose.position + localForward * DistanceToParent * rootScale;
            }
            Quaternion newWorldRotation = parentWorldPose.rotation * childLocalRotation;
            BonePose = new Pose(newWorldPosition, newWorldRotation);
            LocalBonePose = new Pose(LocalBonePose.position, childLocalRotation);
        }

        internal void Setup(Pose childPose, Pose parentPose, bool isRightHand)
        {
            DistanceToParent = Vector3.Distance(childPose.position, parentPose.position);
            BonePose = childPose;
            LocalBonePose = new Pose(childPose.position - parentPose.position, childPose.rotation);
            IsRight = isRightHand;
        }
    }
}