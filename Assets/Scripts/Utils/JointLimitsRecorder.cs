using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand
{
    //[ExecuteInEditMode]
    public class JointLimitsRecorder : MonoBehaviour//, IExposedPropertyTable
    {
        [SerializeField] bool _recordJointLimitsToPreset = true;
       // [SerializeField] bool _recordInactiveDrives = false;
        [SerializeField] JointLimitsPreset _preset;
        [SerializeField] ArticulatedHand handToRecordFrom;
        //[SerializeField] List<JointLimitsPreset.Limits> jointLimits = new List<JointLimitsPreset.Limits>();

        private void Awake()
        {
            // tell ArticulatedHand class that we want to record joint min max values
            handToRecordFrom.RecordJointMinMax = _recordJointLimitsToPreset;

            handToRecordFrom.OnInitialized -= Init;
            handToRecordFrom.OnInitialized += Init;
        }

        private void OnDisable()
        {
            handToRecordFrom.OnInitialized -= Init;
        }

        private void Init(ArticulatedHand hand)
        {
            List<string> joints = hand.AllHandJoints.ConvertAll(j => j.jointName).ToList();
            if (_preset != null && _recordJointLimitsToPreset)
            {
                _preset.CreateJointList(handToRecordFrom.Handedness, joints);
            }
        }

        void OnApplicationQuit()
        {
            SaveJointLimitsToPreset();
        }

        /// <summary> Copy joints min-max values from stats to the preset </summary>
        public void SaveJointLimitsToPreset()
        {
            if (_preset != null && handToRecordFrom.Initialized && handToRecordFrom.RecordJointMinMax)
            {
                int index = 0;
                for (int i = 0; i < handToRecordFrom.Fingers.Length; i++)
                {
                    for (int j = 0; j < handToRecordFrom.Fingers[i].joints.Length; j++)
                    {
                        bool hasX = handToRecordFrom.Fingers[i].joints[j].statsData.hasXDrive;
                        bool hasY = handToRecordFrom.Fingers[i].joints[j].statsData.hasYDrive;
                        bool hasZ = handToRecordFrom.Fingers[i].joints[j].statsData.hasZDrive;

                        _preset.jointLimits[index] = handToRecordFrom.Fingers[i].joints[j].statsData.runtimeJointMinMax;

                        if (hasX) _preset.jointLimits[index].type = DriveEnabled.Xdrive;
                        if (hasY) _preset.jointLimits[index].type |= DriveEnabled.Ydrive;
                        if (hasZ) _preset.jointLimits[index].type |= DriveEnabled.Zdrive;

                        //_preset.jointLimits[index].yDriveLimits = Vector2.zero;
                        // _preset.jointLimits[index].xDriveLimits = Vector2.zero;
                        // _preset.jointLimits[index].zDriveLimits = Vector2.zero;

                        index++;
                    }
                }
                //AssetDatabase.Refresh();
                //JointLimitsPreset.ApplyModifiedProperties();
                //AssetDatabase.SaveAssets();
                UnityEngine.Debug.Log("Saved");
            }
        }

    }



}