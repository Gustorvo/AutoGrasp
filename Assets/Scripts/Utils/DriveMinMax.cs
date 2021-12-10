using NaughtyAttributes;
using SoftHand.Extensions;
using System;
using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand
{
    public partial class JointLimitsPreset
    {
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
            private ArticulationBody jointBody;
            public DriveMinMax(ArticulationBody jointBody, string jointName)
            {
                this.id = jointName;
                this.jointBody = jointBody;
               
               
                bool hasX, hasY, hasZ;
                hasX = jointBody.twistLock != ArticulationDofLock.LockedMotion && jointBody.jointType != ArticulationJointType.FixedJoint;
                hasY = jointBody.swingYLock != ArticulationDofLock.LockedMotion && jointBody.jointType != ArticulationJointType.FixedJoint;
                hasZ = jointBody.swingZLock != ArticulationDofLock.LockedMotion && jointBody.jointType != ArticulationJointType.FixedJoint;
                if (hasX) type = DriveEnabled.Xdrive;
                if (hasY) type |= DriveEnabled.Ydrive;
                if (hasZ) type |= DriveEnabled.Zdrive;
            }

            public void Set()
            {
                Vector3 targets = jointBody.GetArtBodyDriveTargets();               
                targets -= _initialRotationInReducedSpace;
                xMax = Mathf.Max(targets.x, xMax);
                yMax = Mathf.Max(targets.y, yMax);
                zMax = Mathf.Max(targets.z, zMax);
                xMin = Mathf.Min(targets.x, xMin);
                yMin = Mathf.Min(targets.y, yMin);
                zMin = Mathf.Min(targets.z, zMin);

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