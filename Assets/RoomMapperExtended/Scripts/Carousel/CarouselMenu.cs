using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRoom
{
    /// <summary>
    /// Makes Carousel-like menu (of items) that rotates around specified rotation axis (in this case around a tracked hand controller)
    /// </summary>
    public class CarouselMenu : MonoBehaviour
    {
        [SerializeField] Transform _stateMachineParent;
        [SerializeField] Transform _carouselAnchor;
        [SerializeField, Range(.01f, .5f)] float _carouselRadius = 0.05f;
      
        public event Action<MenuItem> OnItemChosen;
        public event Action<int> OnStep; // direction (step) of chosen relative to the prev one: 1/-1 
        public Transform Anchor => _carouselAnchor;
        public float DistToNextItemDeg => ItemList.Count > 0 ? 360f / ItemList.Count : 0f; // distance (in degrees) between each element in carousel
        public List<MenuItem> ItemList { get; private set; } = new List<MenuItem>();
        /// <summary>
        /// index of currently chosen item in carousel
        /// </summary>
        public int Chosen
        {
            get => _chosen;
            private set
            { // circular loop => when max is reached, start from the beginning
                if (value == 0) return; // value hansn't changed (the same as previous)
                int nextChosen = _chosen + value;
                if (nextChosen > _capacity) _chosen = 0;
                else if (nextChosen < 0) _chosen = _capacity;
                else _chosen = nextChosen;
            }
        }

        private int _capacity => _itemsPositions.Length - 1;
        private int _prevIndex;
        private Vector3[] _itemsPositions;
        public int _chosen;

        private void Awake()
        {
            // populate list of menu items                
            for (int i = 0; i < _stateMachineParent.childCount; i++)
            {
                var child = _stateMachineParent.GetChild(i);
                if (child.TryGetComponent(out MenuItem newItem) && !ItemList.Contains(newItem))
                {
                    ItemList.Add(newItem);
                    newItem.Icon.transform.parent = transform; // move icon's GO to the carousel's GO
                }
            }
            RebuildCarousel();
        }
       

        private void Start()
        {
            PlaceItemsIntoCarousel();
        }

        private void PlaceItemsIntoCarousel()
        {
            for (int i = 0; i < ItemList.Count; i++)
            {
                ItemList[i].Icon.transform.localPosition = _itemsPositions[i];
            }
        }

        private void RebuildCarousel()
        {
            if (ItemList.Count >= 2)
            {
                List<Vector3> itemsPositions = new List<Vector3>();

                for (int i = 0; i < ItemList.Count; i++)
                {
                    itemsPositions.Add(GetPosition(i));
                }

                Vector3 GetPosition(int i)
                {
                    float ratio = (float)i / (float)ItemList.Count;
                    ratio *= 2f * Mathf.PI;
                    float a = Mathf.Sin(ratio);
                    float b = Mathf.Cos(ratio);
                    return new Vector3(a, b, 0f) * _carouselRadius;
                }

                _itemsPositions = itemsPositions.ToArray();           
            }
        }

        /// <summary>
        /// shifts chosen by one (-1/1)
        /// </summary>
        /// <param name="step"></param>
        public void ShiftChosen(int step)
        {
            _prevIndex = Chosen;
            Chosen = step;
            InvokeChosen(step);
        }

        // hooked in the inspector
        public void MakeItemChosen(GameObject chosenGO)
        {
            if (chosenGO.TryGetComponent(out MenuItem chosenItem) && ItemList.Contains(chosenItem))
            {
                bool clockwise = false;
                int index = ItemList.IndexOf(chosenItem);
                if (index == _prevIndex) // index hasn't changed since last shift
                {
                    InvokeChosen(0);
                    return;
                }

                // find moving (rotational) direction
                if (IsLast(index) && IsFirst(_prevIndex))
                {
                    clockwise = false;
                }
                else if (IsLast(_prevIndex) && IsFirst(index))
                {
                    clockwise = true;
                }
                else
                {
                    clockwise = index > _prevIndex;
                }
                _prevIndex = index;
                int direction = clockwise ? 1 : -1;

                Chosen = direction;
                InvokeChosen(direction);

                //RotateCarousel(direction);
            }
        }

        private void InvokeChosen(int step)
        {
            OnStep?.Invoke(step);
            OnItemChosen?.Invoke(ItemList[_chosen]);
        }

        private bool IsLast(int index) => index == ItemList.Count - 1;
        private bool IsFirst(int index) => index == 0;


    }
}