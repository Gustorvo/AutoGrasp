
namespace SoftHand
{
//using Leap;
using Leap.Unity;
using Leap.Unity.Encoding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    using static SoftHand.Enums;

    //using static SoftHand.Enums;
    //using Finger = SoftHand.Enums.Finger;
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

    public static class HandExtensions
    {
        #region Transform extensions
        public static Vector3 ToInspectorEulerVector(this Quaternion q)
        {
            return new Vector3(WrapAngle(q.eulerAngles.x), WrapAngle(q.eulerAngles.y), WrapAngle(q.eulerAngles.z));
            float WrapAngle(float angle)
            {
                angle %= 360;
                return angle > 180 ? angle - 360 : angle;
            }
        }

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
    }
}