using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRSkeleton;

namespace SoftHand
{
    [RequireComponent(typeof(SkeletonMapping))]
    public class ArticulatedSkeleton : MonoBehaviour
    {
        private SkeletonMapping _skeletonMapping;
        public IList<OVRBone> OVRBones => _skeletonMapping.OVRSkeletonProvider.Bones;
        private Transform _ovrRootPose => _skeletonMapping.OVRSkeletonProvider.transform;

           

        private void Awake()
        {
            _skeletonMapping = GetComponent<SkeletonMapping>();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (_skeletonMapping.ShouldInitialize())
            {
                _skeletonMapping.Initialize();
            }
#endif

            if (!_skeletonMapping.IsInitialized)
            {
                //IsDataValid = false;
                //IsDataHighConfidence = false;
                return;
            }

            if (_skeletonMapping.OVRSkeletonProvider.IsDataValid)
            {
                //if (SkeletonChangedCount != data.SkeletonChangedCount)
                //{
                //	SkeletonChangedCount = data.SkeletonChangedCount;
                //	IsInitialized = false;
                //	Initialize();
                //}				

                //_update Root Pose

                _skeletonMapping.RootPose.localPosition = _ovrRootPose.transform.localPosition;
                _skeletonMapping.RootPose.transform.localRotation = _ovrRootPose.transform.localRotation;


                //if (_updateRootScale)
                //{
                //transform.localScale = new Vector3(data.RootScale, data.RootScale, data.RootScale);
                //}

                for (var i = 0; i < _skeletonMapping.bones.Count; ++i)
                {
                    if (_skeletonMapping.bones[i].Transform != null)
                    {                      
                        _skeletonMapping.bones[i].Transform.localRotation = OVRBones[i].Transform.localRotation;                      
                    }
                }
            }
        }
    }
}