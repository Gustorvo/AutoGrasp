using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "SoftHand/Create new Joint limits preset")]
    public class JointLimitsPreset : ScriptableObject
    {
        public Handedness hand = Handedness.None;
        public List<DriveMinMax> jointLimits = new List<DriveMinMax>();


        public void CreateJointList(Handedness hand, List<string> joints)
        {
            jointLimits = new List<DriveMinMax>();
            for (int i = 0; i < joints.Count; i++)
            {
                jointLimits.Add(new DriveMinMax(joints[i]));
            }            
        }

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

        [Serializable]
        public class DriveMinMax
        {
            public string id = string.Empty; // joint mane this drive limit belongs to
          
            
            [ShowIf("showX"), MinMaxSlider(-180f, 180f), AllowNesting]
            public Vector2 xDriveLimits = new Vector2();
            [ShowIf("showY"), MinMaxSlider(-180f, 180f), AllowNesting]
            public Vector2 yDriveLimits = new Vector2();
            [ShowIf("showZ"), MinMaxSlider(-180f, 180f), AllowNesting]
            public Vector2 zDriveLimits = new Vector2();

            [EnumFlags]
            public DriveEnabled type = DriveEnabled.None;
           
            private bool showX => type.HasFlag(DriveEnabled.Xdrive);
            private bool showY => type.HasFlag(DriveEnabled.Ydrive);
            private bool showZ => type.HasFlag(DriveEnabled.Zdrive);

            private float xMin, xMax;
            private float yMin, yMax;
            private float zMin, zMax;

            private Vector3 _initialRotationInReducedSpace = Vector3.zero;

            public DriveMinMax(string jointName)
            {               
                this.id = jointName;
            }

            public DriveMinMax(Vector3 initialValues, string jointName)
            {
                this.id = jointName;
                _initialRotationInReducedSpace = initialValues;
            }

            public void Set(Vector3 value)
            {
                value -= _initialRotationInReducedSpace;
                xMax = Mathf.Max(value.x, xMax);
                yMax = Mathf.Max(value.y, yMax);
                zMax = Mathf.Max(value.z, zMax);
                xMin = Mathf.Min(value.x, xMin);
                yMin = Mathf.Min(value.y, yMin);
                zMin = Mathf.Min(value.z, zMin);

                //if (value.x > xMax)
                //    xMax = value.x;
                //if (value.x < xMin)
                //    xMin = value.x;
                //if (value.y > yMax)
                //    yMax = value.y;
                //if (value.y < yMin)
                //    yMin = value.y;
                //if (value.z > zMax)
                //    zMax = value.z;
                //if (value.z < zMin)
                //    zMin = value.z;

                xDriveLimits.Set(xMin, xMax);
                yDriveLimits.Set(yMin, yMax);
                zDriveLimits.Set(zMin, zMax);

            }
        }
    }
}