using NaughtyAttributes;
using SoftHand.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static SoftHand.Enums;

namespace SoftHand
{
    [RequireComponent(typeof(ArticulatedHand))]
    public class ArticulatedHandSettings : MonoBehaviour
    {
        [SerializeField, Expandable] ArticulatedHandSettingsPreset _settingsAsset;
        private bool _hasSettingsAsset => _settingsAsset != null;
        private ArticulatedHand _hand;
        private bool _init;

        private void OnEnable()
        {
            Assert.IsNotNull(_settingsAsset);
        }

        private bool TryInit()
        {
            if (_hand == null && TryGetComponent<ArticulatedHand>(out _hand))
            {
                if (!_hand.Initialized)
                    _hand.Init();
            }

            _init = (_hand && _hand.ArticulationBody && _settingsAsset);
            return _init;
        }

        // show button in the inspector (only if Settings Asset is assigned)       

        [ShowIf("_hasSettingsAsset"), Button("Initialize and apply settings")]
        private void ApplyPhysicalPropertiesFromAsset()
        {
            if (!_init && !TryInit())
                return;


            bool doubleDamping = _settingsAsset.globalArticulationDriveMotorSettings.doubleDampingForFirstJoint;
            float damping = _settingsAsset.globalArticulationDriveMotorSettings.damping;

           // foreach (var finger in _hand.Fingers)
           // {
                for (int i = 0; i < _hand.Joints.Count; i++)
                {
                    _hand.Joints[i].ArticulationBody.mass = _settingsAsset.jointsPhysicalProperties.mass;
                    _hand.Joints[i].ArticulationBody.maxAngularVelocity = _settingsAsset.jointsPhysicalProperties.maxAngularVelocity;
                    _hand.Joints[i].ArticulationBody.maxLinearVelocity = _settingsAsset.jointsPhysicalProperties.maxLinearVelocity;
                    _hand.Joints[i].ArticulationBody.linearDamping = _settingsAsset.jointsPhysicalProperties.linearDamping;
                    _hand.Joints[i].ArticulationBody.angularDamping = _settingsAsset.jointsPhysicalProperties.angularDamping;
                    _hand.Joints[i].ArticulationBody.maxDepenetrationVelocity = _settingsAsset.jointsPhysicalProperties.maxDepenetrationVelocity;
                    _hand.Joints[i].ArticulationBody.useGravity = _settingsAsset.jointsPhysicalProperties.useGravity;
                    _hand.Joints[i].ArticulationBody.jointFriction = _settingsAsset.jointsPhysicalProperties.jointFriciton;
                    _hand.Joints[i].ArticulationBody.collisionDetectionMode = _settingsAsset.jointsPhysicalProperties.CollisionDetection;
                    _hand.Joints[i].ArticulationBody.centerOfMass = GetCoM(_hand.Joints[i].Collider, _settingsAsset.jointsCenterOfMassAlignemnt);
                    //_hand.joints[i].collider.isTrigger = true;

                    _hand.Joints[i].ArticulationBody.xDrive = SetupDrive(_hand.Joints[i].ArticulationBody.xDrive);
                    _hand.Joints[i].ArticulationBody.yDrive = SetupDrive(_hand.Joints[i].ArticulationBody.yDrive);
                    _hand.Joints[i].ArticulationBody.zDrive = SetupDrive(_hand.Joints[i].ArticulationBody.zDrive);

                    ArticulationDrive SetupDrive(ArticulationDrive drive)
                    {
                        drive.stiffness = _settingsAsset.globalArticulationDriveMotorSettings.stiffness;
                        drive.forceLimit = _settingsAsset.globalArticulationDriveMotorSettings.forceLimit;
                        drive.damping = damping; // apply double joint friction on first joint in a finger
                        if (i == 0 && doubleDamping)
                            drive.damping *= damping;
                        return drive;
                    }

                    Vector3 GetCoM(Collider collider, COMAlignment alignment)
                    {
                        bool isLeft = _hand.Handedness == Handedness.Left;
                        CapsuleCollider capsuleCollider = (CapsuleCollider)collider;
                        float radius = capsuleCollider.radius;
                        // Vector3 direction = capsuleCollider.direction == 1 ? Vector3.fo
                        float height = capsuleCollider.height;
                        Vector3 beginning = Vector3.zero;
                        Vector3 center = capsuleCollider.center;
                        Vector3 end = new Vector3(height - radius * 2, 0f, 0f);
                        end *= isLeft ? -1 : 1;
                        return alignment == COMAlignment.beginning ? beginning : alignment == COMAlignment.center ? center : end;
                    }
                }
           // }
            // setup root (palm)
            _hand.ArticulationBody.maxAngularVelocity = _settingsAsset.palmPhysicalProperties.maxAngularVelocity;
            _hand.ArticulationBody.maxLinearVelocity = _settingsAsset.palmPhysicalProperties.maxLinearVelocity;
            _hand.ArticulationBody.maxDepenetrationVelocity = _settingsAsset.palmPhysicalProperties.maxDepenetrationVelocity;
            _hand.ArticulationBody.linearDamping = _settingsAsset.palmPhysicalProperties.linearDamping;
            _hand.ArticulationBody.angularDamping = _settingsAsset.palmPhysicalProperties.angularDamping;
            _hand.ArticulationBody.jointFriction = _settingsAsset.palmPhysicalProperties.jointFriciton;
            _hand.ArticulationBody.mass = _settingsAsset.palmPhysicalProperties.mass;
            _hand.ArticulationBody.useGravity = _settingsAsset.palmPhysicalProperties.useGravity;
            _hand.ArticulationBody.collisionDetectionMode = _settingsAsset.palmPhysicalProperties.CollisionDetection;
            _hand.ArticulationBody.centerOfMass = Vector3.zero;

            // UnityEngine.Debug.Log($"Max angular velocity is set to  { _hand.ArticulationBody.maxAngularVelocity}");
            // UnityEngine.Debug.Log($"Max linea velocity is set to  { _hand.ArticulationBody.maxLinearVelocity}");
             UnityEngine.Debug.Log($"New hand settings applied to { _hand.Handedness} hand");

            // setup friction
            _hand.PalmColliders.ForEach(c => c.material = _settingsAsset.palmPhysicalMaterial);
            _hand.Joints.ToList().ForEach( c => c.Collider.material = _settingsAsset.jointsPhysicalMaterial);

            // IgnoreCollisionBetweenNeighboringJoints();

            // boundingBox = palmArtBody.
        }

    }
}