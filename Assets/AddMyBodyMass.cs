using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMyBodyMass : MonoBehaviour
{
    public PIDTest pid;
    ArticulationBody _body;

    private void Awake()
    {
        _body = GetComponent<ArticulationBody>();        
    }

    private void OnEnable()
    {
        if (pid && _body)
        {
            pid._totalAttachedMasses += _body.mass;
            pid.totalAttachedBodies++;
        }
    }

    private void OnDisable()
    {
        if (pid && _body)
        {
            pid._totalAttachedMasses -= _body.mass;
            pid.totalAttachedBodies--;

        }
    }
}
