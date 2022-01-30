using UnityEngine;
using NaughtyAttributes;

namespace SoftHand
{
    [CreateAssetMenu(menuName = "SoftHand/Create new Force Settings")]
    [System.Serializable]
    public class ForceSettings : ScriptableObject, IForceSettings
    {        
        [Range(0, 1), SerializeField]
        public float _linearForceWeight = _linearForceWeightDefault;
        [Range(0, 100), SerializeField, Tooltip("Converts the distance remaining to the target velocity - if too low, the articulation body slows down early and takes a long time to stop. If too high, it may overshoot")]
        float _toVelocity = _toVelocityDefault;
        [Range(0, 100), SerializeField, Tooltip(" Max speed the articulation body will reach when moving")]
        float _maxVelocity = _maxVelocityDefault;
        [Range(0, 100), SerializeField, Tooltip("Limits the force applied to the articulation body in order to avoid excessive acceleration (and instability)")]
        float _maxForce = _maxForceDefault;
        [Range(0, 100), SerializeField, Tooltip("Sets the feedback amount: if too low, the articulation body stops before the target point; if too high, it may overshoot and oscillate")]
        float _gain = _gainDefault;
       
        #region defaults
        private const float _linearForceWeightDefault = 1f;
        private const float _toVelocityDefault = 50f;
        private const float _maxVelocityDefault = 3;
        private const float _maxForceDefault = 50;
        private const float _gainDefault = 50f;
        #endregion

        public float LinearForceWeight => _linearForceWeight;

        public float ToVelocity => _toVelocity;

        public float MaxVelocity => _maxVelocity;

        public float MaxForce => _maxForce;

        public float Gain => _gain;

        [Button("Reset to defaults")]
        public void ResetToDefaults()
        {
            _linearForceWeight = _linearForceWeightDefault;
            _toVelocity = _toVelocityDefault;
            _maxVelocity = _maxVelocityDefault;
            _maxForce = _maxForceDefault;
            _gain = _gainDefault;
        }
    }

}