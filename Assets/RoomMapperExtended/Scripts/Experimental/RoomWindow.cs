using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomWindow : MonoBehaviour
{  
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && collision.gameObject.TryGetComponent(out MeshFilter meshFilter))
        {
            // TODO: find a more accurate way to orient windows after recovery/app relaunch
           // Quaternion rot = Quaternion.LookRotation(-meshFilter.mesh.normals[0]);
          //  transform.rotation = rot;
        }
    }
    
}
