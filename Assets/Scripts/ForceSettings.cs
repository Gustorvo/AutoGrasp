using UnityEngine;
using SoftHand.Interfaces;

namespace SoftHand
{
    [CreateAssetMenu(menuName = "SoftHand/Create new Force Settings")]
    [System.Serializable]
    public class ForceSettings : ScriptableObject, IForceSettings
    {
        [SerializeField]
        public bool _move;
        [Range(0, 1), SerializeField]
        public float _linearForceWeight = 1;
        [Range(0, 100), SerializeField, Tooltip("Converts the distance remaining to the target velocity - if too low, the articulation body slows down early and takes a long time to stop. If too high, it may overshoot")]
        float _toVelocity = 55;
        [Range(0, 200), SerializeField, Tooltip(" Max speed the articulation body will reach when moving")]
        float _maxVelocity = 100;
        [Range(0, 200), SerializeField, Tooltip("Limits the force applied to the articulation body in order to avoid excessive acceleration (and instability)")]
        float _maxForce = 100;
        [Range(0, 50), SerializeField, Tooltip("Sets the feedback amount: if too low, the articulation body stops before the target point; if too high, it may overshoot and oscillate")]
        float _gain = 20;

        public bool ShouldMove => _move;

        public float LinearForceWeight => _linearForceWeight;

        public float ToVelocity => _toVelocity;

        public float MaxVelocity => _maxVelocity;

        public float MaxForce => _maxForce;

        public float Gain => _gain;
    }

}