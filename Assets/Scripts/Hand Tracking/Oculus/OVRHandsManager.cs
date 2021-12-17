using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static SoftHand.Enums;

namespace SoftHand
{
    [DefaultExecutionOrder(-70)]
    public class OVRHandsManager : MonoBehaviour
    {
        [SerializeField] OVRHand _leftHand = null;
        [SerializeField] OVRHand _rightHand = null;

        //TODO Paste some space here inbetween
        [SerializeField] bool _simulateHandsOffline;
        [SerializeField] GameObject _leftHandOffline = null;
        [SerializeField] GameObject _rightHandOffline = null;


        public static event Action<List<Transform>> OnLeftSkeletonInitialized;
        public static event Action<List<Transform>> OnRightSkeletonInitialized;

        private OVRHand[] _hand = new OVRHand[(int)OVRHand.Hand.HandRight + 1];
        private OVRSkeleton[] _handSkeleton = new OVRSkeleton[(int)OVRHand.Hand.HandRight + 1];

        private int _leftSkeletonChangedCount = -1, _rightSkeletonChangedCount = -1;
        private bool _reInitLeftHand, _reInitRightHand;

        public bool HandIsReliable(Handedness hand)
        {
            return _hand[(int)hand].IsTracked && _hand[(int)hand].HandConfidence == OVRHand.TrackingConfidence.High;
        }

        public OVRHand RightHand
        {
            get
            {
                return _hand[(int)OVRHand.Hand.HandRight];
            }
            private set
            {
                _hand[(int)OVRHand.Hand.HandRight] = value;
            }
        }

        public OVRSkeleton RightHandSkeleton
        {
            get
            {
                return _handSkeleton[(int)OVRHand.Hand.HandRight];
            }
            private set
            {
                _handSkeleton[(int)OVRHand.Hand.HandRight] = value;
            }
        }

        public OVRHand LeftHand
        {
            get
            {
                return _hand[(int)OVRHand.Hand.HandLeft];
            }
            private set
            {
                _hand[(int)OVRHand.Hand.HandLeft] = value;
            }
        }

        public OVRSkeleton LeftHandSkeleton
        {
            get
            {
                return _handSkeleton[(int)OVRHand.Hand.HandLeft];
            }
            private set
            {
                _handSkeleton[(int)OVRHand.Hand.HandLeft] = value;
            }
        }


        public bool Simulate => _simulateHandsOffline;
        public static OVRHandsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            Assert.IsNotNull(_leftHand);
            Assert.IsNotNull(_rightHand);
            Assert.IsNotNull(_leftHandOffline);
            Assert.IsNotNull(_rightHandOffline);

            //if (_simulateHandsOffline)
            //{
            //    LeftHand = _leftHandOffline.GetComponent<OVRHand>();
            //    RightHand = _rightHandOffline.GetComponent<OVRHand>();
            //}
            // else
            //  {
            LeftHand = _leftHand;
            RightHand = _rightHand;
            // }

            LeftHandSkeleton = _leftHand.GetComponent<OVRSkeleton>();
            RightHandSkeleton = _rightHand.GetComponent<OVRSkeleton>();



        }

        private void Start()
        {
            if (Simulate)
            {
                var customSkeleton = (OVRCustomSkeleton)LeftHandSkeleton;
                var customBones = customSkeleton.CustomBones;
                OnLeftSkeletonInitialized?.Invoke(customBones);

                customSkeleton = (OVRCustomSkeleton)RightHandSkeleton;
                customBones = customSkeleton.CustomBones;
                OnRightSkeletonInitialized?.Invoke(customBones);
            }
        }



        public OVRHand GetOVRHand(Handedness hand)
        {
            if (hand == Handedness.Left)
                return LeftHand;
            return RightHand;
        }

        public OVRSkeleton GetOVRSkeleton(Handedness hand)
        {
            if (hand == Handedness.Left)
                return LeftHandSkeleton;
            return RightHandSkeleton;
        }

        public bool AreInitialized()
        {
            return LeftHandSkeleton && LeftHandSkeleton.IsInitialized &&
                RightHandSkeleton && RightHandSkeleton.IsInitialized;
        }





        private void Update()
        {

            if (_leftSkeletonChangedCount != LeftHandSkeleton.SkeletonChangedCount)
            {
                _reInitLeftHand = true;
                _leftSkeletonChangedCount = LeftHandSkeleton.SkeletonChangedCount;
            }
            if (_rightSkeletonChangedCount != RightHandSkeleton.SkeletonChangedCount)
            {
                _reInitRightHand = true;
                _rightSkeletonChangedCount = RightHandSkeleton.SkeletonChangedCount;
            }


            if (_reInitLeftHand && LeftHandSkeleton.IsInitialized)
            {
                _reInitLeftHand = false;
                var bones = LeftHandSkeleton.Bones.ToList();
                List<Transform> transforms = bones.ConvertAll(b => b.Transform);
                OnLeftSkeletonInitialized?.Invoke(transforms);
            }

            if (_reInitRightHand && RightHandSkeleton.IsInitialized)
            {
                _reInitRightHand = false;
                var bones = RightHandSkeleton.Bones.ToList();
                List<Transform> transforms = bones.ConvertAll(b => b.Transform);
                OnRightSkeletonInitialized?.Invoke(transforms);
            }
        }

    }
}
