using Leap;
using Leap.Unity;
using Leap.Unity.Encoding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoftHand;
using static SoftHand.Enums;
using Finger = SoftHand.Enums.Finger;
//    XR_HAND_JOINT_PALM_EXT = 0,
//    XR_HAND_JOINT_WRIST_EXT = 1,
//    XR_HAND_JOINT_THUMB_METACARPAL_EXT = 2,
//    XR_HAND_JOINT_THUMB_PROXIMAL_EXT = 3,
//    XR_HAND_JOINT_THUMB_DISTAL_EXT = 4,
//    XR_HAND_JOINT_THUMB_TIP_EXT = 5,
//    XR_HAND_JOINT_INDEX_METACARPAL_EXT = 6,
//    XR_HAND_JOINT_INDEX_PROXIMAL_EXT = 7,
//    XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT = 8,
//    XR_HAND_JOINT_INDEX_DISTAL_EXT = 9,
//    XR_HAND_JOINT_INDEX_TIP_EXT = 10,
//    XR_HAND_JOINT_MIDDLE_METACARPAL_EXT = 11,
//    XR_HAND_JOINT_MIDDLE_PROXIMAL_EXT = 12,
//    XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT = 13,
//    XR_HAND_JOINT_MIDDLE_DISTAL_EXT = 14,
//    XR_HAND_JOINT_MIDDLE_TIP_EXT = 15,
//    XR_HAND_JOINT_RING_METACARPAL_EXT = 16,
//    XR_HAND_JOINT_RING_PROXIMAL_EXT = 17,
//    XR_HAND_JOINT_RING_INTERMEDIATE_EXT = 18,
//    XR_HAND_JOINT_RING_DISTAL_EXT = 19,
//    XR_HAND_JOINT_RING_TIP_EXT = 20,
//    XR_HAND_JOINT_LITTLE_METACARPAL_EXT = 21,
//    XR_HAND_JOINT_LITTLE_PROXIMAL_EXT = 22,
//    XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT = 23,
//    XR_HAND_JOINT_LITTLE_DISTAL_EXT = 24,
//    XR_HAND_JOINT_LITTLE_TIP_EXT = 25,
//    XR_HAND_JOINT_MAX_ENUM_EXT = 0x7FFFFFFF
namespace LeapExtensions
{
    public static class HandExtensions
    {
        #region Transform extensions
        public static Vector3 FromVector(this Vector3 v)
        {
            return new Vector3() { x = v.x, y = v.y, z = v.z };
        }

        public static Vector3 FromFlippedXVector(this Vector3 v)
        {
            return new Vector3() { x = -v.x, y = v.y, z = v.z };
        }

        public static Vector3 FromFlippedZVector(this Vector3 v)
        {
            return new Vector3() { x = v.x, y = v.y, z = -v.z };
        }

        public static Quaternion FromQuat(this Quaternion q)
        {
            return new Quaternion() { x = q.x, y = q.y, z = q.z, w = q.w };
        }

        public static Quaternion FromFlippedXQuat(this Quaternion q)
        {
            return new Quaternion() { x = q.x, y = -q.y, z = -q.z, w = q.w };
        }

        public static Quaternion FromFlippedZQuat(this Quaternion q)
        {
            return new Quaternion() { x = -q.x, y = -q.y, z = q.z, w = q.w };
        }
        #endregion

        #region Articulation body extension

        public static ArticulationBody SetupForBone(this ArticulationBody body, OVRSkeleton.BoneId bi)
        {
            var bone = GetFingerBone(bi);
            var finger = GetFinger(bi);
            if (bone == FingerBone.Invalid || finger == Finger.Invalid)
                return null;

            body.anchorPosition = new Vector3(0f, 0f, 0f);
            body.anchorRotation = Quaternion.identity;
            body.mass = 3f;

            if (bone == FingerBone.Trapezium || (bone == FingerBone.Metacarpal && finger == Finger.Pinky))
            {
                //return locked AB
                body.jointType = ArticulationJointType.FixedJoint;
                return body;
            }
            else if ((bone == FingerBone.Proximal && finger != Finger.Thumb) || (bone == FingerBone.Metacarpal && finger == Finger.Thumb))
            {
                //return spherical (2 dof)
                body.jointType = ArticulationJointType.SphericalJoint;
                body.swingZLock = ArticulationDofLock.LimitedMotion;
                body.swingYLock = ArticulationDofLock.LimitedMotion;
                body.twistLock = ArticulationDofLock.LockedMotion;
                body.anchorRotation = Quaternion.Euler(90f, 180f, 0f);

                ArticulationDrive yDrive = new ArticulationDrive()
                {
                    stiffness = 100f,// * _strength,
                    forceLimit = 1000f,// * _strength,
                    damping = 3f,
                    lowerLimit = -15f,
                    upperLimit = 85f
                };
                ArticulationDrive zDrive = new ArticulationDrive()
                {
                    stiffness = 100f,// * _strength,
                    forceLimit = 1000f,// * _strength,
                    damping = 6f,
                    lowerLimit = -15f,
                    upperLimit = 15f
                };
                body.zDrive = zDrive;
                body.yDrive = yDrive;

                return body;
            }
            // return 1 dof
            body.jointType = ArticulationJointType.RevoluteJoint;
            body.twistLock = ArticulationDofLock.LimitedMotion;
            ArticulationDrive xDrive = new ArticulationDrive()
            {
                stiffness = 100f,// * _strength,
                forceLimit = 1000f,// * _strength,
                damping = 3f,
                lowerLimit = -15f,
                upperLimit = 115f
            };
            body.xDrive = xDrive;
            body.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
            return body;
        }

        public static FingerBone GetFingerBone(this OVRSkeleton.BoneId bi)
        {
            switch ((int)bi)
            {
                case 2:
                    return FingerBone.Trapezium;
                case 3:
                case 15:
                    return FingerBone.Metacarpal;
                case 4:
                case 6:
                case 9:
                case 12:
                case 16:
                    return FingerBone.Proximal;
                case 5:
                case 8:
                case 11:
                case 14:
                case 18:
                    return FingerBone.Distal;
                case 7:
                case 10:
                case 13:
                case 17:
                    return FingerBone.Intermediate;

                default: return FingerBone.Invalid;
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

        #endregion

    }
}