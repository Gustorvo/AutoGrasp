using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static OVRSkeleton;
using static SoftHand.Helpers;
using System;

namespace SoftHand
{
    [Serializable]
    public struct OvrHandData
    {
        public OVRSkeleton.BoneId[] Ids;      
        public Vector3[] InitialRotationEuler;
        public Vector3[] CurrentRotationEuler;
        public Vector3[] DeltaRotationEuler;
        public Vector3 HandWorldPosition;
        public Quaternion HandWorldRotation;

        public OvrHandData(int lenth) : this()
        {
            Length = lenth;
            Ids = new OVRSkeleton.BoneId[Length];           
            InitialRotationEuler = new Vector3[Length];
            CurrentRotationEuler = new Vector3[Length];
            DeltaRotationEuler = new Vector3[Length];
            HandWorldPosition = Vector3.zero;
            HandWorldRotation = Quaternion.identity;
        }

        public bool IsEmpty => Length == 0;

        public int Length { get; }
    }

    [DefaultExecutionOrder(-50)]
    public class OvrToArtDriveDataProvider : MonoBehaviour, OvrToArtDriveDataProvider.IOvrDataProvider
    {
        private OVRSkeleton.BoneId _start => _skeleton.GetCurrentStartBoneId() + _startOffset;
        private OVRSkeleton.BoneId _end => _skeleton.GetCurrentEndBoneId() - 5;
        private int _startOffset => 2;
        [SerializeField]
        OVRCustomSkeleton _skeleton;      
     
        [SerializeField]
        SkeletonMapping _skeletionMapping;
        [SerializeField]
        List<Vector3> _initialsRotationEulers;
        [SerializeField]
        List<Vector3> _rotationEulers;
        [SerializeField]
        ArtBodyTargetController _controller;

        [SerializeField]
        private List<Vector3> _rotationEulerDeltas;
        [SerializeField]
        private OvrHandData _boneData;

        public interface IOvrDataProvider
        {
            OvrHandData GetRotationData();
        }

        public interface IOvrDataConsumer
        {
            void ConsumeData();
        }

        private void Awake()
        {            
            if (_skeleton)
            {
                Init();
                FetchRotations();
               // done in Init(): _initialsRotationEulers.AddRange(_rotationEulers);
            }           
        }

        private void Init()
        {
            if ((int)_start != -1 && (int)_end != -1)
            {
                int length = _end - _start;
                if (_boneData.IsEmpty)
                    _boneData = new OvrHandData(length);
                for (int i = (int)_start; i < (int)_end; ++i)
                {
                    _boneData.Ids[i - _startOffset] = (BoneId)i;
                    _boneData.InitialRotationEuler[i-_startOffset] = GetBone(i).localRotation.FromQuaternionToDegrees();  //.eulerAngles.ToVector3s();
                }
            }
            else
                Debug.LogError("Skeleton is not initialized");
        }

        void FetchRotations()
        {
            if ((int)_start != -1 && (int)_end != -1) // -1 = bone.invalid
            {
                for (int i = (int)_start; i < (int)_end; ++i)
                {
                    _boneData.CurrentRotationEuler[i - _startOffset] = GetBone(i).localRotation.FromQuaternionToDegrees();                  
                }
            }
        }

        void CalculateDeltas()
        {
            if ((int)_start != -1 && (int)_end != -1) // -1 = bone.invalid
            {
                for (int i = (int)_start; i < (int)_end; ++i)
                {
                    int index = i - _startOffset;
                    _boneData.DeltaRotationEuler[index] = _boneData.CurrentRotationEuler[index] - _boneData.InitialRotationEuler[index];                 
                }
            }          
        }       
       
        private Transform GetBone(int i)
        {
            return _skeleton.CustomBones[i];
        }

        private void Update()
        {
           // if (_skeleton)
             //   FetchRoot();
        }

        private void FixedUpdate()
        {
            if (_skeleton)
            {
                FetchRotations();
                CalculateDeltas();
                FetchRoot();               

                if (_controller)
                    _controller.ConsumeData();
            }           
        }

        private void FetchRoot()
        {
            _boneData.HandWorldPosition = _skeleton.CustomBones[0].position;
            _boneData.HandWorldRotation = _skeleton.CustomBones[0].rotation;
        }

        OvrHandData IOvrDataProvider.GetRotationData()
        {
            return _boneData;
        }
    }
}