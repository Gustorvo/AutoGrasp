using UnityEngine;

namespace SoftHand.Debug
{
    public class DebugQuickBodyMover : MonoBehaviour
    {
        [SerializeField] Rigidbody _rb;
        [SerializeField] ArticulationBody[] _ab;
        [SerializeField] Transform[] _targets;
        [SerializeField] Transform _targetTransform;

        [Header("Angular force values:")]
        public bool useRB;
        public bool rotate = false;
        public float alignmentSpeed = 7f;
        public float alignmentDamping = 5f;
        [Range(0.1f, 0f)] public float _strength = 1f;
        [Range(0.1f, 60f)] public float _freq = 6f;
        [Range(0.1f, 10f)] public float _damp = 1f;
        [Range(0, 1)] public float angularForceWeight = 1;

        [Header("Linear force values:")]
        public bool move = false;
        public float toVel = 25f; // converts the distance remaining to the target velocity - if too low, the rigidbody slows down early and takes a long time to stop. If too high, it may overshoot
        public float maxVel = 1500f; //max speed the rigidbody will reach when moving
        public float maxForce = 1500f; // limits the force applied to the rigidbody in order to avoid excessive acceleration (and instability)
        public float gain = 60f; // sets the feedback amount: if too low, the rigidbody stops before the target point; if too high, it may overshoot and oscillate       
        [Range(0, 1)] public float linearForceWeight = 1;

        private float _totalMass;

        private void Awake()
        {
            // _totalMass = _rb.mass;
        }
        private void FixedUpdate()
        {
            for (int i = 0; i < _ab.Length; i++)
            {
                _ab[i].AddJointForceToMatchTargetRotation(_targets[i].transform.localRotation, 1f);
            }

        }

    }
}