using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class ArtBodyController : MonoBehaviour
{
    [SerializeField]
    ArticulationBody _rootBody;

    [SerializeField]
    float _target = 0f;
    [SerializeField]
    int _lenth = 0;

    public List<float> _targets = new List<float>();
    public List<int> _startIndices = new List<int>();
    // public int _lenth = 0;
    private bool _run = true;


    [ContextMenu("Set Target For All")]
    public void SetTargetForAll()
    {
        // get the num of drives
         _rootBody.GetDofStartIndices(_startIndices);
        _targets = new List<float>();
        if (_lenth == 0)
            _lenth = _startIndices[_startIndices.Count - 1];
        else
            _lenth++;
        float rad = _target * Mathf.Deg2Rad;
        _targets.AddRange(Enumerable.Repeat(rad, _lenth));
        try
        {
            _rootBody.SetJointForces(_targets);            
        }
        catch (System.Exception e)
        {
            //_lenth++;
            Debug.Log(e);
            //SetTargetForAll();
        }
    }

    [ContextMenu("SetXDrive")]
    public void SetXDrive()
    {
        var drive = _rootBody.xDrive;
        drive.target = 10;
        _rootBody.xDrive = drive;
    }
    [ContextMenu("SetYDrive")]
    public void SetYDrive()
    {
        var drive = _rootBody.yDrive;
        drive.target = 10;
        _rootBody.yDrive = drive;

    }
    [ContextMenu("SetZDrive")]
    public void SetZDrive()
    {
        var drive = _rootBody.zDrive;
        drive.target = 10;
        _rootBody.zDrive = drive;
    }

    private void Update()
    {
        
    }
   
}
