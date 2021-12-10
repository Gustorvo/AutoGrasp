using SoftHand.Core;
using UnityEngine;

namespace SoftHand.Experimental
{
    /// <summary>
    /// Filters out noisy hand tracking data (position + rotation),
    /// by providing weighted average smoothing position and rotation.
    /// Might be usefull to reduce tracking jitter (for non-realtime) use cases
    /// </summary>
    public class PoseBuffer : MonoBehaviour
    {
        [SerializeField] int bufferSize;
        public bool bufferFull { get; private set; }
        private ArticulatedHand _hand;
        private Pose[] palmPosedBuffer;
        private int _frame = 0;
        private int _prevFrame = 0;
        private int _prevPrevFrame = 0;

        private void Awake()
        {
            _hand.GetComponent<ArticulatedHand>();
        }

        private void Update()
        {
            if (_hand)
                FillPoseBuffer();
        }

        private void FillPoseBuffer()
        {
            if (!_hand.Initialized || !_hand.Tracking.IsHandReliable(_hand.Handedness) || bufferSize == 0)
            {
                bufferFull = false;
                return;
            }

            if (_frame > 0 && _frame == bufferSize)
            {
                _frame = 0;
                bufferFull = true;
            }

            if (bufferFull && palmPosedBuffer.Length >= 3)
            {
                _prevFrame = _frame - 1;
                _prevPrevFrame = _prevFrame - 1;

                if (_prevFrame < 0)
                    _prevFrame = palmPosedBuffer.Length + _prevFrame;
                if (_prevPrevFrame < 0)
                    _prevPrevFrame = palmPosedBuffer.Length + _prevPrevFrame;
            }
            palmPosedBuffer[_frame] = new Pose(_hand.ArticulationBody.transform.position, _hand.ArticulationBody.transform.rotation);
          
                //for (int j = 0; j < _hand.Joints.Length; j++)
                //{
                //    _hand.Joints[j].poseBuffer[_frame] = new Pose(_hand.Joints[j].ArticulationBody.transform.position, _hand.Joints[j].ArticulationBody.transform.rotation);
                //    if (Vector3.Distance(_hand.Joints[j].poseBuffer[_frame].position, _hand.Joints[j].poseBuffer[_prevPrevFrame].position) < Vector3.Distance(_hand.Joints[j].poseBuffer[_frame].position, _hand.Joints[j].poseBuffer[_prevFrame].position))
                //    {
                //        _hand.Joints[j].poseBuffer[_frame] = _hand.Joints[j].poseBuffer[_prevPrevFrame];
                //        // Debug.LogWarning(" Finger jitter");
                //    }
                //}           

            if (bufferFull && palmPosedBuffer.Length >= 3)
            {
                if (Vector3.Distance(palmPosedBuffer[_frame].position, palmPosedBuffer[_prevPrevFrame].position) < Vector3.Distance(palmPosedBuffer[_frame].position, palmPosedBuffer[_prevFrame].position))
                {
                    palmPosedBuffer[_frame] = palmPosedBuffer[_prevPrevFrame];
                    // Debug.LogWarning("Palm jitter");
                }
            }
            _frame++;
        }

        Pose GetAverageSmoothPose(Pose[] poses)
        {
            Vector3 accumulatedPosition = Vector3.zero;
            Vector4 accumulatedRotation = Vector4.zero;
            for (int i = 0; i < poses.Length; i++)
            {
                Pose curPose = poses[i];
                accumulatedPosition += curPose.position;
                accumulatedRotation += new Vector4(curPose.rotation.x, curPose.rotation.y, curPose.rotation.z, curPose.rotation.w);
            }

            Vector4 averageRotationVector = accumulatedRotation / poses.Length;
            Quaternion averageRotation = new Quaternion(averageRotationVector.x, averageRotationVector.y, averageRotationVector.z, averageRotationVector.w).normalized;

            return new Pose(accumulatedPosition / poses.Length, averageRotation);
        }
    }
}