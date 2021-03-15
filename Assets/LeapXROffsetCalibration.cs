using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using System;

public class LeapXROffsetCalibration : MonoBehaviour
{
    [SerializeField]
    private LeapXRServiceProvider _leapProvider;
    private bool _initialized = false;
    private KeyCode _activeKey;

    [SerializeField]
    private bool _calibrateOffset;
    private float _value = 0;

    private void Start()
    {
        if (_leapProvider != null)
            _initialized = true;
    }

    private void Update()
    {
        if (_calibrateOffset)
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                _value = 0.01f;
                MakeOffset();
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                _value = -0.01f;
                MakeOffset();
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                _activeKey = KeyCode.X;
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                _activeKey = KeyCode.Y;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                _activeKey = KeyCode.Z;
            }
        }
    }

    private void MakeOffset()
    {
        if (_activeKey == KeyCode.X)
        {
            _leapProvider.deviceTiltXAxis += _value;
        }
        if (_activeKey == KeyCode.Y)
        {

            _leapProvider.deviceOffsetYAxis += _value;
        }
        if (_activeKey == KeyCode.Z)
        {
            _leapProvider.deviceOffsetZAxis += _value;
        }
        _value = 0;
    }

}
