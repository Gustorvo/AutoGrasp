using System;
using UnityEngine;

namespace SoftHand
{
    public class HandMover : IBodyMover
    {
      //  public IArticulatedHand Hand => hand;
        public IForceSettings ForceSettings { get; private set; }
        public ITorqueSettings TorqueSettings { get; private set; }
        public IBody BodyToMove =>_hand;

        public event Action OnTeleport;

        private readonly IBody _hand;
        private readonly ArticulationBody _handBody;


        // private readonly IArticulatedHand hand;

        public HandMover(IBody handBody, IForceSettings forceSettings, ITorqueSettings torqueSettings )
        {
            //this.hand = hand;
            this._hand = handBody;
            this._handBody = handBody.ArticulationBody;
            ForceSettings = forceSettings;
            TorqueSettings = torqueSettings;
        }

        public void MoveBody()
        {
            Vector3 linearForce = BodyToMove.ArticulationBody.CalculateLinearForce(_hand.TargetData.Position, ForceSettings.ToVelocity, ForceSettings.MaxVelocity, ForceSettings.MaxForce, ForceSettings.Gain);
            _handBody.AddForce(linearForce * ForceSettings.LinearForceWeight * _hand.ArticulationBody.mass);
          //  UnityEngine.Debug.Log(linearForce);
        }

        public void RotateBody()
        {
            Vector3 angularForce = _handBody.CalculateRequiredTorque(_hand.TargetData.Rotation, TorqueSettings.Frequency, TorqueSettings.Damping);
            _handBody.AddTorque(angularForce * TorqueSettings.AngularForceWeight);
           // UnityEngine.Debug.Log(angularForce);           
        }        

        public void TeleportBody(Pose target)
        {
            UnityEngine.Debug.LogWarning($"Teleporting {_handBody.name}");
            _handBody.immovable = true;
            _handBody.TeleportRoot(target.position, target.rotation);
            _handBody.immovable = false;
            OnTeleport?.Invoke();
        }
    }
}