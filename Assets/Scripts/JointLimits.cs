using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoftHand.Enums;

namespace SoftHand
{
    //[ExecuteInEditMode]
    public class JointLimits : MonoBehaviour
    {
        public OVRCustomSkeleton skeleton;
        public int bonesCount;
        public float offset = 5;
        public List<FingerBoneLimits> fingerBoneIds;
        List<string> boneNames;
        List<Vector3> boneRotValues;

        private void Awake()
        {
            if (skeleton == null)
                skeleton = GetComponent<OVRCustomSkeleton>();
            if (skeleton == null)
                skeleton = transform.parent.GetComponent<OVRCustomSkeleton>();
            fingerBoneIds = new List<FingerBoneLimits>();
            boneNames = GetBoneNames();
            boneRotValues = new List<Vector3>();
            FetchRotations(ref boneRotValues);

            for (int i = 0; i < boneNames.Count; i++)
            {
                fingerBoneIds.Add(new FingerBoneLimits(i, boneNames[i], boneRotValues[i]));
            }

        }


        private void FixedUpdate()
        {
            FetchRotations(ref boneRotValues);
            if (skeleton.IsDataHighConfidence)
            {
                for (int i = 0; i < fingerBoneIds.Count; i++)
                {
                    fingerBoneIds[i].SetDelta(boneRotValues[i]);
                }
            }
        }

        private void Start()
        {

        }

        void FetchRotations(ref List<Vector3> rotationEulers)
        {
            rotationEulers.Clear();
            OVRSkeleton.BoneId start = skeleton.GetCurrentStartBoneId() + 2;
            OVRSkeleton.BoneId end = skeleton.GetCurrentEndBoneId() - 5;
            if ((int)start != -1 && (int)end != -1) // -1 = bone.invalid
            {
                for (int i = (int)start; i < (int)end; ++i)
                {
                    Vector3 rot = GetBoneRotation(i).localRotation.ToInspectorEulerVector();
                    rotationEulers.Add(rot);
                }
            }
        }

        private Transform GetBoneRotation(int i)
        {
            //if (Application.isPlaying)
            //{
            //    return skeleton.Bones[i].Transform;
            //}
            return skeleton.CustomBones[i];
        }

        private List<string> GetBoneNames()
        {
            List<string> names = new List<string>();
            OVRSkeleton.BoneId start = skeleton.GetCurrentStartBoneId() + 2;
            OVRSkeleton.BoneId end = skeleton.GetCurrentEndBoneId() - 5;
            if ((int)start != -1 && (int)end != -1) // -1 = bone.invalid
            {
                for (int i = (int)start; i < (int)end; ++i)
                {
                    FingerBoneId fingerBoneName = (FingerBoneId)i;
                    names.Add(fingerBoneName.ToString());
                }
            }
            return names;
        }



        private void OnValidate()
        {
            // Awake();
        }
    }

    [Serializable]
    public class FingerBoneLimits
    {
        public int Id;
        public int DOFs;
        public string Name;
        public Vector3 InitialRot;
        public Vector3 MinDelta, MaxDelta;

        private float maxX, maxY, maxZ, minX, minY, minZ;
        // public Transform transform;
        public FingerBoneLimits(int id, string name, Vector3 rotation)// : this()
        {
            Id = id;
            Name = name;
            InitialRot = rotation;
            MinDelta = InitialRot;//Vector3.zero;
            MaxDelta = InitialRot; // Vector3.zero;

            maxX = InitialRot.x;
            maxY = InitialRot.y;
            maxZ = InitialRot.z;
            minX = InitialRot.x;
            minY = InitialRot.y;
            minZ = InitialRot.z;
        }

        public void SetDelta(Vector3 newRot)
        {
            if (newRot.x > maxX)
                maxX = newRot.x;
            if (newRot.y > maxY)
                maxY = newRot.y;
            if (newRot.z > maxZ)
                maxZ = newRot.z;

            if (newRot.x < minX)
                minX = newRot.x;
            if (newRot.y < minY)
                minY = newRot.y;
            if (newRot.z < minZ)
                minZ = newRot.z;

            MaxDelta = new Vector3(maxX, maxY, maxZ) - InitialRot;
            MinDelta = new Vector3(minX, minY, minZ) - InitialRot;

            //check for offset error
            if (Math.Abs(maxX - minX) < 7)
            {
                MaxDelta.x = 0;
                MinDelta.x = 0;
            }
            if (Math.Abs(maxY - minY) < 7)
            {
                MaxDelta.y = 0;
                MinDelta.y = 0;
            }
            if (Math.Abs(maxZ - minZ) < 7)
            {
                MaxDelta.z = 0;
                MinDelta.z = 0;
            }

            // round floats
            MaxDelta.x = (int)MaxDelta.x;
            MaxDelta.y = (int)MaxDelta.y;
            MaxDelta.z = (int)MaxDelta.z;
            MinDelta.x = (int)MinDelta.x;
            MinDelta.y = (int)MinDelta.y;
            MinDelta.z = (int)MinDelta.z;

            int dofs = 0;
            if (MaxDelta.x > 0 || MinDelta.x < 0)
                dofs++;
            if (MaxDelta.y > 0 || MinDelta.y < 0)
                dofs++;
            if (MaxDelta.z > 0 || MinDelta.z < 0)
                dofs++;
            DOFs = dofs;

        }
    }
}