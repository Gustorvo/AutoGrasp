using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRoom
{
    public class VirtualTable : MonoBehaviour
    {
        [SerializeField] Transform _interactables;
        private MeshRenderer _renderer;
        private RoomItem _table = null;

        private List<Transform> _interactableList = new List<Transform>();

        private void Start()
        {
            
        }
        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            if (_interactables != null)
            {
                _interactableList.AddRange(_interactables.GetComponentsInChildren<Transform>().ToList());
                _interactableList.RemoveAt(0); // remove empty placeholder
                _interactableList.ForEach(i => i.GetComponent<Collider>().enabled = false);
                _interactableList.ForEach(i => i.GetComponent<Rigidbody>().isKinematic = true);
            }
            ToggleInteractables(false);

            RoomItems.OnItemAdded -= IsItemTable;
            RoomItems.OnItemAdded += IsItemTable;
        }

        private void OnDestroy()
        {
            RoomItems.OnItemAdded -= IsItemTable;
        }

        private void IsItemTable(RoomItem item)
        {
            if (item.Type.Value == ItemType.Table.Value)
            {
                _table = item;
                PlaceVirtualObj();
               // ToggleInteractables(true);
            }
        }

        private void PlaceVirtualObj()
        {
            // get the table 
            Transform tableTransform = _table.transform;

            // plan 'B'
            if (tableTransform == null)
            {
                Debug.LogError("There is no a mapped physical table to place my cubes on.");
                SetVisibility(true);
                return;
            }

            // if passthrought is disabled (most likely we're testing), enable table mesh for viz purpose (better understanding)
            if (!OVRPlugin.IsInsightPassthroughInitialized())
            {
                SetVisibility(true);
            }

            SetVisibility(false);
            // locate table surface
            float yOffset = tableTransform.localScale.y * 0.5f;
            Vector3 surfaceTop = tableTransform.position + Vector3.up * yOffset;

            // place on the table surface
            transform.position = surfaceTop;
            transform.forward = tableTransform.forward;

            //set scale
           // _interactables.parent = null;
           // transform.localScale = tableTransform.localScale;
           // _interactables.parent = transform;
        }

        public void SetVisibility(bool active)
        {
            if (_renderer)
            {
                _renderer.enabled = active;
            }
        }

        public void ToggleInteractables(bool active)
        {
            if (_interactableList.Count > 0)
            {
            _interactableList.ForEach(i => i.GetComponent<Collider>().enabled = active);
            _interactableList.ForEach(i => i.GetComponent<Rigidbody>().isKinematic = !active);
            }
        }
    }
}
