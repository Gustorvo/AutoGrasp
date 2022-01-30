using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SoftHand
{
    public class Deparenter : MonoBehaviour
    {
        [SerializeField] GameObject _parent;
        [SerializeField] PhysicMaterial _material;

        List<CapsuleCollider> _childColldiers = new List<CapsuleCollider>();

        private void Start()
        {
            _childColldiers = _parent.GetComponentsInChildren<CapsuleCollider>().ToList();           
            for (int i = 0; i < _childColldiers.Count; i++)
            {
                CopyComponent(gameObject, _childColldiers[i]);
            }
            
                Destroy(_parent);
            
        }

        public CapsuleCollider CopyComponent( GameObject destination, CapsuleCollider original)
        {
            float scaleFactor = (original.transform.localScale.z + original.transform.localScale.y + original.transform.localScale.x) / 3f;
            float parentScaleFactor = (original.transform.parent.localScale.z + original.transform.parent.localScale.y + original.transform.parent.localScale.x) /3f;
            CapsuleCollider copy = destination.AddComponent<CapsuleCollider>();
            copy.center = original.center + original.transform.localPosition + original.transform.parent.localPosition + _parent.transform.localPosition;
            copy.radius = original.radius * scaleFactor * parentScaleFactor;
            copy.height = original.height * scaleFactor * parentScaleFactor;
            copy.direction = original.direction;
            copy.material = _material;
            return copy;
           
        }
    }
}
