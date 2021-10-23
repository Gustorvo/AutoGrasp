using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindChildPos : MonoBehaviour
{
    public float distance;
    public GameObject objToMove;
    public GameObject child;
    public Vector3 multiplier;

    void Awake()
    {

    }

    void  Update()
    {
       
        distance = Vector3.Distance(transform.position, child.transform.position);       
        Vector3 pos = transform.position + transform.up * distance;       
        objToMove.transform.position = pos;
        objToMove.transform.rotation = child.transform.rotation;
    }
}
