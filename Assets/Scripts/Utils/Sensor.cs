using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoftHand
{
    public class Sensor : MonoBehaviour, ISensor
    {
        private Collision _collision;
        public List<ContactPoint> CollisionContactPoints { get; private set; } = new List<ContactPoint>(10);
        public bool CollidingWithStatic { get; private set; } // buggy


        public event Action<Collision> OnCollision;
        public Collision Collision
        {
            get => _collision;
            set
            {
                _collision = value;
                if (_collision != null)
                {
                    CollisionContactPoints.Clear();
                    CollidingWithStatic = (_collision.rigidbody == null || _collision.rigidbody.isKinematic);
                    _collision.GetContacts(CollisionContactPoints);
                }
            }
        }
    }
}