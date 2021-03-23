using Leap;
using Leap.Unity;
using Leap.Unity.Encoding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        public static void Decode(this VectorHand vHand, Hand intoHand, int num)
        {
            int boneIdx = 0;
            Vector3 prevJoint = Vector3.zero;
            Vector3 nextJoint = Vector3.zero;
            Quaternion boneRot = Quaternion.identity;

            // Fill fingers.
            for (int fingerIdx = 0; fingerIdx < 5; fingerIdx++)
            {
                for (int jointIdx = 0; jointIdx < 4; jointIdx++)
                {
                    boneIdx = fingerIdx * 4 + jointIdx;
                    prevJoint = vHand.jointPositions[fingerIdx * 5 + jointIdx];
                    nextJoint = vHand.jointPositions[fingerIdx * 5 + jointIdx + 1];

                    if ((nextJoint - prevJoint).normalized == Vector3.zero)
                    {
                        // Thumb "metacarpal" slot is an identity bone.
                        boneRot = Quaternion.identity;
                    }
                    else
                    {
                        boneRot = Quaternion.LookRotation(
                                    (nextJoint - prevJoint).normalized,
                                    Vector3.Cross((nextJoint - prevJoint).normalized,
                                                  (fingerIdx == 0 ?
                                                    (vHand.isLeft ? -Vector3.up : Vector3.up)
                                                   : Vector3.right)));
                    }

                    // Convert to world space from palm space.
                    nextJoint = VectorHand.ToWorld(nextJoint, vHand.palmPos, vHand.palmRot);
                    prevJoint = VectorHand.ToWorld(prevJoint, vHand.palmPos, vHand.palmRot);
                    boneRot = vHand.palmRot * boneRot;

                    intoHand.GetBone(boneIdx).Fill(
                      prevJoint: prevJoint.ToVector(),
                      nextJoint: nextJoint.ToVector(),
                      center: ((nextJoint + prevJoint) / 2f).ToVector(),
                      direction: (vHand.palmRot * Vector3.forward).ToVector(),
                      length: (prevJoint - nextJoint).magnitude,
                      width: 0.01f,
                      type: (Bone.BoneType)jointIdx,
                      rotation: boneRot.ToLeapQuaternion());
                }
                intoHand.Fingers[fingerIdx].Fill(
                  frameId: -1,
                  handId: (vHand.isLeft ? 0 : 1),
                  fingerId: fingerIdx,
                  timeVisible: 10f,// Time.time, <- This is unused and main thread only
                  tipPosition: nextJoint.ToVector(),
                  direction: (boneRot * Vector3.forward).ToVector(),
                  width: 1f,
                  length: 1f,
                  isExtended: true,
                  type: (Finger.FingerType)fingerIdx);
            }

            // Fill arm data.
            intoHand.Arm.Fill(VectorHand.ToWorld(new Vector3(0f, 0f, -0.3f), vHand.palmPos, vHand.palmRot).ToVector(),
                            VectorHand.ToWorld(new Vector3(0f, 0f, -0.055f), vHand.palmPos, vHand.palmRot).ToVector(),
                            VectorHand.ToWorld(new Vector3(0f, 0f, -0.125f), vHand.palmPos, vHand.palmRot).ToVector(),
                            Vector.Zero,
                            0.3f,
                            0.05f,
                            (vHand.palmRot).ToLeapQuaternion());

            // Finally, fill hand data.
            var palmPose = new Leap.Unity.Pose(vHand.palmPos, vHand.palmRot);
            // var wristPos = ToWorld(new Vector3(0f, -0.015f, -0.065f), palmPos, palmRot);
            var wristPos = (palmPose * VectorHand.tweakWristPosition).position;
            intoHand.Fill(
              frameID: -1,
              id: (vHand.isLeft ? 0 : 1),
              confidence: 1f,
              grabStrength: 0.5f,
              grabAngle: 100f,
              pinchStrength: 0.5f,
              pinchDistance: 50f,
              palmWidth: 0.085f,
              isLeft: vHand.isLeft,
              timeVisible: 1f,
              fingers: null /* already uploaded finger data */,
              palmPosition: vHand.palmPos.ToVector(),
              stabilizedPalmPosition: vHand.palmPos.ToVector(),
              palmVelocity: Vector3.zero.ToVector(),
              palmNormal: (vHand.palmRot * Vector3.down).ToVector(),
              rotation: (vHand.palmRot.ToLeapQuaternion()),
              direction: (vHand.palmRot * Vector3.forward).ToVector(),
              wristPosition: wristPos.ToVector()
            );            
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

    }


}