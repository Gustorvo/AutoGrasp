using UnityEngine;

namespace SoftHand.Debug
{
        /// <summary>
    /// Displays the difference between this articulation drive target rotaion and target's rotation
    /// </summary>
    public class DebugArtDriveTargetDelta : MonoBehaviour
    {
        public float ratio;

        private ArticulationBody _body;
        private Vector3 _currentJointPosition;
        private Vector3 _prevDriveTargets;
        private Vector3 _currentDriveTargets;
        private Vector3 _prevJointPos;
        public float _traveledDistanceLocal;
        public float _driveTargetTraveledDistanceLocal;


        private void Awake()
        {
            _body = GetComponent<ArticulationBody>();

        }

        private void FixedUpdate()
        {
            if (_body == null)
                return;

            CheckTraveledDistance();
        }

        private void CheckTraveledDistance()
        {
            _currentDriveTargets = _body.GetArtBodyDriveTargets();
            Vector3 delta = (_currentDriveTargets - _prevDriveTargets).Abs();
            _driveTargetTraveledDistanceLocal += delta.x + delta.y + delta.z;
            _prevDriveTargets = _currentDriveTargets;

            for (int i = 0; i < _body.dofCount; i++)
            {
                if (i == 0) // x
                    _currentJointPosition.x = _body.jointPosition[0];
                if (i == 1) // y
                    _currentJointPosition.y = _body.jointPosition[1];
                if (i == 2) // z
                    _currentJointPosition.z = _body.jointPosition[2];
            }
            Vector3 deltaJointPos = (_currentJointPosition - _prevJointPos).Abs();
            _traveledDistanceLocal += deltaJointPos.x + deltaJointPos.y + deltaJointPos.z;
            _prevJointPos = _currentJointPosition;
            ratio = _traveledDistanceLocal > 0.001f ? _driveTargetTraveledDistanceLocal / _traveledDistanceLocal : 0f;

            if (_driveTargetTraveledDistanceLocal > 1000f)
            {
                _driveTargetTraveledDistanceLocal = 0f;
                _traveledDistanceLocal = 0f;
                _prevDriveTargets = Vector3.zero;
                _prevJointPos = Vector3.zero;
            }

        }
    }
}