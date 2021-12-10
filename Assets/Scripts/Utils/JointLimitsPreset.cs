using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "SoftHand/Create new Joint limits preset")]
    public partial class JointLimitsPreset : ScriptableObject
    {
        public Handedness hand = Handedness.None;
        public List<DriveMinMax> jointLimits = new List<DriveMinMax>(); // old
        public List<RuntimeJointLimits> runtimeJointLimits = new List<RuntimeJointLimits>(); // new
               

        public DriveMinMax GetDriveLimits(string jointName)
        {
            return jointLimits?.FirstOrDefault(x => x.id.Equals(jointName));
        }
        public DriveMinMax GetDriveLimits(int jointIndex)
        {
            if (jointLimits != null && jointLimits.Count >= jointIndex && jointIndex >= 0)
                return jointLimits[jointIndex];
            return null;
        }

        public void AddNewRecord(Handedness hand, List<DriveMinMax> jointLimitsRecord)
        {
            int timeStamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var newEntry = new RuntimeJointLimits(timeStamp, hand, jointLimitsRecord);
            runtimeJointLimits.Add(newEntry);
        }

        [Serializable]
        public class RuntimeJointLimits
        {
            public Handedness Hand = Handedness.None;
            public int TimeStamp = 0;
            public float Duration = 0; //seconds
            public List<DriveMinMax> JointLimits = new List<DriveMinMax>();
            public RuntimeJointLimits(int timeStamp, Handedness hand, List<DriveMinMax> jointLimits)
            {
                Hand = hand;
                TimeStamp = timeStamp;
                JointLimits = jointLimits;
            }

        }
    }
}