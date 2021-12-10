using System.Linq;
using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand.Core
{
    public interface IArticulatedFinger
    {
        Finger Type { get; }
        ArticulatedJoint[] Joints { get; }
        TrackingConfidence Confidence { get; } // finger tracking confidance
        float Mass { get; }
    }
    public class ArticulatedFinger : MonoBehaviour, IArticulatedFinger
    {
        public Finger _type { get; set; }
        public ArticulatedJoint[] joints { get; set; }
        public TrackingConfidence _confidence { get; set; } // finger tracking confidance
                                                             // private float _mass { get; set; }

        public Finger Type => _type;

        public ArticulatedJoint[] Joints => joints;

        public TrackingConfidence Confidence => _confidence;

        public float Mass => joints.Sum(j => j.ArticulationBody.mass);

        

        //internal void ResetJoints()
        //{
        //    for (int i = 0; i < joints.Length; i++)
        //    {
        //        var zeroed = new ArticulationReducedSpace(0f, 0f, 0f);
        //        joints[i].ArticulationBody.jointPosition = zeroed;
        //        joints[i].ArticulationBody.jointAcceleration = zeroed;
        //        joints[i].ArticulationBody.jointForce = zeroed;
        //        joints[i].ArticulationBody.jointVelocity = zeroed;

        //        // reset stats
        //        joints[i].statsData.Reset();
        //    }
        //}


        //internal void ResetToLimits(int jointIndex, Vector3 jointPositoin, Vector3 overshoot, Vector3 lowerLimitsRad, Vector3 upperLimitsRad)
        //{
        //    // overshoot *= Mathf.Deg2Rad;
        //    Vector3 newJointPos = new Vector3
        //        (
        //            overshoot.x > 0 ? upperLimitsRad.x : overshoot.x < 0 ? lowerLimitsRad.x : jointPositoin.x,
        //            overshoot.y > 0 ? upperLimitsRad.y : overshoot.y < 0 ? lowerLimitsRad.y : jointPositoin.y,
        //            overshoot.z > 0 ? upperLimitsRad.z : overshoot.z < 0 ? lowerLimitsRad.z : jointPositoin.z
        //        );

        //    ArticulationReducedSpace newJointPositionsReduced = new ArticulationReducedSpace
        //    (newJointPos.x, newJointPos.y, newJointPos.z);

        //    //for (int i = 0; i < joints[jointIndex].ArticulationBody.dofCount; i++)
        //    //{
        //    //    float jointOvershoot = i == 0 ? overshoot.x : i == 1 ? overshoot.y : overshoot.z;
        //    //    += jointOvershoot;
        //    //}  
        //    joints[jointIndex].ArticulationBody.jointPosition = newJointPositionsReduced;
        //    joints[jointIndex].ArticulationBody.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
        //    joints[jointIndex].ArticulationBody.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
        //    joints[jointIndex].ArticulationBody.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
        //}

    }
}