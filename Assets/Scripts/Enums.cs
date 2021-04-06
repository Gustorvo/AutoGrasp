using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftHand
{
    public class Enums
    {
        public enum Bone
        {
            Invalid = -1,
            Trapezium = 0,
            Metacarpal = 1,
            Proximal = 2,
            Intermediate = 3,
            Distal = 4
        }

        public enum Finger
        {
            Invalid = -1,
            Thumb = 0,
            Index = 1,
            Middle = 2,
            Ring = 3,
            Pinky = 4
        }
        public enum FingerBoneId
        {
            Invalid = OVRPlugin.BoneId.Invalid,
            // hand bones
            Hand_Start = OVRPlugin.BoneId.Hand_Start,
            Hand_WristRoot = OVRPlugin.BoneId.Hand_WristRoot,          // root frame of the hand, where the wrist is located
            Hand_ForearmStub = OVRPlugin.BoneId.Hand_ForearmStub,        // frame for user's forearm
            Thumb_Trapezium = OVRPlugin.BoneId.Hand_Thumb0,             // thumb trapezium bone
            Thumb_Metacarpal = OVRPlugin.BoneId.Hand_Thumb1,             // thumb metacarpal bone
            Thumb_Proximal = OVRPlugin.BoneId.Hand_Thumb2,             // thumb proximal phalange bone
            Thumb_Distal = OVRPlugin.BoneId.Hand_Thumb3,             // thumb distal phalange bone
            Index_Proximal = OVRPlugin.BoneId.Hand_Index1,             // index proximal phalange bone
            Index_Intermediate = OVRPlugin.BoneId.Hand_Index2,             // index intermediate phalange bone
            Index_Distal = OVRPlugin.BoneId.Hand_Index3,             // index distal phalange bone
            Middle_Proximal = OVRPlugin.BoneId.Hand_Middle1,            // middle proximal phalange bone
            Middle_Intermediate = OVRPlugin.BoneId.Hand_Middle2,            // middle intermediate phalange bone
            Middle_Distal = OVRPlugin.BoneId.Hand_Middle3,            // middle distal phalange bone
            Ring_Proximal = OVRPlugin.BoneId.Hand_Ring1,              // ring proximal phalange bone
            Ring_Intermediate = OVRPlugin.BoneId.Hand_Ring2,              // ring intermediate phalange bone
            Ring_Distal = OVRPlugin.BoneId.Hand_Ring3,              // ring distal phalange bone
            Pinky_Metacarpal = OVRPlugin.BoneId.Hand_Pinky0,             // pinky metacarpal bone
            Pinky_Proximal = OVRPlugin.BoneId.Hand_Pinky1,             // pinky proximal phalange bone
            Pinky_Intermediate = OVRPlugin.BoneId.Hand_Pinky2,             // pinky intermediate phalange bone
            Pinky_Distal = OVRPlugin.BoneId.Hand_Pinky3,             // pinky distal phalange bone
            Hand_MaxSkinnable = OVRPlugin.BoneId.Hand_MaxSkinnable,
        }
    }
}