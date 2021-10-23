using UnityEngine;

namespace SoftHand.Experimental
{
    public class ArtBodyToTargetFollower : MonoBehaviour
    {
        [SerializeField] ArticulationBody _ab;
        [SerializeField] bool _immovable = true;
        [SerializeField, Range(0.1f, 10f)] float _strength = 1f;

        Pose _fixedPose;
        float _alpha = 0.05f;


        private void Awake()
        {
            if (!_ab) return;
            _fixedPose.position = _ab.transform.localPosition;
            _fixedPose.rotation = _ab.transform.localRotation;
        }
        private void FixedUpdate()
        {
            if (_ab && _immovable)
            {
                //_ab.AddForce(_ab.CalculateLinearForce(_fixedPose.position) * _ab.mass);
                //_ab.AddTorque(_ab.CalculateRequiredTorqueForRotation(_fixedPose.rotation));
                _ab.velocity *= _alpha;
                _ab.AddRelativeForce(GetForce());

                _ab.angularVelocity = GetAngularVelocity();
                _ab.angularDamping = 50f;

            }
        }

        Vector3 GetAngularVelocity()
        {
            Quaternion delta = _fixedPose.rotation * Quaternion.Inverse(_ab.transform.localRotation);
            Vector3 angularVelocity = Vector3.ClampMagnitude(new Vector3(
              Mathf.DeltaAngle(0, delta.eulerAngles.x),
              Mathf.DeltaAngle(0, delta.eulerAngles.y),
              Mathf.DeltaAngle(0, delta.eulerAngles.z)) / Time.fixedDeltaTime * Mathf.Deg2Rad, 45f * _strength);
            return angularVelocity;
        }

        Vector3 GetForce()
        {
            // Blend between existing velocity and all new velocity

            Vector3 delta = _fixedPose.position - _ab.transform.localPosition;
            return Vector3.ClampMagnitude(delta / Time.fixedDeltaTime / Time.fixedDeltaTime * _ab.mass * (1f - _alpha), 1000f * _strength);
        }
    }
}