using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public event Action<Collision> OnCollision;
    private void OnCollisionStay(Collision collision)
    {
        OnCollision?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        OnCollision?.Invoke(null);

    }
}
