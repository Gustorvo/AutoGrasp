using System;
using System.Collections.Generic;
using System.Linq;

namespace VRoom
{
    /// <summary>
    /// Enum like behaviour implementation based on https://ardalis.com/enum-alternatives-in-c/
    /// </summary>
    [Serializable]
    public class ItemType
    {
        // this must appear before other static instance types.
        public static List<ItemType> AllTypes { get; } = new List<ItemType>();

        // emtpy one
        public static ItemType None { get; } = new ItemType(0, "None");
        // static geometry
        public static ItemType Wall { get; } = new ItemType(1, "Wall");
        public static ItemType Floor { get; } = new ItemType(2, "Floor");
        public static ItemType Ceiling { get; } = new ItemType(3, "Ceiling");
        public static ItemType Window { get; } = new ItemType(4, "Window");
        public static ItemType Door { get; } = new ItemType(5, "Door");
        // non static geometry
        public static ItemType Table { get; } = new ItemType(6, "Table");
        public static ItemType Polygon { get; } = new ItemType(7, "Polygon");

        // just add your next item type here like this:
        // public static ItemType NextItem { get; } = new ItemType(nextIndex, "NextItem");



        public string Name { get; private set; }
        public int Value { get; private set; }

        private ItemType(int val, string name)
        {
            Value = val;
            Name = name;
            AllTypes.Add(this);
        }

        public static ItemType FromString(string itemString)
        {
            return AllTypes.Single(r => string.Equals(r.Name, itemString, StringComparison.OrdinalIgnoreCase));
        }

        public static ItemType FromValue(int value)
        {
            return AllTypes.Single(r => r.Value == value);
        }
    }
}