using System.Linq;
using UnityEngine;
using static SoftHand.ArticulatedHandSettingsPreset;
using static SoftHand.ArticulationBodySettings;

namespace SoftHand
{
    public class QuickSetupArticulation : MonoBehaviour
    {
        [SerializeField] ArticulationBody _ab;
        [SerializeField] float _stiftness, _damping, _forceLimit;
        [SerializeField] float _lowerLimit, _upperLimit;


        [ContextMenu("SetupAB")]
        public void SetupAB()
        {
            ArticulationDriveSettings driveSettings = new ArticulationDriveSettings();
            driveSettings.motor.damping = _damping;
            driveSettings.motor.stiffness = _stiftness;
            driveSettings.motor.forceLimit = _forceLimit;
            driveSettings.minMaxLimits.Set(_lowerLimit, _upperLimit); // new       
                                                                      // driveSettings.limits.lowerLimit = _lowerLimit;
                                                                      // driveSettings.limits.upperLimit= _upperLimit;
            if (_ab != null)
            {
                var bodies = _ab.GetComponentsInChildren<ArticulationBody>()?.ToList();
                foreach (var body in bodies)
                {
                    if (body.isRoot)
                        continue;
                    body.xDrive = body.SetupDrive(driveSettings);
                    body.yDrive = body.SetupDrive(driveSettings);
                    body.zDrive = body.SetupDrive(driveSettings);
                }
            }
        }
    }
}