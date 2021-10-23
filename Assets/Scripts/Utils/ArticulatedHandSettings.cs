using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
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
                    _hand.InitializeArticulationBodies();
            }

            _init = (_hand && _hand.Palm && _settingsAsset);
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

            foreach (var finger in _hand.Fingers)
            {
                for (int i = 0; i < finger.joints.Length; i++)
                {
                    finger.joints[i].body.mass = _settingsAsset.jointsPhysicalProperties.mass;
                    finger.joints[i].body.maxAngularVelocity = _settingsAsset.jointsPhysicalProperties.maxAngularVelocity;
                    finger.joints[i].body.maxLinearVelocity = _settingsAsset.jointsPhysicalProperties.maxLinearVelocity;
                    finger.joints[i].body.linearDamping = _settingsAsset.jointsPhysicalProperties.linearDamping;
                    finger.joints[i].body.angularDamping = _settingsAsset.jointsPhysicalProperties.angularDamping;
                    finger.joints[i].body.maxDepenetrationVelocity = _settingsAsset.jointsPhysicalProperties.maxDepenetrationVelocity;
                    finger.joints[i].body.useGravity = _settingsAsset.jointsPhysicalProperties.useGravity;
                    finger.joints[i].body.jointFriction = _settingsAsset.jointsPhysicalProperties.jointFriciton;
                    finger.joints[i].body.collisionDetectionMode = _settingsAsset.jointsPhysicalProperties.CollisionDetection;
                    finger.joints[i].body.centerOfMass = GetCoM(finger.joints[i].collider, _settingsAsset.jointsCenterOfMassAlignemnt);
                    //finger.joints[i].collider.isTrigger = true;

                    finger.joints[i].body.xDrive = SetupDrive(finger.joints[i].body.xDrive);
                    finger.joints[i].body.yDrive = SetupDrive(finger.joints[i].body.yDrive);
                    finger.joints[i].body.zDrive = SetupDrive(finger.joints[i].body.zDrive);

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
            }
            // setup root (palm)
            _hand.Palm.maxAngularVelocity = _settingsAsset.palmPhysicalProperties.maxAngularVelocity;
            _hand.Palm.maxLinearVelocity = _settingsAsset.palmPhysicalProperties.maxLinearVelocity;
            _hand.Palm.maxDepenetrationVelocity = _settingsAsset.palmPhysicalProperties.maxDepenetrationVelocity;
            _hand.Palm.linearDamping = _settingsAsset.palmPhysicalProperties.linearDamping;
            _hand.Palm.angularDamping = _settingsAsset.palmPhysicalProperties.angularDamping;
            _hand.Palm.jointFriction = _settingsAsset.palmPhysicalProperties.jointFriciton;
            _hand.Palm.mass = _settingsAsset.palmPhysicalProperties.mass;
            _hand.Palm.useGravity = _settingsAsset.palmPhysicalProperties.useGravity;
            _hand.Palm.collisionDetectionMode = _settingsAsset.palmPhysicalProperties.CollisionDetection;
            _hand.Palm.centerOfMass = Vector3.zero;

            UnityEngine.Debug.Log($"Max angular velocity is set to  { _hand.Palm.maxAngularVelocity}");
            UnityEngine.Debug.Log($"Max linea velocity is set to  { _hand.Palm.maxLinearVelocity}");

            // setup friction
            _hand.PalmColliders.ForEach(c => c.material = _settingsAsset.palmPhysicalMaterial);
            _hand.AllHandJoints.ForEach(c => c.collider.material = _settingsAsset.jointsPhysicalMaterial);

            // IgnoreCollisionBetweenNeighboringJoints();

            // boundingBox = palmArtBody.
        }

    }
}