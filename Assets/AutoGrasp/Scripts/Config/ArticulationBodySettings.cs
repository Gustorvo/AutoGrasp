using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand
{
    public class ArticulationBodySettings
    {

        [Serializable]
        public class ArticulationBodyPhysicsSettings
        {
            [Tooltip("For physics stability, the palm mass shuold be x5 of bone mass")]
            public float mass = 0.05f;
            public bool useGravity = false;
            [Tooltip("Coefficient that controls the linear slow down")]
            public float linearDamping = 0.05f;
            [Tooltip("Coefficient that controls rotational slow down")]
            public float angularDamping = 0.05f;
            [Tooltip("Coefficient that controls the energy loss caused by friction in the joint")]
            public float jointFriciton = 0.05f;
            // [Range(0, 5)] public float dampMultiplier = 1f;
            public CollisionDetectionMode CollisionDetection;

            public float maxAngularVelocity = 1.75f; // default is 7
            public float maxLinearVelocity = 1.5f;
            public float maxDepenetrationVelocity = 1.5f;
            // [Range(0, 5)] public float velMultiplier = 1f;
        }

        [Serializable]
        public class MotorSettings
        {
            [Tooltip("The stiffness of the spring that attracts the joint to the target")]
            public float stiffness = 100f;
            [Tooltip("The damping of the spring that attracts the joint to the target")]
            public float damping = 3f;
            [Tooltip("The maximum amount of force or torque this drive can produce")]
            public float forceLimit = 100f;
            [Tooltip("The target velocity this drive aims to reach")]
            public float targetVelocity = 0f;
            [Tooltip("Every first joint in finger will will be dampten twice as much")]
            public bool doubleDampingForFirstJoint = true;
        }

        [Serializable]
        public class MotionSettings
        {
            public ArticulationDofLock swingY = ArticulationDofLock.FreeMotion;
            public ArticulationDofLock swingZ = ArticulationDofLock.FreeMotion;
            public ArticulationDofLock twist = ArticulationDofLock.FreeMotion;

            public MotionSettings() { }
            public MotionSettings(ArticulationDofLock xMotion)
            {
                twist = xMotion;
            }

            public MotionSettings(ArticulationDofLock xMotion, ArticulationDofLock yMotion, ArticulationDofLock zMotion)
            {
                swingY = yMotion;
                swingZ = zMotion;
                twist = xMotion;
            }
        }

        [Serializable]
        public class ArticulationDriveSettings
        {          
            [MinMaxSlider(180f, 180f)]
            public Vector2 minMaxLimits;
            public ArticulationDriveType type;
           // public DriveLimitSettings limits;
            public MotorSettings motor;

            public ArticulationDriveSettings(ArticulationDriveType type)
            {
                this.type = type;
            }

            public ArticulationDriveSettings()
            {
              // limits = new DriveLimitSettings();
                motor = new MotorSettings();
            }

        }

        //[Serializable]
        //public class DriveLimitSettings
        //{
        //    [MinMaxSlider(Min = -180f, Max = 180f)]
        //    public Vector2 minMaxLimits;
        //    [HideInInspector] public float lowerLimit;
        //    [HideInInspector] public float upperLimit;
        //}
        [Serializable]
        public class ArticulatedFingerSettings
        {
            [HideInInspector]
            public string name = string.Empty;
            public Finger type = Finger.Invalid;
            public List<ArticulatedJointSettings> joints;

            public ArticulatedFingerSettings(Finger type)
            {
                this.name = type.ToString();
                this.type = type;
            }

            public void SetLimits(JointLimitsPreset preset)
            {
                for (int i = 0; i < joints.Count; i++)
                {
                    var limits = preset.GetDriveLimits(joints[i].name);

                    joints[i].xDriveSettings.minMaxLimits = limits.xDriveLimits;
                    joints[i].yDriveSettings.minMaxLimits = limits.yDriveLimits;
                    joints[i].zDriveSettings.minMaxLimits = limits.zDriveLimits;
                   //  joints[i].yDriveSettings.limits = limits.yLimits;
                    // joints[i].zDriveSettings.limits = limits.zLimits;
                }
            }

            public void SetJointType(ArticulationJointType type)
            {
                for (int i = 0; i < joints.Count; i++)
                {
                    joints[i].jointType = type;
                }

            }
            public void SetMotorSettings(MotorSettings settings)
            {
                for (int i = 0; i < joints.Count; i++)
                {
                    joints[i].xDriveSettings.motor = settings;
                    joints[i].yDriveSettings.motor = settings;
                    joints[i].zDriveSettings.motor = settings;
                }
            }

            public void SetDriveLocks(MotionSettings driveMotions)
            {
                for (int i = 0; i < joints.Count; i++)
                {
                    joints[i].motions = driveMotions;
                }
            }
        }

        [Serializable]
        public class ArticulatedJointSettings
        {
            public int index;
            public string name;
            public ArticulationJointType jointType = ArticulationJointType.FixedJoint;
            public Vector3 anchorPosition = Vector3.zero;
            public Vector3 anchorRotation = Vector3.zero;
            public Vector3 parentAnchorRotation = Vector3.zero;
            public MotionSettings motions = new MotionSettings();
            public ArticulationDriveSettings xDriveSettings = new ArticulationDriveSettings(ArticulationDriveType.xDrive);
            public ArticulationDriveSettings yDriveSettings = new ArticulationDriveSettings(ArticulationDriveType.yDrive);
            public ArticulationDriveSettings zDriveSettings = new ArticulationDriveSettings(ArticulationDriveType.zDrive);

            public ArticulatedJointSettings(int index)
            {
                this.index = index;
            }

            internal void ApplySettings(ArticulationJointType jointType, Vector3 anchorRotation, MotionSettings driveLocks)
            {
                this.jointType = jointType;
                this.anchorRotation = anchorRotation;
                this.motions = driveLocks;
            }
            internal void ApplyDrivesSettings(ArticulationDriveSettings xDriveSettings, ArticulationDriveSettings yDriveSettings, ArticulationDriveSettings zDriveSettings)
            {
                this.xDriveSettings = xDriveSettings;
                this.yDriveSettings = yDriveSettings;
                this.zDriveSettings = zDriveSettings;
            }
            internal void ApplyDriveSettings(ArticulationDriveSettings driveSettings)
            {
                ArticulationDriveType type = driveSettings.type;
                if (type == ArticulationDriveType.xDrive)
                    xDriveSettings = driveSettings;
                else if (type == ArticulationDriveType.yDrive)
                    yDriveSettings = driveSettings;
                else
                    zDriveSettings = driveSettings;
            }

            internal void ApplyDriveLimitSettings(ArticulationDriveType type, ArticulationDriveSettings settings)
            {
                if (type == ArticulationDriveType.xDrive)
                    xDriveSettings.minMaxLimits = settings.minMaxLimits;
                else if (type == ArticulationDriveType.yDrive)
                    yDriveSettings.minMaxLimits = settings.minMaxLimits;
                else
                    zDriveSettings.minMaxLimits = settings.minMaxLimits;
            }
            internal void ApplyDriveMotorSettings(ArticulationDriveType type, MotorSettings motorSettings)
            {
                if (type == ArticulationDriveType.xDrive)
                    xDriveSettings.motor = motorSettings;
                else if (type == ArticulationDriveType.yDrive)
                    yDriveSettings.motor = motorSettings;
                else
                    zDriveSettings.motor = motorSettings;
            }
        }
    }
}