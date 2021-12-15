using UnityEngine;
using System.Collections.Generic;
using static SoftHand.Enums;
using System.Linq;

namespace SoftHand
{
    /// <summary>
    /// Controlls articulated hands, where movements are driven by physical (linear and angular) forces.
    /// Takes care of some bugs as well related to articulation bodies movements'
    /// </summary>
    public class ArticulatedHandsController : MonoBehaviour, IArticulatedHandsController
    {
        [SerializeField] float _onTeleportDistance = 0.3f;
        [SerializeField] bool _moveHand, _rotateHand, _stabilizeHand, _moveFingers, _allowTeleport;

        public IEnumerable<IArticulatedHand> Hands => _hands;
        private List<IBodyMover> _handMovers = new List<IBodyMover>();
        private List<IFingerMover> _fingerMovers = new List<IFingerMover>();
        private List<IBugFixable> _bugFixers = new List<IBugFixable>();
        private readonly List<IArticulatedHand> _hands = new List<IArticulatedHand>();
        private bool _isTooFarAwayFromTarget;

        private void Update()
        {
            FetchTrackingData();            
        }

        private void FixedUpdate()
        {
            MoveHands();
            if (_moveFingers) MoveFingers();
            if (_stabilizeHand) StabilizeHands();

            FixBugs();
        }

        public bool TryAdd(IArticulatedHand hand)
        {
            if (!_hands.Contains(hand))
            {
                _hands.Add(hand);
                _handMovers.Add(new HandMover(hand, hand.ForceSettings, hand.TorqueSettings));
                _fingerMovers.Add(new FingerMover(hand.Joints.ToList()));
                _bugFixers.Add(new BugFixer(hand));

                return true;
            }
            return false;
        }

        public bool TryRemove(IArticulatedHand hand)
        {
            int index = _hands.IndexOf(hand);
            if (index >= 0)
            {
                _hands.RemoveAt(index);
                _handMovers.RemoveAt(index);
                _fingerMovers.RemoveAt(index);
                _bugFixers.RemoveAt(index);
            }
            return index >= 0;
        }

        public void FetchTrackingData()
        {
            for (int i = 0; i < _hands.Count; i++)
            {
                _hands[i].UpdateData();
            }
        }
        public void MoveHands()
        {
            for (int i = 0; i < _handMovers.Count; i++)
            {
                _isTooFarAwayFromTarget = _hands[i].SqrDistanceToTarget > _onTeleportDistance * _onTeleportDistance;
                bool teleport = _allowTeleport && _isTooFarAwayFromTarget;
                if (teleport)
                {
                    Pose target = _hands[i].TargetData.Pose;
                    teleport = _hands[i].CanTeleport(target);
                    if (teleport)
                    {
                        _handMovers[i].TeleportBody(target);
                        _hands[i].OnTeleport?.Invoke();
                    }
                }

                // dont apply forces & torques if hand is teleporting
                if (!teleport &&_moveHand) _handMovers[i].MoveBody();
                if (!teleport && _rotateHand) _handMovers[i].RotateBody();


            }
        }
        public void MoveFingers()
        {
            for (int i = 0; i < _fingerMovers.Count; i++)
            {
                _fingerMovers[i].MoveFingers();
            }
        }
        private void FixBugs()
        {
            for (int i = 0; i < _bugFixers.Count; i++)
            {
                _bugFixers[i].ResetJointIfOvershooting();
                _bugFixers[i].ResetJointIfStuck();
            }
        }
        private void StabilizeHands()
        {
            // to reduce rotational inertia, we need to somehow stabilize the hand...
            // this could be done with IK system, where hand would be automatically stabilezed by the stiffness and damping properties of the attached arm (elbow's and shoulder's  articulation joints), but since we don't have any IK yet,
            // we're stabilizing by applying small forces to the palm, distributed across the body, where each impact position is the joint position and forces are being calculated based on taget joints' positions
            // this is equivalent as if adding the same ammount of force to every joint individually, but instead we add this force to the root (for simplicity reason).
            // TODO: we could theoretically average the position of impact point and apply accumulated forces at once. This approach needs further testing. For now we are skipping this since we'll no longer need this hack once we implemented functional IK system

            for (int i = 0; i < _hands.Count; i++)
            {
                for (int j = 0; j < _hands[i].Joints.Count; j++)
                {
                    Vector3 force = _hands[i].Joints[j].ArticulationBody.CalculateLinearForce(_hands[i].Joints[j].TargetData.Position, _hands[i].ForceSettings.ToVelocity, _hands[i].ForceSettings.MaxVelocity, _hands[i].ForceSettings.MaxForce, _hands[i].ForceSettings.Gain);
                    _hands[i].ArticulationBody.AddForceAtPosition(force * _hands[i].ForceSettings.LinearForceWeight * _hands[i].Joints[j].ArticulationBody.mass, _hands[i].Joints[j].ArticulationBody.worldCenterOfMass);

                }
            }
        }

        #region Unused
        //private void SetFingersDriveRotationsAllAtOnce(ArticulatedHand hand)
        //        {
        //            // Currently doesn't work due to unity bug introduced in 2020.3.01f.
        //            // if using unity version < 2020.3.01f, it might work!

        //            if (!hand.Initialized || !hand.Tracking.IsHandReliable(hand.Handedness))
        //                return;
        //            List<float> driveTargets = new List<float>();
        //            List<int> startIndexes = new List<int>();
        //            hand.ArticulationBody.GetDriveTargets(driveTargets);
        //            hand.ArticulationBody.GetDofStartIndices(startIndexes);
        //            hand.ArticulationBody.SetDriveRotations(hand.Joints.Select(x => x.ArticulationBody).ToArray(), hand._targetJointsPoseBuffer, ref startIndexes, ref driveTargets);
        //        }
        #endregion

    }
}

#region remove
//private void SetPalmPositionAndRotation(IArticulatedHand hand)
//{
//    if (hand.Initialized)
//    {
//        // to reduce rotational inertia, we need to somehow stabilize the hand
//        // this could be done with IK system, where hand would be automatically stabilezed by the stiffness and damping properties of the attached arm (elbow's and shoulder's  articulation joints), but since we don't have any IK yet,
//        // we're stabilizing by applying small forces to the palm, distributed across the body, where each impact position is the joint position and forces are being calculated based on taget joints' positions
//        // this is equivalent as if adding the same ammount of force to every joint individually, but instead we add this force to the root (for simplicity reason).
//        // TODO: we could theoretically average the position of impact point and apply accumulated forces at once. This approach needs further testing. For now we are skipping this since we'll no longer need those small distributed forces once we implemented functional IK system
//        if (_stabilizeByAddingSmallForces)
//        {
//            //   foreach (var finger in hand.Fingers) // TODO: use for loop
//            // {
//            for (int i = 0; i < hand.Joints.Length; i++)
//            {
//                Vector3 force = hand.Joints[i].ArticulationBody.CalculateLinearForce(hand.Joints[i].targetPose.position, linearForceSettings.ToVelocity, linearForceSettings.MaxVelocity, linearForceSettings.MaxForce, linearForceSettings.Gain);
//                // adds force to the root (palm) at the position of its joints 
//                hand.Palm.AddForceAtPosition(force * linearForceSettings.LinearForceWeight * hand.Joints[i].ArticulationBody.mass, hand.Joints[i].ArticulationBody.transform.position);
//            }
//            // }
//        }

//        if (linearForceSettings.ShouldMove)
//        {
//            Vector3 linearForce = hand.Palm.CalculateLinearForce(hand.LastReliablePose.position, linearForceSettings.ToVelocity, linearForceSettings.MaxVelocity, linearForceSettings.MaxForce, linearForceSettings.Gain);
//            float mass = _stabilizeByAddingSmallForces ? hand.Palm.mass : hand.TotalMass;
//            hand.Palm.AddForce(linearForce * linearForceSettings.LinearForceWeight * mass);
//        }


//        if (angularForceSettings.ShouldRotate)
//        {
//            Vector3 angularForce = hand.Palm.CalculateRequiredTorque(hand.LastReliablePose.rotation, angularForceSettings.Frequency, angularForceSettings.Damping);
//            hand.Palm.AddTorque(angularForce * angularForceSettings.AngularForceWeight);
//        }



//        // Teleport hand if it gets too far from the target
//        if (hand.DistanceToTargetSqr > _teleportOnDistance * _teleportOnDistance)
//        {
//            TeleportToTarget(hand);
//        }
//    }
//}
//private void SetFingersRotations(IArticulatedHand hand)
//{
//    if (!hand.Initialized || !hand.Tracking.IsHandReliable(hand.Handedness))
//        return;

//    ResetIfOverLimit(hand); //  <---- should not be here...

//    // update finger joints' rotations
//    // foreach (var finger in hand.Fingers)
//    //  {
//    for (int i = 0; i < hand.Joints.Length; i++)
//    {
//        Quaternion targetRotaion = Quaternion.identity;

//        if (hand.Joints[i].fingerIndex == 0 && hand.Joints[i].index == 0)
//            continue; // because art. body components tend to be unstable when its chain > 4 (bug ??), we want to fix the joint (thumb trapezium bone - thumb0) and skip its rotation

//        //since thumb0 is fixed (se above), we want its rotaion to be applied to the next joint in the chain (thumb1)
//        if (hand.Joints[i].fingerIndex == 0 && hand.Joints[i].index == 1)
//        {
//            // calculate rotation for thumb's second joint (metacarpal bone) and
//            // combine 2 rotations into 1 (thumb0 + thumb1), 
//            targetRotaion = Quaternion.Inverse(hand.Joints[i - 1].ArticulationBody.anchorRotation) * // exclude thumb0 anchor rotaion
//            hand.Joints[i - 1].targetPose.rotation *  // include thumb0 target rotaion
//            hand.Joints[i].targetPose.rotation; // include thumb1 target rotaion
//        }
//        // calculate rotations for rest of the joints
//        else
//            targetRotaion = hand.Joints[i].targetPose.rotation;
//        // Vector3 targetRotatioInReducedSpace = Vector3.zero;
//        hand.Joints[i].ArticulationBody.SetDriveTargetRotation(targetRotaion);



//        // TODO: report this bug to unity.
//        // art body sometimes stuck
//        if (hand.Joints[i].statsData.actualTravelledRatio > 400f) // joint is stuck!                  
//        {
//            int finger = hand.Joints[i].fingerIndex;
//            hand.Joints[i].Reset(); // reset not just ONE but ALL joints in this chain
//            UnityEngine.Debug.LogWarning($"Resetting {(Finger)finger} finger on {hand.Handedness} hand");
//            continue;
//        }
//    }
//    // }
//}

#endregion


