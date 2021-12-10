using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoftHand.Interfaces;


namespace SoftHand
{
    public class FromLeap2DriveInputs : IDriveInputs
    {
        public Vector3[] DriveTargets => throw new System.NotImplementedException();

        public ArticulationReducedSpace[] JointPositions => throw new System.NotImplementedException();

        public ArticulationReducedSpace[] JointForces => throw new System.NotImplementedException();

        public IDataProvider DataProvider => throw new System.NotImplementedException();
    }
}