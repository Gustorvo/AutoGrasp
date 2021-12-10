using SoftHand.Extensions;
using UnityEngine;
using SoftHand.Interfaces;
using System.Collections.Generic;

namespace SoftHand
{
    public class FingerMover : IFingerMover
    {
        //public IArticulatedHand Hand => hand;
        //private readonly IArticulatedHand hand;
        public List<IJoint> HandJoints => joints;
        private readonly List<IJoint> joints;

        public FingerMover(List<IJoint> joints)
        {
           // this.hand = hand;
            this.joints = joints;
        }

        public void MoveFingers()
        {
            for (int i = 0; i < HandJoints.Count; i++)
            {
                Quaternion targetRotaion = Quaternion.identity;

                if (HandJoints[i].FingerIndex == 0 && HandJoints[i].Index == 0)
                    continue; // because art. body components tend to be unstable when its chain > 4 (bug ??), we want to fix the joint (thumb trapezium bone - thumb0) and skip its rotation

                //since thumb0 is fixed (se above), we want its rotaion to be applied to the next joint in the chain (thumb1)
                if (HandJoints[i].FingerIndex == 0 && HandJoints[i].Index == 1)
                {
                    // calculate rotation for thumb's second joint (metacarpal bone) and
                    // combine 2 rotations into 1 (thumb0 + thumb1), 
                    targetRotaion = Quaternion.Inverse(HandJoints[i - 1].ArticulationBody.anchorRotation) * // exclude thumb0 anchor rotaion
                    HandJoints[i - 1].TargetData.Rotation *  // include thumb0 target rotaion
                    HandJoints[i].TargetData.Rotation; // include thumb1 target rotaion
                }
                // calculate rotations for rest of the joints
                else
                    targetRotaion = HandJoints[i].TargetData.Rotation;

                HandJoints[i].ArticulationBody.SetDriveTargetRotation(targetRotaion);
            }
           // hand.OnDriveTargetsSet?.Invoke();
        }       
    }
}