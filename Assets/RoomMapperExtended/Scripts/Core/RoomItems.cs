using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace VRoom
{
    /// <summary>
    /// holds reference and handles logic for all scanned room items (windows, table, door etc)
    /// </summary>
    public class RoomItems : MonoBehaviour
    {
        public static event Action<RoomItem> OnItemAdded;
        public static RoomItems Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<RoomItems>();
                }

                return _instance;
            }
        }
        private static RoomItems _instance;

        private Dictionary<ItemType, List<RoomItem>> _itemsByType = new Dictionary<ItemType, List<RoomItem>>();
        Vector3[,] WindowsPoses { get; } // TODO refactor

        private void OnValidate()
        {
            CreateLayerForEachItemType();
        }

        private static void CreateLayerForEachItemType()
        {
            foreach (var item in ItemType.AllTypes)
            {
                if (item.Value != 0) // skip the empty one
                    LayerMaskEx.CreateLayer($"{item.Name}");
            }
        }

        public bool TryGetItems(ItemType type, out List<RoomItem> items)
        {
            if (_itemsByType.TryGetValue(type, out items))
            {
                return true;
            }
            return false;
        }

        public void AddItem(RoomItem item)
        {
            if (!_itemsByType.ContainsKey(item.Type))
            {
                _itemsByType.Add(item.Type, new List<RoomItem>());
            }           
            _itemsByType[item.Type].Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void AddItem(ItemType type, GameObject itemGO)
        {
            RoomItem newRoomItem = itemGO.AddComponent<RoomItem>();
            // newRoomItem.Type = type;
            newRoomItem.SetType(type);
            newRoomItem.SetLayer(type.Name);         
            AddItem(newRoomItem);
        }

        public void AddItems(ItemType type, List<GameObject> itemGOs)
        {
            itemGOs.ForEach(i => AddItem(type, i));
        }

        public void AddItems(List<RoomItem> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                AddItem(items[i]);
            }
        }

        public bool TryRemove(RoomItem item)
        {
            if (!_itemsByType.TryGetValue(item.Type, out List<RoomItem> items))
            {
                return items.Remove(item);
            }
            return false;
        }

        public void DestroyAll()
        {
            foreach (KeyValuePair<ItemType, List<RoomItem>> items in _itemsByType)
            {
                items.Value.ForEach(i => Destroy(i.gameObject));
            }
            _itemsByType.Clear();
        }
    }
}