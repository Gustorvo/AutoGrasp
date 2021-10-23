using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftHand
{
    public class Enums
    {
        public enum Handedness
        {
            None = -1,
            Left = 0,
            Right = 1
        }

        //public enum FingerBoneId
        //{
        //    Invalid = -1,
        //    Trapezium = 0,
        //    Metacarpal = 1,
        //    Proximal = 2,
        //    Intermediate = 3,
        //    Distal = 4
        //}

        public enum Finger
        {
            Invalid = -1,
            Thumb = 0,
            Index = 1,
            Middle = 2,
            Ring = 3,
            Pinky = 4
        }


        public enum DriveEnabled
        {
            None = 0,
            Xdrive = 1,
            Ydrive = 2,
            Zdrive = 4


        }

        public enum ArticulationDriveType
        {
            xDrive,
            yDrive,
            zDrive
        }

        /// <summary>
        /// Alignment of center of mass. Applicable to capsule collider
        /// </summary>
        public enum COMAlignment
        {
            beginning,
            center,
            end
        }
    }
}