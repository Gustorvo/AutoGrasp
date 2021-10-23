using NaughtyAttributes;
using SoftHand.Extensions;
using UnityEngine;

namespace SoftHand.Experimental
{
    public class Mover : MonoBehaviour
    {
        [SerializeField] Transform _target;
        [SerializeField] Rigidbody _rb;
        [SerializeField] ArticulationBody _ab;
        [SerializeField] MoveBy _howToMove;
        [SerializeField] bool _move, _rotate;
        [SerializeField] bool _fixedLocalRotation;
        [SerializeField, Range(0.5f, 20f)] float _forceMultiplier = 2f;
        [SerializeField] Vector3 _anchor;
        [SerializeField, Range(0f, 0.5f)] float _moveOnDistance = 0.25f;
        [SerializeField] float _distance = 0f;
        [SerializeField, MinMaxSlider(0.1f, 20f)] Vector2 _moveSpeedMinMax = new Vector2(1f, 20f);
        [SerializeField] float _speed = 0f;

        private Quaternion _fixedRot;
        private bool _isMoving;
        private float _anchorDistance;

        private void Awake()
        {
            if (_rotate)
                _fixedLocalRotation = false;
            else if (_fixedLocalRotation)
                _rotate = false;
            _fixedRot = _ab ? _ab.transform.rotation : _rb ? _rb.rotation : Quaternion.identity;
            SetAnchor();
        }

        private void SetAnchor()
        {
            _anchor = _target.position - (_ab ? _ab.transform.position : _rb ? _rb.position : Vector3.zero);
            _anchorDistance = Vector3.Distance(_target.position, (_ab ? _ab.transform.position : _rb ? _rb.position : Vector3.zero));
        }
        private void FixedUpdate()
        {

            GetDistanceToTarget();
            MoveRigidBody();
            MoveArticulationBody();
            FixRotation();
        }

        private void GetDistanceToTarget()
        {
            if (!_target) return;
            Vector3 from = _ab ? _ab.transform.position : _rb ? _rb.transform.position : Vector3.zero;
            _distance = Vector3.Distance((from + _anchor.normalized * _anchorDistance), _target.position);
        }

        private void FixRotation()
        {
            if (_fixedLocalRotation)
            {
                if (_ab)
                {
                    var newRot = _ab.CalculateRequiredTorqueForRotation(_fixedRot);
                    _ab.AddRelativeTorque(newRot);
                }
                if (_rb)
                {
                    var newRot = _rb.CalculateRequiredTorqueForRotation(_fixedRot);
                    _rb.AddRelativeTorque(newRot);
                }
            }
        }

        private void MoveRigidBody()
        {
            if (_target && _rb)
            {
                if (_howToMove == MoveBy.Kinematic)
                {
                    _rb.isKinematic = true;
                    if (_move /*&&  _distance > _moveOnDistance*/)
                    {
                        float speedFractionBasedOnDistanceToTarget = Mathf.InverseLerp(0.025f, 0.075f, _distance);
                        _speed = Mathf.Lerp(_moveSpeedMinMax.x, _moveSpeedMinMax.y, speedFractionBasedOnDistanceToTarget);
                        float step = _speed * Time.deltaTime; // calculate distance to move
                        Vector3 newPos = Vector3.Lerp(_rb.position, _target.position - _anchor, step);
                        _rb.MovePosition(newPos);
                        // _rb.transform.position = newPos;
                    }
                    if (_rotate)
                        _rb.MoveRotation(_target.rotation);
                }
                else if (_howToMove == MoveBy.ByForce)
                {
                    if (_move && _distance > _moveOnDistance)
                    {
                        Vector3 newPos = _rb.CalculateLinearForce(_target.position - _anchor);
                        _rb.AddForce(newPos * _rb.mass * _forceMultiplier);

                    }
                    if (_rotate)
                    {
                        Vector3 newRot = _rb.CalculateRequiredTorqueForRotation(_target.rotation);
                        _rb.AddTorque(newRot, ForceMode.Force);

                    }
                }
            }
        }

        private void MoveArticulationBody()
        {
            if (_target && _ab)
            {
                if (_howToMove == MoveBy.Kinematic)
                {
                    //_ab.isKinematic = true;
                    if (_move && _rotate && _distance > _moveOnDistance)
                        _ab.TeleportRoot(_target.position - _anchor, _target.rotation);
                }
                else if (_howToMove == MoveBy.ByForce)
                {
                    if (_move && _distance > _moveOnDistance)
                    {
                        Vector3 newPos = _ab.CalculateLinearForce(_target.position - _anchor);
                        _ab.AddForce(newPos * _ab.mass * _forceMultiplier);

                    }
                    if (_rotate)
                    {
                        Vector3 newRot = _ab.CalculateRequiredTorqueForRotation(_target.rotation);
                        _ab.AddTorque(newRot);

                    }
                }
            }
        }
    }

    public enum MoveBy
    {
        Kinematic,
        ByForce
    }
}
