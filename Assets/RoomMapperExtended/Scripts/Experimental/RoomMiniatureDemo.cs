using Pixelplacement;
using Pixelplacement.RoomMapperDemo;
using Pixelplacement.XRTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomMiniatureDemo : RoomMapperDemoState
{
    [SerializeField] Material _wallMaterial;
    [SerializeField] Material _floorMaterial;
    [SerializeField] float _scaleFactor = 0.01f;
    [SerializeField] float _distance = .75f;
    [SerializeField] float _height = 1.25f;

    private GameObject _miniature = null;
    private bool _initialized = false;
  

    private void OnEnable()
    {
        if (!_initialized)
        {
            CreateRoomMiniature();
            return;
        }
        SetPose();
        ToggleMiniature(true);
    }
    private void OnDisable()
    {
        ToggleMiniature(false);
    }

    private void ToggleMiniature(bool active)
    {
        _miniature?.SetActive(active);
    }

    private void CreateRoomMiniature()
    {
        // no celling, so that we can see inside

        _miniature = new GameObject("Room miniature");        
       
        //get walls
        foreach (var wall in RoomMapper.Instance.Walls)
        {
            GameObject wallClone = Instantiate(wall, _miniature.transform);
            wallClone.GetComponent<Renderer>().material = _wallMaterial;
        }
       
        //get floor
        GameObject floorClone = Instantiate(RoomMapper.Instance.Floor, _miniature.transform);
        floorClone.GetComponent<Renderer>().material = _floorMaterial;

        // set scale       
        SetPose();
        _miniature.transform.localScale *= _scaleFactor;
        _miniature.AddComponent<PopInTransform>();
        _initialized = true;
    }

    private void SetPose()
    {
        Vector3 newPosition = _rig.leftEyeCamera.transform.position + _rig.leftEyeCamera.transform.forward * _distance;
        newPosition.Set(newPosition.x, _height, newPosition.z);
       
        _miniature.transform.SetPositionAndRotation(newPosition,Quaternion.identity);
    }
}
