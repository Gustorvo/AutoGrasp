using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoftHand.Enums;
using static SoftHand.JointLimitsPreset;
using System.Linq;
using static SoftHand.ArticulationBodySettings;
using NaughtyAttributes;

namespace SoftHand
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "SoftHand/Create new settings preset for Articulated hand")]
    public class ArticulatedHandSettingsPreset : ScriptableObject
    {    
        [Header("Hand physical properties")]
        public ArticulationBodyPhysicsSettings palmPhysicalProperties;
        public ArticulationBodyPhysicsSettings jointsPhysicalProperties;
        [Tooltip("Specify where the center of mass (of every joint) will be placed")]
        public COMAlignment jointsCenterOfMassAlignemnt;

        [Header("Hand physical materials (used for friction simulation)")]   
        public PhysicMaterial palmPhysicalMaterial;
        public PhysicMaterial jointsPhysicalMaterial;




        [Header("Articulation Bodies' motor properties:")]
        public MotorSettings globalArticulationDriveMotorSettings;


        [Expandable]
        public JointLimitsPreset limitPreset;

        public ArticulatedFingerSettings[] fingers => _fingers;

        public bool showExperimentalSettings = false;
        [Header("Per finger/joint settings (experimental)"), SerializeField]
        private ArticulatedFingerSettings[] _fingers = new ArticulatedFingerSettings[5];       
       
        [HideInInspector]
        public bool initialized = false;

      

        public void Init()
        {
            //script.fingers = new ArtHandSettingsPreset.ArtFinger[5];
            _fingers = new ArticulatedFingerSettings[5];
            fingers[0] = new ArticulatedFingerSettings(Finger.Thumb);
            fingers[1] = new ArticulatedFingerSettings(Finger.Index);
            fingers[2] = new ArticulatedFingerSettings(Finger.Middle);
            fingers[3] = new ArticulatedFingerSettings(Finger.Ring);
            fingers[4] = new ArticulatedFingerSettings(Finger.Pinky);
            initialized = true;
        }

        private void CreateJoints(ArticulatedFingerSettings finger, int numOfJoints)
        {
            finger.joints = new List<ArticulatedJointSettings>(numOfJoints);
            for (int i = 0; i < numOfJoints; i++)
            {
                var joint = new ArticulatedJointSettings(i);
                joint.name = SkeletonMapping.GetJointName(finger.type, i);
                finger.joints.Add(joint);
            }
        }

        public void CreateJoints()
        {
            for (int i = 0; i < fingers.Length; i++)
            {
                CreateJoints(fingers[i], SkeletonMapping.GetNumOfJointsInFinger(i));
            }
        }

        public void SetupCustom()
        {
            SetJointLimits();

            //set to 1 dof limited (Revolute Joint) with anchor Rotation 90 degrees on Y axis
            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].joints.ForEach(x => x.jointType = ArticulationJointType.RevoluteJoint);
                fingers[i].joints.ForEach(x => x.motions = new MotionSettings(ArticulationDofLock.LimitedMotion, ArticulationDofLock.LockedMotion, ArticulationDofLock.LockedMotion));
                fingers[i].joints.ForEach(x => x.anchorRotation = new Vector3(0f, 90f, 0f));
            }

            // set to 3 dofs (Spherical Joint) limited (on each 1st joint in finger)
            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].joints[0].jointType = ArticulationJointType.SphericalJoint;
                fingers[i].joints[0].motions = new MotionSettings(ArticulationDofLock.LimitedMotion, ArticulationDofLock.LimitedMotion, ArticulationDofLock.LimitedMotion);
                fingers[i].joints[0].anchorRotation = Vector3.zero;
            }

            //set thumb0 and pinky0 to Fixed
            fingers[0].joints[0].jointType = ArticulationJointType.FixedJoint;
            fingers[4].joints[0].motions = new MotionSettings(ArticulationDofLock.LockedMotion, ArticulationDofLock.LockedMotion, ArticulationDofLock.LockedMotion);

            fingers[4].joints[0].jointType = ArticulationJointType.FixedJoint;
            fingers[0].joints[0].motions = new MotionSettings(ArticulationDofLock.LockedMotion, ArticulationDofLock.LockedMotion, ArticulationDofLock.LockedMotion);

            // .. and thumb1 and pinky1 to to 3 dof limited
            fingers[4].joints[1].jointType = ArticulationJointType.SphericalJoint;
            fingers[4].joints[1].motions = new MotionSettings(ArticulationDofLock.LimitedMotion, ArticulationDofLock.LimitedMotion, ArticulationDofLock.LimitedMotion);
            fingers[4].joints[1].anchorRotation = Vector3.zero;

            fingers[0].joints[1].jointType = ArticulationJointType.SphericalJoint;
            fingers[0].joints[1].motions = new MotionSettings(ArticulationDofLock.LimitedMotion, ArticulationDofLock.LimitedMotion, ArticulationDofLock.LimitedMotion);
            fingers[0].joints[1].anchorRotation = Vector3.zero;
        }

        public void SetToFreeMotion()
        {
            MotionSettings driveLock = new MotionSettings();
            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].SetDriveLocks(driveLock);
                fingers[i].joints.ForEach(x => x.jointType = ArticulationJointType.SphericalJoint);
            }
        }

        public void ResetAnchorRotaionOnSphericalJoints()
        {
            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].joints.ForEach(x => x.anchorRotation = Vector3.zero);
            }
        }

        public void SetAllDrivesToRevolute()
        {
            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].SetJointType(ArticulationJointType.RevoluteJoint);
            }
        }


        public void SetGlobalMotorSettings()
        {
            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].SetMotorSettings(globalArticulationDriveMotorSettings);
            }
        }

        public void SetJointLimits()
        {
            if (limitPreset != null && limitPreset.jointLimits.Count == 17) // there should be 17 bones totaly
            {
                for (int i = 0; i < fingers.Length; i++)
                {
                    fingers[i].SetLimits(limitPreset);
                    fingers[i].SetDriveLocks(new MotionSettings(ArticulationDofLock.LimitedMotion, ArticulationDofLock.LimitedMotion, ArticulationDofLock.LimitedMotion));
                    // fingers[i].SetJointType(ArticulationJointType.SphericalJoint);
                }
            }
        }



       

       

        

    }
}