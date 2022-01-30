using UnityEngine;
using static SoftHand.ArticulationBodySettings;

namespace SoftHand
{
    public static class ArticulationBodySetupExtensions
    {
        public static ArticulationBody ApplySettings(this ArticulationBody body, ArticulatedJointSettings settings)
        {
            //reset to defaults
            //body.mass = 0.05f;
            //body.useGravity = false;
            //body.linearDamping = 0.05f;
            //body.angularDamping = 0.05f;
            //body.jointFriction = 0.05f;
            //body.anchorPosition = Vector3.zero;
            //body.anchorPosition = new Vector3(0f, 0f, 0f);
            //body.parentAnchorRotation = Quaternion.identity;
            //body.anchorRotation = Quaternion.identity;
            //body.jointType = ArticulationJointType.SphericalJoint;
            //body.twistLock = ArticulationDofLock.FreeMotion;
            //body.swingYLock = ArticulationDofLock.FreeMotion;
            //body.swingZLock = ArticulationDofLock.FreeMotion;


            //apply settings
            body.anchorPosition = settings.anchorPosition;
            body.anchorRotation = Quaternion.Euler(settings.anchorRotation);
            body.parentAnchorRotation = Quaternion.Euler(settings.parentAnchorRotation);
            body.jointType = settings.jointType;
            body.twistLock = settings.motions.twist;
            body.swingYLock = settings.motions.swingY;
            body.swingZLock = settings.motions.swingZ;

            //body.xDrive = body.SetupDrive(settings.xDriveSettings);
            //body.yDrive = body.SetupDrive(settings.yDriveSettings);
            //body.zDrive = body.SetupDrive(settings.zDriveSettings);
            return body;
        }

        public static ArticulationDrive SetupDrive(this ArticulationBody body, ArticulationDriveSettings settings)
        {
            var drive = new ArticulationDrive()
            {
                stiffness = settings.motor.stiffness,
                forceLimit = settings.motor.forceLimit,
                damping = settings.motor.damping,
                target = 0f,
                targetVelocity = 0f,
                lowerLimit = settings.minMaxLimits.x,
                upperLimit = settings.minMaxLimits.y
               // lowerLimit = settings.limits.lowerLimit,
               // upperLimit = settings.limits.upperLimit
            };
            return drive;
        }

    }
}