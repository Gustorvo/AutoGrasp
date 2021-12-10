using UnityEngine;
using SoftHand.Interfaces;

namespace SoftHand
{
    public class BugFixer : IBugFixable
    {
        public IArticulatedHand Hand { get; set; }

        public BugFixer(IArticulatedHand hand)
        {
            Hand = hand;
        }
        public void ResetFingerIfOvershooting()
        {
            // This fixes a bug in articulation body (resets when stuck). Issue is not reported yet!
            // TODO: Add better method description, report issue to unity

            for (int i = 0; i < Hand.Joints.Count; i++)
            {
                var joint = Hand.Joints[i];
                if (Hand.RuntimeStats.IsJointPositionOverLimit(joint, out Vector3 overlimit))
                {
                    Vector3 newPos = Hand.RuntimeStats.GetNearestJointMinMaxRange(joint, overlimit);
                    Hand.Joints[i].ForceJointToPosition(newPos);
                    Hand.RuntimeStats.ResetTravelRation(joint);
                    UnityEngine.Debug.Log($"Resetting { Hand.Joints[i].Name} to its limits");
                }
            }
        }
    }
}

