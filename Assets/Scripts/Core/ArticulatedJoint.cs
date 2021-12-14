using SoftHand.Extensions;
using SoftHand.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoftHand.Enums;
using static SoftHand.JointLimitsPreset;

namespace SoftHand.Core
{
    public partial struct ArticulatedJoint : IJoint
    {      
        public ArticulationBody ArticulationBody { get; }       
        public ITrackable BodyData { get; private set; }
        public ITrackable TargetData { get; private set; }
        public string Name { get; private set; }
        public int Index { get; private set; }
        public int FingerIndex { get; private set; }
        public IJointStats Stats { get; private set; }
        public float SqrDistanceToTarget => BodyData.Position.DistanceSquared(TargetData.Position);       
        public Collider Collider { get; }        
        public int Id { get; }

        public ArticulatedJoint(ArticulationBody body, string name, int index, int fingerIndex) : this()
        {            
            ArticulationBody = body;
            Id = body.GetInstanceID();                                        
            Name = name;
            Index = index;
            FingerIndex = fingerIndex;
            BodyData = new TrackableTarget(name);
            TargetData = new TrackableTarget();
            Collider = body.GetComponent<Collider>();           
        }  

        public bool IsColliding()
        {
            int layerMask = 0;
            // Get the layers that are alowed to collide with this joint     
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(ArticulationBody.transform.gameObject.layer, i))
                {
                    layerMask = layerMask | 1 << i;
                }
            }
            CapsuleCollider col = (CapsuleCollider)Collider;
            Vector3 direction = Vector3.zero;//hand.Handedness == Handedness.Right ? Vector3.right : Vector3.left;
            Vector3 endLocal = ArticulationBody.transform.localPosition + col.height * direction;
            Vector3 endWorld = ArticulationBody.transform.TransformPoint(endLocal);
            return Physics.CheckCapsule(ArticulationBody.transform.position, endWorld, col.radius, layerMask);
        }       

        public void ForceJointToPosition(Vector3 newJointPosition)
        {
            Reset();
            ArticulationReducedSpace newJointPositionsReduced = new ArticulationReducedSpace(newJointPosition.x, newJointPosition.y, newJointPosition.z);
            ArticulationBody.jointPosition = newJointPositionsReduced;
            //body.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
            //body.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
            //body.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
        }

        internal void Reset()
        {
            var zeroed = new ArticulationReducedSpace(0f, 0f, 0f);
            ArticulationBody.jointPosition = zeroed;
            ArticulationBody.jointAcceleration = zeroed;
            ArticulationBody.jointForce = zeroed;
            ArticulationBody.jointVelocity = zeroed;          
        }        
    }
}