using UnityEngine;
using NaughtyAttributes;

namespace SoftHand
{
    [CreateAssetMenu(menuName = "SoftHand/Create new Torque Settings")]
    [System.Serializable]
    public class TorqueSettings : ScriptableObject, ITorqueSettings
    {
        [Range(0, 1), SerializeField]
        float _angularForceWeight = _angularForceWeighDefault;
        [Range(0f, 30f), SerializeField, Tooltip("Frequency is the speed of convergence. If damping is 1, frequency is the 1/time taken to reach ~95% of the target value. i.e. a frequency of 6 will bring you very close to the target within 1/6 seconds")]
        float _frequency = _frequencyDefault;
        [Range(0f, 10f), SerializeField, Tooltip("Damping = 1: the system is critically damped. Damping > 1: the system is over damped(sluggish), damping < 1: the system is under damped (oscillating a little")]
        float _damping = _dampingDefault;

        #region defaults
        private const float _angularForceWeighDefault = 1f;
        private const float _frequencyDefault = 6f;
        private const float _dampingDefault = 1f;
        #endregion

        public float AngularForceWeight => _angularForceWeight;
        public float Frequency => _frequency;
        public float Damping => _damping;

        [Button("Reset to defaults")]
        public void ResetToDefaults()
        {
            _angularForceWeight = _angularForceWeighDefault;
            _frequency = _frequencyDefault;
            _damping = _dampingDefault;
        }
    }

}