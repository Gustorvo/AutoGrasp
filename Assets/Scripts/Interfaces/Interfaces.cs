using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using static SoftHand.Enums;
using static SoftHand.JointLimitsPreset;

namespace SoftHand
{
    public interface IHand
    {
        IArticulatedHand Hand { get; }
    }
    public interface IDriveInputs
    {
        /// <summary>
        /// Main input source
        /// </summary>
        IDataProvider DataProvider { get; }
        /// <summary>
        /// ArticulationDrive targets in reduced spaced. Used to set X-Drive (Twist), Y-Drive (Swing Y) and Z-Drive (Swing Z) of an articulation body
        /// </summary>
        Vector3[] DriveTargets { get; }
        /// <summary>
        /// Joint positions are used to operate on body.jointPosition
        /// </summary>
        ArticulationReducedSpace[] JointPositions { get; }
        /// <summary>
        /// JointForces are used to operate on body.jointForce
        /// </summary>
        ArticulationReducedSpace[] JointForces { get; }
    }
    public interface IDataProvider
    {
        Pose GetRootPose(Handedness hand);
        Pose[] GetBonesPoses(Handedness hand);
        bool JointPositionsProvided { get; }
        bool JointRotationsProvided { get; }
        bool IsInitialized { get; }
    }
    public interface IHandTrackingDataProvider : IDataProvider
    {
        HandTrackingDataProvider Type { get; }
        int GetNumberOfJoints();
        bool IsReliable(Handedness hand);
        Pose GetLastReliableRootPose(Handedness hand);
        TrackingConfidence GetFingerConfidence(Handedness handedness, Finger finger);
    }
    public interface IArticulatedHandsController
    {
        /// <summary>
        /// List if all hands currently in controll (of this controller)
        /// </summary>
        IEnumerable<IArticulatedHand> Hands { get; }
        /// <summary>
        /// Adds an articulatedHand to this controller's loop
        /// Which will take controll of this hand
        /// </summary>        
        bool TryAdd(IArticulatedHand articulatedHand);

        /// <summary>
        /// Removes an articulatedHand from the controller's loop
        /// </summary>       
        bool TryRemove(IArticulatedHand articulatedHand);

        void FetchTrackingData();
        void MoveHands();
        void MoveFingers();
        // IArticulatedHand GetHand();
    }
    public interface IForceSettings
    {      
        float LinearForceWeight { get; }
        float ToVelocity { get; }
        float MaxVelocity { get; }
        float MaxForce { get; }
        float Gain { get; }
        void ResetToDefaults();
    }
    public interface ITorqueSettings
    {      
        float AngularForceWeight { get; }
        float Frequency { get; }
        float Damping { get; }
        void ResetToDefaults();
    }
    public interface IBodyConfig
    {
        /// <summary>
        /// For better physics stability, the palm mass shuold be x5 of bone mass
        /// </summary>
        float Mass { get; }
        bool UseGravity { get; }
        /// <summary>
        /// Coefficient that controls the linear slow down
        /// </summary>
        float LinearDamping { get; }
        /// <summary>
        /// Coefficient that controls rotational slow down
        /// </summary>
        float AngularDamping { get; }
        /// <summary>
        /// Coefficient that controls the energy loss caused by friction in the joint
        /// </summary>
        float JointFriciton { get; }
        public CollisionDetectionMode CollisionDetection { get; }
        /// <summary>
        /// The maximimum angular velocity of the articulation body measured in radians per second. Default: 7, range { 0, infinity }.
        /// </summary>
        float MaxAngularVelocity { get; }
        /// <summary>
        /// The maximum linear velocity of the articulation body measured in meters per second.
        /// </summary>
        float MaxLinearVelocity { get; }
        /// <summary>
        /// The maximum velocity of an articulation body when moving out of penetrating state.
        /// </summary>
        float MaxDepenetrationVelocity { get; }
    }
    public interface IHandConfig
    {
        IBodyConfig Palm { get; }
        IBodyConfig Joint { get; }
    }
    public interface ITrackable
    {
        string Name { get; }
        Pose Pose { get; }
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        //  float DistanceSqr { get; }
        Vector3 Velocity { get; }
        float Speed { get; }
        void Update(Pose newPose);
    }
    public interface IArticulatedHand : IBody
    {
        int InstanceId { get; }
        Handedness Handedness { get; }
        //IHandConfig Config { get; }
        IForceSettings ForceSettings { get; }
        ITorqueSettings TorqueSettings { get; }
        IHandTrackingDataProvider Tracking { get; }
        IJointStatsController RuntimeStats { get; }
        bool Initialized { get; }
        /// <summary>
        /// all joints found in this hand
        /// sorted in this order: thumb => pinky
        /// </summary>       
        ReadOnlyCollection<IJoint> Joints { get; }
        /// <summary>
        /// Total mass of this hand (palm + fingers)
        /// </summary>
        float TotalMass { get; }

        //  Action OnTeleport { get; }
        Action OnDriveTargetsSet { get; }
        Action OnTeleport { get; }
        event Action<IArticulatedHand> OnInitialized;

        /// <summary>
        ///  Returns True if hand doesn't collide with anything at target position.
        ///  Hint: collisions are defined in collision matrix 
        /// </summary>
        /// <param name="targetPose"></param>
        /// <returns></returns>
        bool CanTeleport(Pose targetPose);


        /// <summary>
        /// Internal update loop.
        /// </summary>
        void UpdateData();
    }
    public interface IBody
    {
        ArticulationBody ArticulationBody { get; }
        ITrackable BodyData { get; }
        ITrackable TargetData { get; }
        float SqrDistanceToTarget { get; }
        // void UpdateData(Pose newTargetData);

        // C# 8 isn't available in this version of Unity
        //{
        //    BodyData.Update(new Pose());
        //    TargetData.Update(new Pose());
        //}

    }
    public interface IBodyMover
    {
        IBody BodyToMove { get; }
        IForceSettings ForceSettings { get; }
        ITorqueSettings TorqueSettings { get; }
        void MoveBody();
        void RotateBody();

        /// <summary>
        /// Teleports articulation body to desired pose (physics 'turned-off').
        /// </summary>        
        void TeleportBody(Pose target);
        event Action OnTeleport;

    }
    public interface IFingerMover
    {
        void MoveFingers();
        List<IJoint> HandJoints { get; }
    }
    public interface IJoint : IBody
    {
        /// <summary>
        /// Instance Id
        /// </summary>
        int Id { get; }           
        string Name { get; }
        int Index { get; }
        int FingerIndex { get; }
        Collider Collider { get; }
        void ForceJointToPosition(Vector3 overshootValue);
        void Reset();
    }

    public interface IJointStatsController
    {
        bool RecordRuntimeMinMax { get; }
        bool IsJointStuck(IJoint joint);
        bool IsJointPositionOverLimit(IJoint joint, out Vector3 overshoot);
        Vector3 GetNearestJointMinMaxRange(IJoint joint, Vector3 currentRange);
        void ResetTravelRation(IJoint joint);
        Dictionary<int, List<IJointStats>> Stats { get; }
    }
    public interface IJointStats
    {
        DriveMinMax RuntimeJointMinMax { get; } // record joints' max/min values 
        /// <summary>
        /// Instance Id of the Articulation body this obj refers to
        /// </summary>
        int Id { get; }
        Vector3 JointPositionLocal { get; }
        float JointPositionsSqrMag { get; }
        int DofCount { get; }
        bool HasXDrive { get; }
        bool HasYDrive { get; }
        bool HasZDrive { get; }
        float MaxJointPositionsSqrMagLocal { get; } // the maximum possible squared magnitude of this joint in local space (in radians)
        Vector3 LowerLimitsRad { get; }
        Vector3 UpperLimitsRad { get; }// in radians              
        Vector3 LowerLimitsDeg { get; }
        Vector3 UpperLimitsDeg { get; } // in degrees
        Vector3 PrevJointPos { get; }
        Vector3 PrevDriveTargets { get; }
        float TraveledJointDistanceLocal { get; }
        float DriveTargetTraveledDistanceLocal { get; } // in radians
        float ActualTravelledRatio { get; } // driveTargetTraveledDistanceLocal / traveledJointDistanceLocal. used to be ~ 1 radian (57) for a 'healthy' joint, or > 500 for a buggy one

        void CollectsStats();
        void ResetTravelRation();
    }
    public interface ISensor
    {
        event Action<Collision> OnCollision;
    }


    public interface IBugFixable
    {
        IArticulatedHand Hand { get; }
        void ResetJointIfOvershooting();
        void ResetJointIfStuck();
    }
}