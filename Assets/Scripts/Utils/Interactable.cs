using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] Rigidbody _rb;
    [SerializeField] MeshRenderer _mesh;
    public int bufferSize = 2;
    public bool bufferFull;
    private Pose[] poseBuffer;
    
    private int _frame;
    private int _sleepingFrames;

    private void Start()
    {
        poseBuffer = new Pose[bufferSize]; 
    }

    private void FixedUpdate()
    {
        FillPoseBuffers();

        if (bufferFull)
        {
            var pose = GetAverageSmoothPose(poseBuffer);
            _mesh.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }

    private void FillPoseBuffers()
    {
        if (!_rb || bufferSize == 0 || _sleepingFrames >= bufferSize)
        {
            bufferFull = false;
            return;
        }

        _sleepingFrames = _rb.IsSleeping() ? _sleepingFrames++ : 0;

        if (_frame > 0 && _frame == bufferSize)
        {
            _frame = 0;
            bufferFull = true;
        }
        poseBuffer[_frame] = new Pose(_rb.position, _rb.rotation);      
        _frame++;
    }

    private Pose GetAverageSmoothPose(Pose[] poses)
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
