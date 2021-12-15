using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static SoftHand.Enums;

namespace SoftHand
{
    [DefaultExecutionOrder(-70)]
    public class HandsCore : MonoBehaviour
    {
        [SerializeField] List<HandTrackingBase> _handTrackingProvides = new List<HandTrackingBase>();
        [SerializeField] GameObject _handsController;
        [SerializeField] JointLimitsPreset _runtimeJointLimits;

        public static JointLimitsPreset RuntimeJointLimits { get; set; }
        public static IArticulatedHandsController HandsController { get; private set; }
        private static List<IHandTrackingDataProvider> HandTrackingProvides { get; set; }


        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            Assert.IsFalse(_handTrackingProvides.Count == 0, "There should be at least 1 hand tracking data provider");
            HandTrackingProvides = _handTrackingProvides.OfType<IHandTrackingDataProvider>().ToList();
            if (_handsController && _handsController.TryGetComponent(out IArticulatedHandsController componentWithHandsController))
            {
                HandsController = componentWithHandsController;
            }
            else
            {
                UnityEngine.Debug.LogError($"GameObject doesn't have IArticulatedHandsController attached to it");
                _handsController = null;
            }
            RuntimeJointLimits = _runtimeJointLimits;
        }

        public static IHandTrackingDataProvider GetHandTrackingDataProvider(HandTrackingDataProvider type)
        {
            return HandTrackingProvides.FirstOrDefault(x => x.Type == type);
        }

        private void OnValidate()
        {
            Init();
        }

    }
}