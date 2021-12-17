using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;

namespace VRoom
{
    public abstract class RoomItemBase : MonoBehaviour
    {
        #region Inspector
        [SerializeField, Dropdown("AllItemTypes"), OnValueChanged("SetType")]
        private string TypeName = ItemType.None.Name;
        private List<string> AllItemTypes => ItemType.AllTypes.ConvertAll(t => t.Name);
        #endregion
     
        public ItemType Type { get; private set; }

        public void SetType(ItemType type)
        {
            Type = type;
        }
        private void SetType()
        {
            Type = ItemType.FromString(TypeName);
        }
      
        protected virtual void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void OnEnable()
        {
            SetType();
        }

        private void OnValidate()
        {
            SetType();
        }
        public virtual void ConstructFromParameters(Parameters parameters)
        {

        }
    }
}