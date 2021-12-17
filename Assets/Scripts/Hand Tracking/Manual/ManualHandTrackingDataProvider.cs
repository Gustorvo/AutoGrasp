using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftHand
{
    public class ManualHandTrackingDataProvider : HandTrackingBase, IHandTrackingDataProvider
    {
        [SerializeField] HandTrackingBase _alternativeDataProvider;
        [SerializeField] Transform _rHandRoot;
        [SerializeField] Transform _lHandRoot;
        [SerializeField] float _speed;
        [SerializeField] bool _copyFingerTrackingDataFromAlternativeProvider = true;

        private Pose[] _wristPose = new Pose[2];
        private IHandTrackingDataProvider _altDataProvider;

        public override Enums.HandTrackingDataProvider Type => Enums.HandTrackingDataProvider.Manual;
        public bool JointPositionsProvided { get; private set; }
        public bool JointRotationsProvided { get; private set; }
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            //_wristPose[0] = new Pose(Vector3.zero, Quaternion.identity);
            //_wristPose[1] = new Pose(Vector3.zero, Quaternion.identity);
        }

        public bool IsReliable(Enums.Handedness hand) => false;
        public Pose GetLastReliableRootPose(Enums.Handedness hand) => GetRootPose(hand);
        public Pose GetRootPose(Enums.Handedness hand) => _wristPose[(int)hand];



        private void LateUpdate()
        {
            for (int i = 0; i < _wristPose.Length; i++)
            {
                Transform target = i == 0 ? _lHandRoot : _rHandRoot;
                Vector3 newPosition = Vector3.Lerp(_wristPose[i].position, target.position, Time.deltaTime * _speed);
                Quaternion newRotaion = Quaternion.Slerp(_wristPose[i].rotation, target.rotation, Time.deltaTime * _speed);
                _wristPose[i] = new Pose(newPosition, newRotaion);
            }
        }
        public void Init()
        {
            if (_alternativeDataProvider != null)
            {
                _altDataProvider = (IHandTrackingDataProvider)_alternativeDataProvider;
                JointPositionsProvided = true;
                JointRotationsProvided = true;
                Snap();
            }
            else
            {
                JointPositionsProvided = false;
                JointRotationsProvided = false;
            }
            IsInitialized = true;
        }

        private void OnDisable()
        {
            IsInitialized = false;
        }

        private void OnEnable()
        {
            Init();
        }

        private void Snap()
        {
            _wristPose[0] = _altDataProvider.GetLastReliableRootPose((Enums.Handedness)0);
            _wristPose[1] = _altDataProvider.GetLastReliableRootPose((Enums.Handedness)1);

            _lHandRoot.SetPositionAndRotation(_wristPose[0].position, _wristPose[0].rotation);
            _rHandRoot.SetPositionAndRotation(_wristPose[1].position, _wristPose[1].rotation);
        }
        public Pose[] GetBonesPoses(Enums.Handedness hand)
        {
            if (_copyFingerTrackingDataFromAlternativeProvider && _altDataProvider != null)
            {
                return _altDataProvider.GetBonesPoses(hand);
            }
            throw new System.NotImplementedException();

        }
        public Enums.TrackingConfidence GetFingerConfidence(Enums.Handedness handedness, Enums.Finger finger)
        {
            if (_copyFingerTrackingDataFromAlternativeProvider && _altDataProvider != null)
            {
                return _altDataProvider.GetFingerConfidence(handedness, finger);
            }            
            throw new System.NotImplementedException();
        }

        #region not implemented
        public int GetNumberOfJoints()
        {
            throw new System.NotImplementedException();
        }

        #endregion // not implemented
    }
}
