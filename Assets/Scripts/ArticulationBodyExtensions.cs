using System.Linq;
using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand
{
    public static class ArticulationBodyExtensions
    {
        public static void AddForce(this ArticulationBody ab, Vector3 force, ForceMode mode)
        {
            switch (mode)
            {
                case ForceMode.Force:
                    ab.AddForce(force);
                    break;
                case ForceMode.Impulse:
                    ab.AddForce(force / Time.fixedDeltaTime);
                    break;
                case ForceMode.Acceleration:
                    ab.AddForce(force * ab.mass);
                    break;
                case ForceMode.VelocityChange:
                    ab.AddForce(force * ab.mass / Time.fixedDeltaTime);
                    break;
            }
        }

        public static void SetCoMInHierarchy(this ArticulationBody ab)
        {
            //Find the center of mass of a collection of art bodies
            var _bodies = ab.transform.GetComponentsInChildren<ArticulationBody>().ToList();
            Vector3 CoM = Vector3.zero;
            float c = 0f;
            foreach (ArticulationBody body in _bodies)
            {
                //body.centerOfMass = LocalCoM(body, ab.transform.position);
                if (body.transform.TryGetComponent<CapsuleCollider>(out CapsuleCollider capsuleCol))
                    body.centerOfMass = capsuleCol.center;

                // body.inertiaTensor = Vector3.one;
                //body.ResetCenterOfMass();
                //body.ResetInertiaTensor();
                CoM += body.worldCenterOfMass * body.mass;
                c += body.mass;
            }
            CoM /= c;
            if (ab.transform.TryGetComponent<BoxCollider>(out BoxCollider boxCol))
                ab.centerOfMass = boxCol.center;
            //ab.centerOfMass = CoM + new Vector3(-0.2f, 0, 0);
            // ab.ResetCenterOfMass();
            // ab.ResetInertiaTensor();

            Vector3 LocalCoM(ArticulationBody ab, Vector3 worldCoM)
            {
                return ab.transform.InverseTransformPoint(worldCoM - ab.transform.position);
            }
        }

        public static Vector3[] GetDriveLimits(int index)
        {
            JointLimits jointLimits = (JointLimits)Object.FindObjectOfType(typeof(JointLimits));
            if (jointLimits != null)
            {
                return jointLimits.GetDriveLimits(index);
            }
            return null;
        }

        public static void SetupRootBody(this ArticulationBody body, bool immovable)
        {
            if (!body.isRoot)
                return;
            //body.anchorPosition = new Vector3(0f, 0f, 0f);
            //body.anchorRotation = Quaternion.identity;
            //body.mass = mass;
            body.angularDamping = 0.05f;
            body.linearDamping = 0.05f;

            //body.maxAngularVelocity *= 0.5f;
           // body.maxLinearVelocity *= 0.5f;
            body.immovable = immovable;
        }

        public static ArticulationBody SetupForBone(this ArticulationBody body, OVRSkeleton.BoneId bi, out bool inverted, out bool isYFlipped)
        {
            isYFlipped = false;
            inverted = true;
            var limits = GetDriveLimits((int)bi - 2);
            if (limits != null)
            { // invert
                inverted = true;
                limits[0] *= -1f;
                limits[1] *= -1f;
            }
            var bone = GetFingerBone(bi);
            var finger = GetFinger(bi);
            if (bone == FingerBoneId.Invalid || finger == Finger.Invalid)
                return null;
            float stiffness = 150;
            float damping = 10; // we should regulate damping based on % done on target. 99% done = 100% damping, 0% done = 0% damping
            float forceLimit = 1f;
            float mass = 0.05f;
            float targetVelocity = 0;

            body.anchorPosition = new Vector3(0f, 0f, 0f);
            body.anchorRotation = Quaternion.identity;
            body.mass = mass;
            body.angularDamping = 0.05f;
            //body.linearDamping = 0.1f;
            //body.jointFriction = 10;
            //body.maxAngularVelocity *= 0.1f; //too small value will missrotate root body
            // body.maxJointVelocity *= 0.1f;
            // body.maxLinearVelocity *= 0.1f;
            //body.solverIterations = 255;
            //body.solverVelocityIterations = 255;

            ArticulationDrive xDrive = new ArticulationDrive();
            ArticulationDrive yDrive = new ArticulationDrive();
            ArticulationDrive zDrive = new ArticulationDrive();

            if (bone == FingerBoneId.Metacarpal && finger == Finger.Pinky /*|| bone == FingerBoneId.Trapezium*/)
            {
                //return locked AB
                body.jointType = ArticulationJointType.FixedJoint;
                body.twistLock = ArticulationDofLock.LockedMotion;
                body.swingYLock = ArticulationDofLock.LockedMotion;
                body.swingZLock = ArticulationDofLock.LockedMotion;
                //body.mass = 0f;
                return body;
            }

            //return spherical (3 dof)
            // since spherecal joint if buggy when all 3 dofs are enabled,
            // we limiting it to just 2 dofs for now
            else if ((bone == FingerBoneId.Trapezium || bone == FingerBoneId.Metacarpal) && finger == Finger.Thumb)
            {
                if (limits != null)
                { // invert again to set back to normal
                    inverted = false;
                    limits[0] *= -1;
                    limits[1] *= -1;
                }
                body.jointType = ArticulationJointType.SphericalJoint;
                body.swingZLock = ArticulationDofLock.LimitedMotion;
                body.swingYLock = ArticulationDofLock.LimitedMotion;
                body.twistLock = ArticulationDofLock.LimitedMotion;

                //body.anchorRotation = Quaternion.Euler(0f, 180f, 0f);
                xDrive = new ArticulationDrive()
                {
                    stiffness = stiffness,// * _strength,
                    forceLimit = forceLimit,// * _strength,
                    damping = damping,
                    targetVelocity = targetVelocity,
                    lowerLimit = limits != null ? limits[0].x : -20f,
                    upperLimit = limits != null ? limits[1].x : 20f
                };
                yDrive = new ArticulationDrive()
                {
                    stiffness = stiffness,// * _strength,
                    forceLimit = forceLimit,// * _strength,
                    damping = damping,
                    targetVelocity = targetVelocity,
                    lowerLimit = limits != null ? limits[0].y : -20f,
                    upperLimit = limits != null ? limits[1].y : 20f
                };
                zDrive = new ArticulationDrive()
                {
                    stiffness = stiffness,// * _strength,
                    forceLimit = forceLimit,// * _strength,
                    damping = damping,
                    targetVelocity = targetVelocity,
                    lowerLimit = limits != null ? limits[0].z : -20f,
                    upperLimit = limits != null ? limits[1].z : 20f
                };
                // dissable 3d axis due to unstable solver in spherical joint
                body.xDrive = xDrive;
                body.yDrive = yDrive;
                body.zDrive = zDrive;
                return body;
            }
            else if (bone == FingerBoneId.Proximal && finger != Finger.Thumb || bone == FingerBoneId.Metacarpal && finger == Finger.Thumb)
            {
                //return spherical (2 dof)
                body.jointType = ArticulationJointType.SphericalJoint;
                body.swingZLock = ArticulationDofLock.LimitedMotion;
                body.swingYLock = ArticulationDofLock.LimitedMotion;
                body.twistLock = ArticulationDofLock.LockedMotion;
                body.anchorRotation = Quaternion.Euler(0f, 180f, 0f);

                zDrive = new ArticulationDrive()
                {
                    stiffness = stiffness,// * _strength,
                    forceLimit = forceLimit,// * _strength,
                    damping = damping,
                    targetVelocity = targetVelocity,
                    lowerLimit = limits != null ? limits[1].z : -15f,
                    upperLimit = limits != null ? limits[0].z : 85f
                };
                yDrive = new ArticulationDrive()
                {
                    stiffness = stiffness,// * _strength,
                    forceLimit = forceLimit,// * _strength,
                    damping = damping,
                    targetVelocity = targetVelocity,
                    lowerLimit = limits != null ? limits[1].y : -15f,
                    upperLimit = limits != null ? limits[0].y : 15f
                };
                body.zDrive = zDrive;
                body.yDrive = yDrive;

                return body;
            }
            // return 1 dof
            body.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
            body.jointType = ArticulationJointType.RevoluteJoint;
            body.twistLock = ArticulationDofLock.LimitedMotion;
            body.swingYLock = ArticulationDofLock.LockedMotion;
            body.swingZLock = ArticulationDofLock.LockedMotion;
            isYFlipped = true;
            xDrive = new ArticulationDrive()
            {
                stiffness = stiffness,// * _strength,
                forceLimit = forceLimit,// * _strength,
                damping = damping,
                targetVelocity = targetVelocity,
                lowerLimit = limits != null ? limits[1].z : -15f,
                upperLimit = limits != null ? limits[0].z : 115f
            };
            body.xDrive = xDrive;
            return body;
        }

        public static FingerBoneId GetFingerBone(this OVRSkeleton.BoneId bi)
        {
            switch ((int)bi)
            {
                case 2:
                    return FingerBoneId.Trapezium;
                case 3:
                case 15:
                    return FingerBoneId.Metacarpal;
                case 4:
                case 6:
                case 9:
                case 12:
                case 16:
                    return FingerBoneId.Proximal;
                case 5:
                case 8:
                case 11:
                case 14:
                case 18:
                    return FingerBoneId.Distal;
                case 7:
                case 10:
                case 13:
                case 17:
                    return FingerBoneId.Intermediate;

                default: return FingerBoneId.Invalid;
            }

        }

        public static Finger GetFinger(this OVRSkeleton.BoneId bi)
        {
            switch ((int)bi)
            {
                case int n when n == 20 || n >= 2 && n <= 5:
                    return Finger.Thumb;
                case int n when n == 21 || n >= 6 && n <= 8:
                    return Finger.Index;
                case int n when n == 22 || n >= 9 && n <= 11:
                    return Finger.Middle;
                case int n when n == 23 || n >= 12 && n <= 14:
                    return Finger.Ring;
                case int n when n == 24 || n >= 15 && n <= 18:
                    return Finger.Pinky;


                default: return Finger.Invalid;
            }
        }

    }
}