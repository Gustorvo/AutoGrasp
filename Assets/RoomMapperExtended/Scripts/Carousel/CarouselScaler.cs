using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoom
{
    [RequireComponent(typeof(CarouselMenu))]
    public class CarouselScaler : MonoBehaviour
    {      
        [SerializeField] float _upscaleFactor = 1.5f;
        [SerializeField] float _lerpTime = 1f;

        private float _downscaleFactor => _upscaleFactor * 0.5f;
        private CarouselMenu _menu;
        private float _currentLerpTime;
        private Coroutine _scaleCoroutine;

        private void Awake()
        {
            _menu = GetComponent<CarouselMenu>();
            _menu.OnStep -= Scale;
            _menu.OnStep += Scale;
        }

        private void Scale(int step)
        {
            if (_scaleCoroutine != null)
            {
                StopCoroutine(_scaleCoroutine);
            }
            _scaleCoroutine = StartCoroutine(ScaleRoutine());
        }

        private IEnumerator ScaleRoutine()
        {
            // ease in with exponential movement
            float t = 0f;
            _currentLerpTime = 0f;
            while (t != 1f)
            {
                //increment timer once per frame
                _currentLerpTime += Time.deltaTime;
                if (_currentLerpTime > _lerpTime)
                {
                    _currentLerpTime = _lerpTime;
                }
                t = _currentLerpTime / _lerpTime;
                t *= t;
                for (int i = 0; i < _menu.ItemList.Count; i++)
                {
                    Vector3 scale = _menu.ItemList[i].Icon.transform.localScale;
                    Vector3 newScale = Vector3.Lerp(scale, GetTargetScale(i), t);
                    _menu.ItemList[i].Icon.transform.localScale = newScale;
                }
                yield return null;
            }
        }

        Vector3 GetTargetScale(int i)
        {
            // find the middle
            int half = _menu.ItemList.Count / 2;
            // 'i' is chosen, scale it up!
            if (i == _menu.Chosen)
                return Vector3.one * _upscaleFactor;
            // else
            // scale down each element sequentially, bases on its distance to the chosen
            int delta = Mathf.Abs(i - _menu.Chosen);
            if (delta > half)
            {
                delta = _menu.ItemList.Count - delta;
            }            
            float decreasing = delta / 8f; // 8 is a magic number... feel free to experiment with it :)
            float scale = _downscaleFactor - decreasing;
            return Vector3.one * scale;
        }       
    }
}
