using UnityEngine;

namespace SoftHand
{
    public class FromOVR2DriveInputs : IDriveInputs
    {
        public Vector3[] DriveTargets { get; private set; }
        public ArticulationReducedSpace[] JointPositions { get; private set; }
        public ArticulationReducedSpace[] JointForces { get; private set; }
        public IDataProvider DataProvider => _dataProvider;
        private readonly IDataProvider _dataProvider;

        public FromOVR2DriveInputs(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public void Update()
        {
            //DriveTargets = _dataProvider.GetJointsRotations
        }
    }
}