using NaughtyAttributes;
using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoom
{    public class MenuItem : MonoBehaviour
    {
        [SerializeField] GameObject _icon;
        public GameObject Icon => _icon;
        public SpriteRenderer SpriteRenderer { get; private set; }

        private void Awake()
        {
            SpriteRenderer = Icon.GetComponent<SpriteRenderer>();
        }

        public void SetColor(Color newColor)
        {
            Icon.GetComponent<SpriteRenderer>().color = newColor;
        }
    }
}
