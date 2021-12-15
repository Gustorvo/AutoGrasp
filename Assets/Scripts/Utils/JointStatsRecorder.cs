using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static SoftHand.ArticulatedJoint;
using static SoftHand.Enums;
using static SoftHand.JointLimitsPreset;

namespace SoftHand
{
    //[ExecuteInEditMode]
    public class JointStatsRecorder : MonoBehaviour, IJointStatsController
    {
        [SerializeField] bool _recordJointStats = true;
        [SerializeField] bool _recordJointLimitsToPreset = false;


        private JointLimitsPreset _preset;
        public bool RecordRuntimeMinMax => throw new NotImplementedException();
        private bool _initialized;
        public Dictionary<int, List<IJointStats>> Stats { get; private set; } = new Dictionary<int, List<IJointStats>>();
        private List<IJointStats> _runtimeJointsStats = new List<IJointStats>();
        private List<DriveMinMax> _runtimeJointsMinMax = new List<DriveMinMax>();
        private IArticulatedHand _handToRecordFrom { get; set; }        

        //[SerializeField] List<JointLimitsPreset.Limits> jointLimits = new List<JointLimitsPreset.Limits>();

        private void Reset()
        {
            _preset = HandsCore.RuntimeJointLimits;
        }
        private void Awake()
        {
            _handToRecordFrom = GetComponent<IArticulatedHand>();
            _preset = HandsCore.RuntimeJointLimits;
            Assert.IsNotNull(_handToRecordFrom);
            Assert.IsNotNull(_preset);
            _handToRecordFrom.OnInitialized -= Init;
            _handToRecordFrom.OnInitialized += Init;          
        }       

        private void FixedUpdate()
        {
            if (!_initialized)
                return;
            for (int i = 0; i < _handToRecordFrom.Joints.Count; i++)
            {
                _runtimeJointsStats[i].CollectsStats();
                _runtimeJointsMinMax[i].Set();
            }
        }

        private void OnDisable()
        {
            if (!_initialized)
                return;
            _handToRecordFrom.OnInitialized -= Init;
        }

        private void Init(IArticulatedHand hand)
        {            
            _runtimeJointsStats.Clear();
            _runtimeJointsMinMax.Clear();
            for (int i = 0; i < _handToRecordFrom.Joints.Count; i++)
            {
                ArticulationBody jointBody = _handToRecordFrom.Joints[i].ArticulationBody;
                string jointName = _handToRecordFrom.Joints[i].Name;
                _runtimeJointsStats.Add(new StatsData(jointBody));
                _runtimeJointsMinMax.Add(new DriveMinMax(jointBody, jointName));
            }
            _initialized = _handToRecordFrom.Joints.Count > 0;
        }

        void OnApplicationQuit()
        {
            SaveJointLimitsToPreset();
        }

        /// <summary> Copy joints min-max values from stats to the preset </summary>
        public void SaveJointLimitsToPreset()
        {
            if (!_initialized || !_recordJointLimitsToPreset)
                return;
            _preset?.AddNewRecord(_handToRecordFrom.Handedness, _runtimeJointsMinMax);

            //    for (int j = 0; j < _handToRecordFrom.Joints.Count; j++)
            //    {
            //        bool hasX = _handToRecordFrom.Joints[j].Stats.HasXDrive;
            //        bool hasY = _handToRecordFrom.Joints[j].Stats.HasYDrive;
            //        bool hasZ = _handToRecordFrom.Joints[j].Stats.HasZDrive;

            //        _preset.jointLimits[index] = _handToRecordFrom.Joints[j].Stats.RuntimeJointMinMax;

            //        if (hasX) _preset.jointLimits[index].type = DriveEnabled.Xdrive;
            //        if (hasY) _preset.jointLimits[index].type |= DriveEnabled.Ydrive;
            //        if (hasZ) _preset.jointLimits[index].type |= DriveEnabled.Zdrive;

            //        //_preset.jointLimits[index].yDriveLimits = Vector2.zero;
            //        // _preset.jointLimits[index].xDriveLimits = Vector2.zero;
            //        // _preset.jointLimits[index].zDriveLimits = Vector2.zero;


            //    //}
            //    //AssetDatabase.Refresh();
            //    //JointLimitsPreset.ApplyModifiedProperties();
            //    //AssetDatabase.SaveAssets();
            //    UnityEngine.Debug.Log("Saved");
            //}


        }

        public bool IsJointPositionOverLimit(IJoint joint, out Vector3 overlimitRadians)
        {
            overlimitRadians = Vector3.zero;
            IJointStats statsData = _runtimeJointsStats.FirstOrDefault(x => x.Id == joint.Id);
            if (statsData == null)
            {
                return false;
            }
            float threshold = 1.5f * Mathf.Deg2Rad;

            if (statsData.JointPositionLocal.x > statsData.UpperLimitsRad.x + threshold)
                overlimitRadians.x = statsData.JointPositionLocal.x - statsData.UpperLimitsRad.x;
            else if (statsData.JointPositionLocal.x < statsData.LowerLimitsRad.x - threshold)
                overlimitRadians.x = statsData.JointPositionLocal.x - statsData.LowerLimitsRad.x;

            if (statsData.JointPositionLocal.y > statsData.UpperLimitsRad.y + threshold)
                overlimitRadians.y = statsData.JointPositionLocal.y - statsData.UpperLimitsRad.y;
            else if (statsData.JointPositionLocal.y < statsData.LowerLimitsRad.y - threshold)
                overlimitRadians.y = statsData.JointPositionLocal.y - statsData.LowerLimitsRad.y;

            if (statsData.JointPositionLocal.z > statsData.UpperLimitsRad.z + threshold)
                overlimitRadians.z = statsData.JointPositionLocal.z - statsData.UpperLimitsRad.z;
            else if (statsData.JointPositionLocal.z < statsData.LowerLimitsRad.z - threshold)
                overlimitRadians.z = statsData.JointPositionLocal.z - statsData.LowerLimitsRad.z;

            return overlimitRadians != Vector3.zero;
        }

        public bool IsJointStuck(IJoint joint)
        {
            IJointStats statsData = _runtimeJointsStats.FirstOrDefault(x => x.Id == joint.Id);
            if (statsData == null)
            {
                return false;
            }
            // calculated as driveTargetTraveledDistanceLocal / traveledJointDistanceLocal.
            // used to be ~ 1 radian (57) for a 'healthy' joint, or > 500 for a buggy one
            return statsData.ActualTravelledRatio > 700f;
            
        }
        public Vector3 GetNearestJointMinMaxRange(IJoint joint, Vector3 currentRange)
        {
            IJointStats statsData = _runtimeJointsStats.FirstOrDefault(x => x.Id == joint.Id);
            if (statsData == null)
            {
                return Vector3.zero;
            }
            Vector3 newJointPos = new Vector3
               (
                   currentRange.x > 0 ? statsData.UpperLimitsRad.x : currentRange.x < 0 ? statsData.LowerLimitsRad.x : statsData.JointPositionLocal.x,
                   currentRange.y > 0 ? statsData.UpperLimitsRad.y : currentRange.y < 0 ? statsData.LowerLimitsRad.y : statsData.JointPositionLocal.y,
                   currentRange.z > 0 ? statsData.UpperLimitsRad.z : currentRange.z < 0 ? statsData.LowerLimitsRad.z : statsData.JointPositionLocal.z
               );
            return newJointPos;
        }

        public void ResetTravelRation(IJoint joint)
        {
            IJointStats statsData = _runtimeJointsStats.FirstOrDefault(x => x.Id == joint.Id);
            if (statsData == null) return;
            statsData.ResetTravelRation();
        }

    }
}