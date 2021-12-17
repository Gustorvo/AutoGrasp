using Pixelplacement;
using System;

namespace VRoom
{
    /// <summary>
    /// base class for all 'room-item-placer' classes
    /// </summary>
    public class ItemPlacerState : State
    {
        protected OVRCameraRig _rig;
        protected GeometryBuilder _geometry;
       // public static event Action<int> OnStateChanged; // where int values is step: +1 forward, -1 backward        

        int lastState => transform.parent.childCount - 1;


        protected virtual void Awake()
        {
            _rig = FindObjectOfType<OVRCameraRig>();
            _geometry = FindObjectOfType<GeometryBuilder>();
        }


        protected virtual void Update()
        {
            //next:
            if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch))
            {
                if (IsLast) // handle cyclic loop
                {
                    StateMachine.ChangeState(StateMachine.defaultState);
                }
                else
                {
                    Next();
                }
               // OnStateChanged?.Invoke(1);
            }

            //previous:
            if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch))
            {
                if (IsFirst) // handle cyclic loop
                {
                    StateMachine.ChangeState(lastState);
                }
                else
                {
                    Previous();
                }
               // OnStateChanged?.Invoke(-1);
            }
        }
    }
}