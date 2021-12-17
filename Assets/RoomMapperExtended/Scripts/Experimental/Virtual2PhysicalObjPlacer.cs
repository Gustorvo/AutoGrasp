using Pixelplacement.XRTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoom
{

    /// <summary>
    /// Places cubes on a mapped surface/table
    /// </summary>
    public class Virtual2PhysicalObjPlacer : MonoBehaviour
    {
        const string _tooltip = "this is a plan 'b'. If there is no a mapped physical table, cubes will be place as usual/as before (on a 3d model)";
        [SerializeField, Tooltip(_tooltip)] VirtualTable _virtualTable;
        [SerializeField] GameObject _vObjToPlace;
        
        private RoomItem _table = null;

        private void Awake()
        {
            // we want to make sure to position the cubes JUST AFTER the environment has been set, 
            // otherwise the position won't match and cubes will fall onto the floor       
            RoomItems.OnItemAdded += IsItemTable;
        }

        private void IsItemTable(RoomItem item)
        {
            if (item.Type.Value == ItemType.Table.Value)
            {
                _table = item;
                PlaceVirtualObj();
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
                _virtualTable.SetVisibility(true);
                return;
            }

            //// if passthrought is disabled (most likely we're testing), enable table mesh for viz purpose (better understanding)
            //if (!OVRPlugin.IsInsightPassthroughInitialized())
            //{
            //    tableTransform.GetComponent<MeshRenderer>().enabled = true;
            //}

            _virtualTable.SetVisibility(false);
            // locate table surface
            float yOffset = tableTransform.localScale.y * 0.5f;
            Vector3 surfaceTop = tableTransform.position + Vector3.up * yOffset;

            // place on the table surface
            _vObjToPlace.transform.position = surfaceTop;
            _vObjToPlace.transform.forward = tableTransform.forward;
        }

    }
}