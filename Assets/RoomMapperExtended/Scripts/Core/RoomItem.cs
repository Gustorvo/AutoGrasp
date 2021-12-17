using System;
using UnityEngine;

namespace VRoom
{
    public class RoomItem : RoomItemBase
    {
        internal void SetLayer(string name)
        {
            gameObject.layer = LayerMask.NameToLayer(name);
        }
    }
}