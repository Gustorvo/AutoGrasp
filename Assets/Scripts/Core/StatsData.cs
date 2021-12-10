using SoftHand.Extensions;
using UnityEngine;
using static SoftHand.JointLimitsPreset;
using SoftHand.Interfaces;

namespace SoftHand.Core
{
    public partial struct ArticulatedJoint
    {
        public struct StatsData : IJointStats
        {
            private readonly ArticulationBody joint;
            private readonly int _id;
           // private DriveMinMax runtimeJointMinMax; // record joints' max/min values 
            private Vector3 jointPositionLocal;
            private float jointPositionsSqrMag;
            private bool hasXDrive, hasYDrive, hasZDrive;
            private float maxJointPositionsSqrMagLocal; // the maximum possible squared magnitude of this joint in local space (in radians)
            private Vector3 lowerLimitsRad, upperLimitsRad; // in radians              
            private Vector3 lowerLimitsDeg, upperLimitsDeg; // in degrees
            private Vector3 prevJointPos;
            private Vector3 prevDriveTargets;
            private float traveledJointDistanceLocal;
            private float driveTargetTraveledDistanceLocal; // in radians
            private float actualTravelledRatio; // driveTargetTraveledDistanceLocal / traveledJointDistanceLocal. used to be ~ 1 radian (57) for a 'healthy' joint, or > 500 for a buggy one

            #region Properties

            public DriveMinMax RuntimeJointMinMax { get; private set; }

            public Vector3 JointPositionLocal { get; private set; }

            public float JointPositionsSqrMag { get; private set; }

            public int DofCount { get; private set; }

            public bool HasXDrive { get; private set; }

            public bool HasYDrive { get; private set; }

            public bool HasZDrive { get; private set; }

            public float MaxJointPositionsSqrMagLocal { get; private set; }

            public Vector3 LowerLimitsRad { get; private set; }

            public Vector3 UpperLimitsRad { get; private set; }

            public Vector3 LowerLimitsDeg { get; private set; }

            public Vector3 UpperLimitsDeg { get; private set; }

            public Vector3 PrevJointPos { get; private set; }

            public Vector3 PrevDriveTargets { get; private set; }

            public float TraveledJointDistanceLocal { get; private set; }

            public float DriveTargetTraveledDistanceLocal { get; private set; }

            public float ActualTravelledRatio { get; private set; }

            public int Id { get; }
            #endregion
            public StatsData(ArticulationBody articulationBody) : this()
            {
                 Id = articulationBody.GetInstanceID();
                joint = articulationBody;
                int dofCount = articulationBody.dofCount;

                lowerLimitsDeg.x = dofCount >= 1 ? articulationBody.xDrive.lowerLimit : 0;
                lowerLimitsDeg.y = dofCount >= 2 ? articulationBody.yDrive.lowerLimit : 0;
                lowerLimitsDeg.z = dofCount == 3 ? articulationBody.zDrive.lowerLimit : 0;

                upperLimitsDeg.x = dofCount >= 1 ? articulationBody.xDrive.upperLimit : 0;
                upperLimitsDeg.y = dofCount >= 2 ? articulationBody.yDrive.upperLimit : 0;
                upperLimitsDeg.z = dofCount == 3 ? articulationBody.zDrive.upperLimit : 0;

                lowerLimitsRad = lowerLimitsDeg * Mathf.Deg2Rad;
                upperLimitsRad = upperLimitsDeg * Mathf.Deg2Rad;

                maxJointPositionsSqrMagLocal = (lowerLimitsRad.Abs() + upperLimitsRad.Abs()).sqrMagnitude;

                hasXDrive = articulationBody.twistLock != ArticulationDofLock.LockedMotion && articulationBody.jointType != ArticulationJointType.FixedJoint;
                hasYDrive = articulationBody.swingYLock != ArticulationDofLock.LockedMotion && articulationBody.jointType != ArticulationJointType.FixedJoint;
                hasZDrive = articulationBody.swingZLock != ArticulationDofLock.LockedMotion && articulationBody.jointType != ArticulationJointType.FixedJoint;
            }           

            public void CollectsStats()
            {
                Vector3 currentDriveTargets = joint.GetArtBodyDriveTargets();
                Vector3 delta = (currentDriveTargets - prevDriveTargets).Abs();
                driveTargetTraveledDistanceLocal += delta.x + delta.y + delta.z;
                prevDriveTargets = currentDriveTargets;
                Vector3 currentJointPositionLocal = Vector3.zero;
                if (joint.dofCount >= 1) // x
                    currentJointPositionLocal.x = joint.jointPosition[0];
                if (joint.dofCount >= 2) // y
                    currentJointPositionLocal.y = joint.jointPosition[1];
                if (joint.dofCount == 3) // z
                    currentJointPositionLocal.z = joint.jointPosition[2];

                Vector3 deltaJointPos = (currentJointPositionLocal - prevJointPos).Abs();
                traveledJointDistanceLocal += deltaJointPos.x + deltaJointPos.y + deltaJointPos.z;
                prevJointPos = currentJointPositionLocal;


                //  if (RecordJointMinMax && joints[j].Confidence == TrackingConfidence.High)
                // {
               // runtimeJointMinMax.Set(currentDriveTargets);
                // }

                // reset statistics data when joints accumulated traveled distance > 100 degrees
                if (driveTargetTraveledDistanceLocal > 100)
                {
                    actualTravelledRatio = traveledJointDistanceLocal != 0 ? driveTargetTraveledDistanceLocal / traveledJointDistanceLocal : 0;
                    driveTargetTraveledDistanceLocal = 0;
                    traveledJointDistanceLocal = 0;
                    // traveledJointDistanceLocal = 0f;
                    //driveTargetTraveledDistanceLocal = 0f;
                    // ratio = 0f;
                }
                jointPositionLocal = currentJointPositionLocal;
                jointPositionsSqrMag = currentJointPositionLocal.Abs().sqrMagnitude;
                // data.maxJointLimitsReached = data.jointPositionsSqrMag > data.maxJointPositionsSqrMagLocal;
                bool overshootOnX = Mathf.Clamp(currentJointPositionLocal.x, lowerLimitsRad.x, upperLimitsRad.x) != currentJointPositionLocal.x;
                bool overshootOnY = Mathf.Clamp(currentJointPositionLocal.y, lowerLimitsRad.y, upperLimitsRad.y) != currentJointPositionLocal.y;
                bool overshootOnZ = Mathf.Clamp(currentJointPositionLocal.z, lowerLimitsRad.z, upperLimitsRad.z) != currentJointPositionLocal.z;
                if (overshootOnX || overshootOnY || overshootOnZ)
                {
                    // TODO: calculate the overshoot value and compare it against joint limits + some threshold
                    // raise event only if overshoot is significant

                    float overlimitRad = Mathf.Sqrt(jointPositionsSqrMag - maxJointPositionsSqrMagLocal);
                    // if (data.IsOvershooting(currentJointPositionLocal, out Vector3 overshoot))
                    //  Debug.LogWarning($"{fingers[i].joints[j].jointName} is over limit {overshoot * Mathf.Rad2Deg} degrees");
                    // Debug.Break();
                }

                // joints[j].statsData = data;

            }

            public void ResetTravelRation()
            {
                actualTravelledRatio = traveledJointDistanceLocal != 0 ? driveTargetTraveledDistanceLocal / traveledJointDistanceLocal : 0;
                driveTargetTraveledDistanceLocal = 0;
                traveledJointDistanceLocal = 0;
            }
        }
    }
}