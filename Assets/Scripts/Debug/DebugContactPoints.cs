using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftHand.Debug
{
    public class DebugContactPoints : MonoBehaviour
    {
        private List<ContactPoint> _contacts = new List<ContactPoint>();
        public int _contactCount = 0;
        Color _color = Color.red;

        private void Awake()
        {

        }

        private void OnCollisionStay(Collision collision)
        {
            _contactCount = collision.contactCount;
            collision.GetContacts(_contacts);
        }
        private void OnCollisionEnter(Collision collision)
        {
            _contactCount = collision.contactCount;

            collision.GetContacts(_contacts);

        }

        private void OnCollisionExit(Collision collision)
        {
            _contacts.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            foreach (var point in _contacts)
            {
                if (point.otherCollider.gameObject.layer == LayerMask.NameToLayer("Interaction"))
                {
                    Gizmos.DrawSphere(point.point, 0.002f);
                }
            }
        }
    }
}
