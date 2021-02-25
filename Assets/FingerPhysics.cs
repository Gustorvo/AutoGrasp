using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerPhysics : MonoBehaviour
{
    [SerializeField]
    FingerPhysics _attractionFinger;

    [SerializeField]
    bool _attrack;

    Rigidbody _attractionRB;
    Rigidbody _thisRB;

    public float strenght;

    private void Start()
    {
        if (_attractionFinger != null)
            _attractionRB = _attractionFinger.GetComponent<Rigidbody>();
        _thisRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_attrack)
        {
            FaceMagnet();
            Squeeze();
        }
    }

    private void Squeeze()
    {
        if (_attractionFinger != null && _thisRB != null)
        {
            Vector3 directionToMagnet = (_attractionFinger.transform.position - transform.position).normalized;
            _thisRB.AddForce(directionToMagnet * strenght, ForceMode.Force);
        }
    }

    // face the magnet
    void FaceMagnet()
    {
        if (_attractionFinger != null && _thisRB != null)
        {
            Vector3 directionToMagnet = (_attractionFinger.transform.position - this.transform.position).normalized;
            this.transform.forward = directionToMagnet;
        }
    }
}
