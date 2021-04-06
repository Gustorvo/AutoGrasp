using UnityEngine;
using static SoftHand.Enums;

public static class ArticulationBodyExtensions
{
    public static void AddForce(this ArticulationBody ab, Vector3 force, ForceMode mode)
    {
        switch (mode)
        {
            case ForceMode.Force:
                ab.AddForce(force);
                break;
            case ForceMode.Impulse:
                ab.AddForce(force / Time.fixedDeltaTime);
                break;
            case ForceMode.Acceleration:
                ab.AddForce(force * ab.mass);
                break;
            case ForceMode.VelocityChange:
                ab.AddForce(force * ab.mass / Time.fixedDeltaTime);
                break;
        }
    } 

    public static ArticulationBody SetupForBone(this ArticulationBody body, OVRSkeleton.BoneId bi)
    {
        var bone = GetFingerBone(bi);
        var finger = GetFinger(bi);
        if (bone == Bone.Invalid || finger == Finger.Invalid)
            return null;

        body.anchorPosition = new Vector3(0f, 0f, 0f);
        body.anchorRotation = Quaternion.identity;
        body.mass = 3f;

        if (bone == Bone.Trapezium || (bone == Bone.Metacarpal && finger == Finger.Pinky))
        {
            //return locked AB
            body.jointType = ArticulationJointType.FixedJoint;
            return body;
        }
        else if ((bone == Bone.Proximal && finger != Finger.Thumb) || (bone == Bone.Metacarpal && finger == Finger.Thumb))
        {
            //return spherical (2 dof)
            body.jointType = ArticulationJointType.SphericalJoint;
            body.swingZLock = ArticulationDofLock.LimitedMotion;
            body.swingYLock = ArticulationDofLock.LimitedMotion;
            body.twistLock = ArticulationDofLock.LockedMotion;
            body.anchorRotation = Quaternion.Euler(90f, 180f, 0f);

            ArticulationDrive yDrive = new ArticulationDrive()
            {
                stiffness = 100f,// * _strength,
                forceLimit = 1000f,// * _strength,
                damping = 3f,
                lowerLimit = -15f,
                upperLimit = 85f
            };
            ArticulationDrive zDrive = new ArticulationDrive()
            {
                stiffness = 100f,// * _strength,
                forceLimit = 1000f,// * _strength,
                damping = 6f,
                lowerLimit = -15f,
                upperLimit = 15f
            };
            body.zDrive = zDrive;
            body.yDrive = yDrive;

            return body;
        }
        // return 1 dof
        body.jointType = ArticulationJointType.RevoluteJoint;
        body.twistLock = ArticulationDofLock.LimitedMotion;
        ArticulationDrive xDrive = new ArticulationDrive()
        {
            stiffness = 100f,// * _strength,
            forceLimit = 1000f,// * _strength,
            damping = 3f,
            lowerLimit = -15f,
            upperLimit = 115f
        };
        body.xDrive = xDrive;
        body.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
        return body;
    }

    public static Bone GetFingerBone(this OVRSkeleton.BoneId bi)
    {
        switch ((int)bi)
        {
            case 2:
                return Bone.Trapezium;
            case 3:
            case 15:
                return Bone.Metacarpal;
            case 4:
            case 6:
            case 9:
            case 12:
            case 16:
                return Bone.Proximal;
            case 5:
            case 8:
            case 11:
            case 14:
            case 18:
                return Bone.Distal;
            case 7:
            case 10:
            case 13:
            case 17:
                return Bone.Intermediate;

            default: return Bone.Invalid;
        }

    }

    public static Finger GetFinger(this OVRSkeleton.BoneId bi)
    {
        switch ((int)bi)
        {
            case int n when n == 20 || n >= 2 && n <= 5:
                return Finger.Thumb;
            case int n when n == 21 || n >= 6 && n <= 8:
                return Finger.Index;
            case int n when n == 22 || n >= 9 && n <= 11:
                return Finger.Middle;
            case int n when n == 23 || n >= 12 && n <= 14:
                return Finger.Ring;
            case int n when n == 24 || n >= 15 && n <= 18:
                return Finger.Pinky;


            default: return Finger.Invalid;
        }
    }   

}