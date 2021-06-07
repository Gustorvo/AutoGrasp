using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftHand
{
    public class Enums
    {
        public enum Handedness
        {
            Left,
            Right
        }

        public enum FingerBoneId
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

        public enum Drive
        {
            Invalid = -1,
            Xdrive = 1,
            Ydrive = 2,
            Zdrive = 3
        }
    }
}