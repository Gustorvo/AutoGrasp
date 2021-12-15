using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand
{
    /// <summary>
    /// Base class for Hand tracking
    /// Since there is no easy way for the interfaces to show up in the Unity Inspector,
    /// every Hand tracking data provider class must inherit from this class (in order to pup-up in the inspector).
    /// </summary>
    public abstract class HandTrackingBase : MonoBehaviour
    {
        public abstract HandTrackingDataProvider Type { get; }        
    }
}