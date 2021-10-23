using UnityEngine;
using System.Collections.Generic;
using static SoftHand.Enums;
using SoftHand.Extensions;
using System;

namespace SoftHand
{
    public class ArticulatedHandsController : MonoBehaviour
    {

        [SerializeField] ArticulatedHand _rightHand;
        [SerializeField] ArticulatedHand _leftHand;
        [SerializeField, Tooltip("Max distance threshold above which the articulated hand will be teleported (to match the target hand)")]
        float _teleportOnDistance = 0.35f;

        [Header("Angular (Rotational) force settings:")]
        [SerializeField] bool _rotate = false;
        [Range(0, 1)] public float _angularForceWeight = 1;

        [Range(0f, 30f), SerializeField, Tooltip("Frequency is the speed of convergence. If damping is 1, frequency is the 1/time taken to reach ~95% of the target value. i.e. a frequency of 6 will bring you very close to the target within 1/6 seconds")]
        float _frequency = 6f;
        [Range(0f, 10f), SerializeField, Tooltip("Damping = 1: the system is critically damped. Damping > 1: the system is over damped(sluggish), damping < 1: the system is under damped (oscillating a little")]
        float _damping = 1f;

        [Header("Linear force settings:")]
        public bool _move = false;
        [Tooltip("Applying linear forces to every joint will also apply a torque on the palm (root of articulation body). This is equivalent to AddForceAtPosition() method")]
        public bool _applyForceToEveryJoint = false;
        [Range(0, 1), SerializeField] public float _linearForceWeight = 1;
        [Range(0, 100), SerializeField, Tooltip("Converts the distance remaining to the target velocity - if too low, the articulation body slows down early and takes a long time to stop. If too high, it may overshoot")]
        float _toVelocity = 55;
        [Range(0, 200), SerializeField, Tooltip("Max speed the articulation body will reach when moving")]
        float _maxVelocity = 100;
        [Range(0, 200), SerializeField, Tooltip("Limits the force applied to the articulation body in order to avoid excessive acceleration (and instability)")]
        float _maxForce = 100;
        [Range(0, 50), SerializeField, Tooltip("Sets the feedback amount: if too low, the articulation body stops before the target point; if too high, it may overshoot and oscillate")]
        float _gain = 20;

        public static ArticulatedHandsController Instance { get; private set; } // singelton
        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

           // _rightHand.OnInitialized += TeleportToTarget;
        }

        private void TeleportToTarget(ArticulatedHand hand)
        {
            if (hand.TryCheckSphereToTarget(hand.LastReliablePose, out Pose newPose))
            {               
                hand.Teleport(newPose);

                // TODO: force hand to fist when teleporting
            }
        }

        private void Update()
        {
            if (!_rotate && !_move)
            {
                //if (_leftHand.initialized)
                //    _leftHand.palmRigidBody.isKinematic = true;
                //if (_rightHand.initialized && _rightHand.palmRigidBody != null)
                //    _rightHand.palmRigidBody.isKinematic = true;
            }
        }

        private void FixedUpdate()
        {
            // Move palm
            SetPalmPositionAndRotation(_leftHand);
            SetPalmPositionAndRotation(_rightHand);

            // Move fingers
            SetFingersRotations(_leftHand);
            SetFingersRotations(_rightHand);

        }


        /// <summary>
        /// This fixes a bug in articulation body (resets when stuck). Issue is not reported yet!
        /// TODO: Add better method description, report issue to unity
        /// </summary>
        /// <param name="hand"></param>
        private void ResetIfOverLimit(ArticulatedHand hand)
        {
            if (!hand.Initialized)
                return;
            foreach (var finger in hand.Fingers)
            {
                for (int i = 0; i < finger.joints.Length; i++)
                {
                    if (finger.joints[i].IsOvershooting(out Vector3 overshoot))
                    {
                        finger.joints[i].ResetToLimits(overshoot);
                        //Debug.Log($"Resetting to its limits on {finger.joints[i].jointName}");
                    }
                }
            }
        }

        /// <summary>
        /// TODO: Add summary 
        /// </summary>
        /// <param name="hand"></param>
        private void SetFingersDriveRotationsAllAtOnce(ArticulatedHand hand)
        {
            ResetIfOverLimit(hand);
            if (!hand.Initialized || !hand.IsTrackingReliable)
                return;
            List<float> driveTargets = new List<float>();
            List<int> startIndexes = new List<int>();
            hand.Palm.GetDriveTargets(driveTargets);
            hand.Palm.GetDofStartIndices(startIndexes);
            hand.Palm.SetDriveRotations(ref hand.jointBodies, ref hand.targetRotations, ref startIndexes, ref driveTargets);
        }



        private void SetFingersRotations(ArticulatedHand hand)
        {
            if (!hand.Initialized || !hand.IsTrackingReliable)
                return;
            
            ResetIfOverLimit(hand); //  <---- should not be here...

            // update finger joints' rotations
            foreach (var finger in hand.Fingers)
            {
                for (int i = 0; i < finger.joints.Length; i++)
                {
                    Quaternion targetRotaion = Quaternion.identity;

                    if (finger.type == Finger.Thumb && finger.joints[i].index == 0)
                        continue; // because art. body components tend to be unstable when its chain > 4 (bug ??), we want to fix the bone (humb trapezium bone - thumb0) and skip its rotation

                    //since thumb0 is fixed (se above), we want its rotaion to be applied to the next bone in the chain (thumb1)
                    if (finger.type == Finger.Thumb && finger.joints[i].index == 1)
                    {
                        // calculate rotation for thumb's second joint (metacarpal bone) and
                        // combine 2 rotations into 1 (thumb0 + thumb1), 
                        targetRotaion = Quaternion.Inverse(finger.joints[i - 1].body.anchorRotation) * // exclude thumb0 anchor rotaion
                        finger.joints[i - 1].targetPose.rotation *  // include thumb0 target rotaion
                        finger.joints[i].targetPose.rotation; // include thumb1 target rotaion
                    }
                    // calculate rotations for rest of the joints
                    else
                        targetRotaion = finger.joints[i].targetPose.rotation;
                    Vector3 targetRotatioInReducedSpace = Vector3.zero;
                    finger.joints[i].body.SetDriveTargetRotation(targetRotaion, out targetRotatioInReducedSpace);

                    // accumulate finger rotations for the last N frames in order to find the avarage
                    // we're not using it since it has side effect: tracking latency increases and becomes noticable when N>2
                    //if (finger.joints[i].meshTarget && hand.bufferFull)
                    //{
                    //    var pose = GetAverageSmoothPose(finger.joints[i].poseBuffer);
                    //    finger.joints[i].meshTarget.SetPositionAndRotation(pose.position, pose.rotation);
                    //}

                    // TODO: report this bug to unity.
                    // art body sometimes stuck
                    if (finger.joints[i].statsData.actualTravelledRatio > 400f) // joint is stuck!                  
                    {
                        finger.ResetJoints(); // reset not just ONE but ALL joints in this chain
                        UnityEngine.Debug.LogWarning($"Resetting {finger.type} finger on {hand.Handedness} hand");
                        continue;
                    }

                }
            }
        }



        private void SetPalmPositionAndRotation(ArticulatedHand hand)
        {
            if (hand && hand.Initialized)
            {
                if (_applyForceToEveryJoint)
                {
                    Vector3 accumulatedForce = Vector3.zero;
                    Vector3 accumulatedForcePosition = Vector3.zero;
                    foreach (var finger in hand.Fingers)
                    {
                        for (int i = 0; i < finger.joints.Length; i++)
                        {
                            Vector3 force = finger.joints[i].body.CalculateLinearForce(finger.joints[i].targetPose.position, _toVelocity, _maxVelocity, _maxForce, _gain);
                            hand.Palm.AddForceAtPosition(force * _linearForceWeight * hand.PerBoneMass, finger.joints[i].body.transform.position); // moves articulation body

                        }
                    }

                }

                if (_move)
                {
                    Vector3 linearForce = hand.Palm.CalculateLinearForce(hand.LastReliablePose.position, _toVelocity, _maxVelocity, _maxForce, _gain);
                    float mass = _applyForceToEveryJoint ? hand.PalmMass : hand.TotalMass;
                    hand.Palm.AddForce(linearForce * _linearForceWeight * mass); // moves articulation body


                    //if (hand.palmMeshBody && hand.bufferFull)
                    //{
                    //    UnityEngine.Pose pose = GetAverageSmoothPose(hand.palmPosedBuffer);
                    //    hand.palmMeshBody.position = pose.position;
                    //}
                }


                if (_rotate)
                {
                    Vector3 angularForce = hand.Palm.CalculateRequiredTorque(hand.LastReliablePose.rotation, _frequency, _damping);
                    hand.Palm.AddTorque(angularForce * _angularForceWeight);

                    //if (hand.palmMeshBody && hand.bufferFull)
                    //{
                    //    UnityEngine.Pose pose = GetAverageSmoothPose(hand.palmPosedBuffer);
                    //    hand.palmMeshBody.rotation = pose.rotation;
                    //}
                }



                // Teleport hand if it gets too far from the target
                if (hand.DistanceToTargetSqr > _teleportOnDistance * _teleportOnDistance)
                {                   
                    TeleportToTarget(hand);
                }
            }
        }
    }

}