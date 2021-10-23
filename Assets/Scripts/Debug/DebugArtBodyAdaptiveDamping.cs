using SoftHand.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace SoftHand.Debug
{
    public class DebugArtBodyAdaptiveDamping : MonoBehaviour
    {
        public LayerMask interationLayer;
        public bool resetJointTos;
        private ArticulationBody _ab;
        private Vector3 _abLowerLimits, _abUpperLimits;
        private Vector3 _fraction;
        public Vector3 _overshootPercentage;
        public Vector3 _jointPositionsRounded;

        public float collisionRelativeVelocity;
        public float collisionImpulse;

        public float _jointPositionsSqrMag;
        public float _maxJointPositionsSqrMag;
        public bool maxJointLimitsReached;
        public Vector3 overshootPercentage => _overshootPercentage;
        public Vector3 jointPosition => _ab != null ? new Vector3(_is1Dof ? _ab.jointPosition[0] : 0f, _is2Dof ? _ab.jointPosition[1] : 0, _is3Dof ? _ab.jointPosition[2] : 0) : Vector3.zero;
        private List<float> jointPositionsRadians;
        public List<float> jointPositionsDegrees;

        private bool _is1Dof, _is2Dof, _is3Dof;

        private void Awake()
        {
            jointPositionsRadians = new List<float>();
            jointPositionsDegrees = new List<float>();

            _ab = GetComponent<ArticulationBody>();
            if (_ab == null)
                return;
            _abLowerLimits.x = _ab.xDrive.lowerLimit;
            _abLowerLimits.y = _ab.yDrive.lowerLimit;
            _abLowerLimits.z = _ab.zDrive.lowerLimit;

            _abUpperLimits.x = _ab.xDrive.upperLimit;
            _abUpperLimits.y = _ab.yDrive.upperLimit;
            _abUpperLimits.z = _ab.zDrive.upperLimit;

            _maxJointPositionsSqrMag = (_abLowerLimits.Abs() + _abUpperLimits.Abs()).sqrMagnitude;
        }
        private void OnEnable()
        {
            _ab = GetComponent<ArticulationBody>();
        }

        private void FixedUpdate()
        {
            if (_ab == null)
                return;
            _ab.GetJointPositions(jointPositionsRadians);
            jointPositionsDegrees = jointPositionsRadians;
            for (int i = 0; i < jointPositionsDegrees.Count; i++)
            {
                jointPositionsDegrees[i] *= Mathf.Rad2Deg;
            }
            GetOvershoot();
            ResetJointPositionToZero();
        }

        private void Start()
        {
            if (_ab.jointPosition.dofCount == 3)
                _is3Dof = true;
            if (_ab.jointPosition.dofCount == 2)
                _is2Dof = true;
            if (_ab.jointPosition.dofCount == 1)
                _is1Dof = true;
        }

        public bool IsInteraction(int layer)
        {
            bool contains = interationLayer == (interationLayer | 1 << layer);
            //if (contains)
            //    Debug.Log(contains);
            return contains;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (IsInteraction(collision.collider.gameObject.layer))
            {
                collisionRelativeVelocity = collision.relativeVelocity.magnitude;
                collisionImpulse = collision.impulse.magnitude;

            }
            //for (int i = 0; i < collision.contactCount; i++)
            //{
            //    ContactPoint point = collision.GetContact(i);

            //}
        }

        private void ResetJointPositionToZero()
        {
            if (_ab == null)
                return;
            if (resetJointTos)
            {
                List<float> zeros = jointPositionsRadians;
                for (int i = 0; i < zeros.Count; i++)
                {
                    zeros[i] = 0;
                }
                _ab.SetJointPositions(zeros);
            }

        }

        private void GetOvershoot()
        {
            if (_is1Dof) _fraction.x = Mathf.InverseLerp(_abLowerLimits.x, _abUpperLimits.x, _ab.jointPosition[0] * Mathf.Rad2Deg);
            if (_is2Dof) _fraction.y = Mathf.InverseLerp(_abLowerLimits.y, _abUpperLimits.y, _ab.jointPosition[1] * Mathf.Rad2Deg);
            if (_is3Dof) _fraction.z = Mathf.InverseLerp(_abLowerLimits.z, _abUpperLimits.z, _ab.jointPosition[2] * Mathf.Rad2Deg);

            _fraction = _fraction.Abs();

            // clamp
            _overshootPercentage.Set(
                Mathf.Clamp(_fraction.x, 1f, 360f),
                Mathf.Clamp(_fraction.y, 1f, 360f),
                Mathf.Clamp(_fraction.z, 1f, 360f));

            _overshootPercentage -= Vector3.one;

            _jointPositionsRounded = new Vector3(
                 Mathf.Round(jointPosition.x * Mathf.Rad2Deg * 100f) * 0.01f,
                 Mathf.Round(jointPosition.y * Mathf.Rad2Deg * 100f) * 0.01f,
                 Mathf.Round(jointPosition.z * Mathf.Rad2Deg * 100f) * 0.01f
                );
            _jointPositionsSqrMag = _jointPositionsRounded.Abs().sqrMagnitude;

            //if (!maxJointLimitsReached)
            maxJointLimitsReached = _jointPositionsSqrMag > _maxJointPositionsSqrMag;
            float overlimitRad = Mathf.Sqrt(_jointPositionsSqrMag - _maxJointPositionsSqrMag);

            if (maxJointLimitsReached)
            {
                //   Debug.LogWarning($"{gameObject.name} is over limit by {overlimitRad} degrees. {_jointPositionsRounded}");

            }



        }
    }
}