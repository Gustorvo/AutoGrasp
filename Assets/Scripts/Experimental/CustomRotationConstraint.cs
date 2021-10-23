using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftHand.Experimental
{
    public class CustomRotationConstraint : MonoBehaviour
    {
        [SerializeField] Transform _source;
        [SerializeField] float _velocity = 0f;
        [SerializeField] float maximumRotateSpeed = 40;
        [SerializeField] float minimumTimeToReachTarget = 0.5f;
       // private Quaternion _initRot;

        private void Update()
        {
            Quaternion newRot = Quaternion.Euler(transform.rotation.eulerAngles.x,
                Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, _source.rotation.eulerAngles.y, ref _velocity, minimumTimeToReachTarget, maximumRotateSpeed),
                transform.rotation.eulerAngles.z);

            transform.rotation = newRot;
        }

    }
}