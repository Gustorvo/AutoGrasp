using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand
{
    public class ArtBodyController : MonoBehaviour
    {
        [SerializeField] ArtDriveTargetStruct _rightHand;
        [SerializeField] ArtDriveTargetStruct _leftHand;

        private Vector3 _linearForce, _angularForce;
        private float _deltaAngle;

        [Header("Angular force values:")]
        public float alignmentSpeed = 7f;
        public float alignmentDamping = 5f;
        public bool applyCounterTorque = false;
        public bool applyTorque = false;

        [Header("Linear force values:")]
        public float _sqrDistance = 0.002f;
        public float toVel = 25f; // converts the distance remaining to the target velocity - if too low, the rigidbody slows down early and takes a long time to stop. If too high, it may overshoot
        public float maxVel = 1500f; //max speed the rigidbody will reach when moving
        public float maxForce = 1500f; // limits the force applied to the rigidbody in order to avoid excessive acceleration (and instability)
        public float gain = 60f; // sets the feedback amount: if too low, the rigidbody stops before the target point; if too high, it may overshoot and oscillate
        public bool applyCounterLinearForces = false;
        public bool applyLinearForces = false;
        private Vector3 _counterForce;

        public void Init()
        {
            // check if palmTarget and palmArtBody are assigned in the inspector
            _leftHand.initialized = _leftHand.palmTarget && _leftHand.palmArtBody;
            _rightHand.initialized = _rightHand.palmTarget && _rightHand.palmArtBody;

            // Set total mass
            SetTotalMass(ref _leftHand);
            SetTotalMass(ref _rightHand);

            SetupPhysics(ref _leftHand);
            SetupPhysics(ref _rightHand);
        }

        private void SetupPhysics(ref ArtDriveTargetStruct hand)
        {
            // increase max angualar velocity (for bigger Alignment speed to take effect.)
            if (hand.palmRigidBody != null)
            hand.palmRigidBody.maxAngularVelocity = 20f; // default is 7
        }

        private void Start()
        {
            Init();

            // TODO
            // Looks like Counter force for linea velocity doesn't make any sence any more
        }

        private void SetTotalMass(ref ArtDriveTargetStruct hand)
        {
            if (!hand.initialized)
                return;
            float artBodiesMass = 0f;
            var bodies = hand.palmArtBody.GetComponentsInChildren<ArticulationBody>()?.ToList();
            if (bodies.Count <= 1)
                return;
            //bodies.Remove(hand.palmArtBody); // remove root body since it is a base          

            // accumulate all art bodies' masses in hierarchy
            bodies.ForEach(x => artBodiesMass += x.mass);
            hand.totalMass = artBodiesMass + hand.palmRigidBody.mass;
        }

        private void FixedUpdate()
        {
            // update hand root (palm position and rotaion)
            SetPalmPositionAndRotation(_leftHand);
            SetPalmPositionAndRotation(_rightHand);

            SetFingersRotations(_leftHand);
            SetFingersRotations(_rightHand);

            // SetFingerRotationsOnReferenceObj(_leftHand);
        }

        private void SetFingerRotationsOnReferenceObj(ArtDriveTargetStruct hand)
        {
            foreach (var finger in hand.fingers)
            {
                for (int i = 0; i < finger.joints.Length; i++)
                {
                    Quaternion targetRotaion = finger.joints[i].target.localRotation;
                    Quaternion sourceRotaion = finger.joints[i].backReferenceTarget.localRotation;
                    Quaternion difference = Quaternion.Inverse(targetRotaion) * sourceRotaion;
                    Quaternion newRotation = finger.joints[i].backReferenceTarget.localRotation * Quaternion.Inverse(difference);
                    finger.joints[i].backReferenceTarget.localEulerAngles = newRotation.eulerAngles;
                }
            }
        }

        private void SetFingersRotations(ArtDriveTargetStruct hand)
        {  // update finger joints' rotations
            foreach (var finger in hand.fingers)
            {
                for (int i = 0; i < finger.joints.Length; i++)
                {
                    Quaternion targetRotaion = finger.joints[i].target.localRotation;
                    finger.joints[i].body.SetDriveRotation(targetRotaion);
                }
            }
        }

        /// <summary>
        /// This algorithm first calculates the velocity needed proportionally to the current distance to the target position,
        // then estimates the force necessary to reach the desired velocity - this way it automatically
        // accelerates during most of the time and decelerates when getting near to the target.        
        /// <param name="hand"></param>
        /// <returns></returns>
        private Vector3 CalculateLinearForce(ArtDriveTargetStruct hand)
        {
            Vector3 dist = hand.palmTarget.transform.position - hand.palmArtBody.transform.position;
            // calc a target velocity proportional to distance (clamped to max velocity)
            Vector3 tgtVel = Vector3.ClampMagnitude(toVel * dist, maxVel);
            // calculate the velocity error
            Vector3 error = tgtVel - hand.palmRigidBody.velocity;
            // calculate a force proportional to the error (clamped to maxForce)
            Vector3 linearForce = Vector3.ClampMagnitude(gain * error, maxForce);
            // accounter for body masses (hand + fingers)
            linearForce *= hand.totalMass;
            return linearForce;
        }

        // handle overshooting.
        // Calcutates forces needed to stop a moving body
        private Vector3 CalculateCounterForces(ArtDriveTargetStruct hand)
        {
            Vector3 dist = hand.palmTarget.transform.position - hand.palmArtBody.transform.position;
            // Vector3 counterForce = Vector3.zero;
            // if (dist.sqrMagnitude < _sqrDistance && applyCounterLinearForces)
            //  {
            // calculate counter Force, where F = Mass * (0 - Vel) / dTime
            Vector3 counterForce = hand.totalMass * (Vector3.zero - hand.palmRigidBody.velocity) / Time.fixedDeltaTime;
            // }
            return counterForce;
        }

        private Vector3 CalculateAngularForce(ArtDriveTargetStruct hand)
        {
            // calculate delta rotation
            Quaternion deltaRotation = Quaternion.Inverse(hand.palmRigidBody.transform.rotation) * hand.palmTarget.rotation;
            Vector3 deltaAngles = GetRelativeAngles(deltaRotation.eulerAngles);
            Vector3 worldDeltaAngles = hand.palmRigidBody.transform.TransformDirection(deltaAngles);
            Vector3 angularForce = hand.totalMass * alignmentSpeed * worldDeltaAngles - alignmentDamping * hand.palmRigidBody.angularVelocity * hand.totalMass;
            Vector3 GetRelativeAngles(Vector3 angles)
            {
                Vector3 relativeAngles = angles;
                if (relativeAngles.x > 180f)
                    relativeAngles.x -= 360f;
                if (relativeAngles.y > 180f)
                    relativeAngles.y -= 360f;
                if (relativeAngles.z > 180f)
                    relativeAngles.z -= 360f;
                return relativeAngles;
            }
            return angularForce;
        }

        private Vector3 GetCounterAngularForce(ArtDriveTargetStruct hand)
        {
            // handle overshoot for torque forces
           // float deltaAngle = Quaternion.Angle(hand.palmRigidBody.transform.rotation, hand.palmTarget.rotation);
            // apply counter torque forces, where  T = I * (0 - A) / dTime
            //  Vector3 counterTorque = Vector3.zero;
            //if (deltaAngle <= 5f && applyCounterTorque)
            //{
            return hand.palmRigidBody.inertiaTensorRotation * (Vector3.zero - hand.palmRigidBody.angularVelocity) / Time.fixedDeltaTime;         
            //}
        }

        private void SetPalmPositionAndRotation(ArtDriveTargetStruct hand)
        {
            if (hand.initialized)
            {
                Vector3 limearForce = CalculateLinearForce(hand);
                Vector3 angularForce = CalculateAngularForce(hand);

                // Apply linear and angular forces
                if (applyLinearForces) hand.palmRigidBody.AddForce(limearForce);
                if (applyTorque) hand.palmRigidBody.AddTorque(angularForce);




                ////Fix the hand if it gets into a bad situation by teleporting and holding in place until its bad velocities disappear
                //if (Vector3.Distance(rootBody.transform.position, data.HandWorldPosition) > 1.0f)
                //{
                //    if (_bodyToMove != null)
                //    {
                //        //_bodyToMove.isKinematic = true;
                //        //_bodyToMove.transform.position = data.HandWorldPosition;
                //        //_bodyToMove.transform.rotation = data.HandWorldRotation;
                //        //_bodyToMove.velocity = Vector3.zero;
                //        //_bodyToMove.angularVelocity = Vector3.zero;
                //    }

                //    rootBody.immovable = true;
                //    rootBody.TeleportRoot(data.HandWorldPosition, data.HandWorldRotation);
                //    rootBody.velocity = Vector3.zero;
                //    rootBody.angularVelocity = Vector3.zero;
                //    _lastFrameTeleport = Time.frameCount;

                //    foreach (var collider in _colliders) collider.enabled = false;
                //    for (int i = 0; i < bodies.Count; i++)
                //    {
                //        //_articulationBodies[i].jointVelocity   = new ArticulationReducedSpace(0f, 0f, 0f);
                //        bodies[i].velocity = Vector3.zero;
                //        bodies[i].angularVelocity = Vector3.zero;
                //    }
                //    _ghosted = true;
                //}
                //if (Time.frameCount - _lastFrameTeleport >= 1)
                //{
                //    rootBody.immovable = false;
                //    rootBody.WakeUp();
                //    //  _bodyToMove.isKinematic = false;
                //    _bodyToMove.WakeUp();

                //    //loPolyHandRenderer.enabled = true;
                //}
                //if (Time.frameCount - _lastFrameTeleport >= 2 && _ghosted &&
                //    !Physics.CheckSphere(rootBody.worldCenterOfMass, 0.1f, _layerMask))
                //{
                //    foreach (var collider in _colliders)
                //    {
                //        collider.enabled = true;
                //        collider.isTrigger = true;
                //    }
                //    _ghosted = false;
                //}


            }
        }
    }

    [Serializable]
    public struct ArtDriveTargetStruct
    {
        public Handedness handedness;
        public ArticulationBody palmArtBody;
        public Rigidbody palmRigidBody;
        public Transform palmTarget;
        public ArtFinger[] fingers;
        public float totalMass;
        public bool initialized;
    }
    [Serializable]
    public struct ArtFinger
    {
        public Finger finger;
        public ArtDriveTargetPair[] joints;

        [Serializable]
        public struct ArtDriveTargetPair
        {
            public ArticulationBody body;
            public Transform target;
            public Transform backReferenceTarget;
            //  public Quaternion initialRotation;
        }

    }
}