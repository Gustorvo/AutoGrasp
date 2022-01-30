using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class FollowVRHeadset : MonoBehaviour
{
    [SerializeField] Transform _centerEye;
    [SerializeField] Transform _vRCamera;
    [SerializeField] bool _follow = true;
    public float _distance = 0f;

    private void Awake()
    {
    }

    private void Update()
    {
     if (_follow && _centerEye && _vRCamera)
        {
        _distance = Vector3.Distance(_centerEye.transform.position, transform.position);
            transform.position = _vRCamera.transform.position;// - (( _centerEye.transform.position - transform.position).normalized * _distance);
            transform.rotation = _vRCamera.transform.rotation * Quaternion.Euler(0, 0, -90f);
        }
    }
}
