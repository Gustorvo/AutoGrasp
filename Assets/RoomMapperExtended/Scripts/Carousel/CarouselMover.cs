using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoom
{
    [RequireComponent(typeof(CarouselMenu))]
    public class CarouselMover : MonoBehaviour
    {
        [Header("Spring values")]
        [SerializeField] float _moveDamping = 0.5f;
        [SerializeField] float _moveFrequency = 3f;
        /// <summary>
        /// is carousel currently moming/rotating?
        /// </summary>
        public bool IsMoving => _velocity != 0f;

        private CarouselMenu _menu;
        private float _velocity;
        private float _curAngleZ;
        private float _targetAngleZ;

        private void Awake()
        {
            _menu = GetComponent<CarouselMenu>();
            _curAngleZ = 0.0f;
            _velocity = 0.0f;
            _menu.OnStep -= RotateCarousel;
            _menu.OnStep += RotateCarousel;
        }

        private void Update()
        {
            transform.position = _menu.Anchor.position;

            // tween position
            NumericSpring(ref _curAngleZ, ref _velocity, _targetAngleZ, _moveDamping, _moveFrequency * Mathf.PI, Time.deltaTime);
            Quaternion newRot = _menu.Anchor.rotation * Quaternion.AngleAxis(_curAngleZ, Vector3.forward);
            transform.rotation = newRot;
            if (IsMoving) 
            {         
                // by setting forward vector, we essentially rotate each item to align with up vector
                _menu.ItemList.ForEach(i => i.Icon.transform.forward = _menu.Anchor.forward);
            }
        }
        private void RotateCarousel(int step)
        {
            _targetAngleZ += _menu.DistToNextItemDeg * step;
        }

        //  The variables curValue and velocity are initialized once and then
        //  passed into the function by reference every frame,
        //  where the function keeps updating their values every time it’s called.
        // source: http://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/
        void NumericSpring(ref float curValue, ref float velocity, float targetValue, float damping, float angFreq, float timeStep)
        {
            // it may take very long time untill spring comes to rest...
            // to prevent this, we are checking if the current value is is close enought to the target
            if (Mathf.Abs(curValue - targetValue) < 0.001f)
            {
                curValue = targetValue;
                velocity = 0f;
                return;
            }

            float f = 1.0f + 2.0f * timeStep * damping * angFreq;
            float oo = angFreq * angFreq;
            float hoo = timeStep * oo;
            float hhoo = timeStep * hoo;
            float detInv = 1.0f / (f + hhoo);
            float detX = f * curValue + timeStep * velocity + hhoo * targetValue;
            float detV = velocity + hoo * (targetValue - curValue);
            curValue = detX * detInv;
            velocity = detV * detInv;
        }

        #region Debug
        [NaughtyAttributes.Button("Next")]
        public void DebugNext()
        {
            _menu.ShiftChosen(1);
        }

        [NaughtyAttributes.Button("Previous")]
        public void DebugPrev()
        {
            _menu.ShiftChosen(-1);
        }
        #endregion
    }
}
