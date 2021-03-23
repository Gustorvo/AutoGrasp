
namespace SoftHandInternal
{
    using System;
    using UnityEngine;

    public class OVRToSoftHandTrackingData
    {

        public struct OVR_TRACKING_EVENT
        {
            public Int64 frame_id;
            public Int64 timestamp;
            public Int64 tracking_id;
            public UInt32 nHands;
            public IntPtr pHands; //LEAP_HAND*
            public float framerate;
        }

        public struct OVR_HEAD_POSE_EVENT
        {
            public Int64 timestamp;
            public Vector3 head_position;
            public Quaternion head_orientation;
        }

    }
    
}