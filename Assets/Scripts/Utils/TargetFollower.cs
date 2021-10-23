using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollower : MonoBehaviour
{

    [SerializeField] GameObject _target;
    [SerializeField] float _smoothTime = 0.3f;
    [SerializeField] private float _threshold = 0.02f;

    private Vector3 _offset;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _prevTargetPos = Vector3.zero;
    private Transform _camera;




    private void Awake()
    {
        _offset = transform.position - _target.transform.position;
        _camera = Camera.main?.transform;
    }

    private void Update()
    {
        if (!_camera) return;
        if (Vector3.Distance(_target.transform.position, transform.position) > _threshold)
            transform.position = Vector3.SmoothDamp(transform.position, _target.transform.position + _offset, ref _velocity, _smoothTime);  //Vector3.Lerp(transform.position + _offset, _target.transform.position, Time.deltaTime * _speed);
        transform.rotation = Quaternion.LookRotation(_camera.forward);
        _prevTargetPos = _target.transform.position;



    }
}