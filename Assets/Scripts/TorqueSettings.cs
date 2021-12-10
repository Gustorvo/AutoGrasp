using UnityEngine;
using SoftHand.Interfaces;

namespace SoftHand
{
    [CreateAssetMenu(menuName = "SoftHand/Create new Torque Settings")]
    [System.Serializable]
    public class TorqueSettings : ScriptableObject, ITorqueSettings
    {
        [SerializeField]
        bool _rotate = true;
        [Range(0, 1), SerializeField]
        float _angularForceWeight = 1;
        [Range(0f, 30f), SerializeField, Tooltip("Frequency is the speed of convergence. If damping is 1, frequency is the 1/time taken to reach ~95% of the target value. i.e. a frequency of 6 will bring you very close to the target within 1/6 seconds")]
        float _frequency = 6f;
        [Range(0f, 10f), SerializeField, Tooltip("Damping = 1: the system is critically damped. Damping > 1: the system is over damped(sluggish), damping < 1: the system is under damped (oscillating a little")]
        float _damping = 1f;

        public bool ShouldRotate => _rotate;
        public float AngularForceWeight => _angularForceWeight;
        public float Frequency => _frequency;
        public float Damping => _damping;
    }

}