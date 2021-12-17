using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoom
{
    public class EnableHandInteractionState : ItemPlacerState
    {
        [SerializeField] VirtualTable _table;
        private void OnEnable()
        {
            _table.ToggleInteractables(true);
        }
        private void OnDisable()
        {
            _table.ToggleInteractables(false);
        }
    }
}
