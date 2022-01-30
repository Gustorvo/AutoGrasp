using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SoftHand.Debug
{
    public class DebugArtBodyProtertiesUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _xFractionOvershootText, _yFractionOvershootText, _zFractionOvershootText;
        [SerializeField] TMP_Text _xOvershootText, _yOvershootText, _zOvershootText;
        [SerializeField] DebugArtBodyAdaptiveDamping _dampController;

        private ArticulationBody _ab;
        private List<float> _forces;
        private Vector3 _abLowerLimits, _abUpperLimits;
        private Vector3 _fraction, _overshootPercentage;

        private void Awake()
        {
            _forces = new List<float>(3);
            _ab = GetComponent<ArticulationBody>();
        }

        private void Start()
        {
           // UnityEngine.Debug.Log($"{gameObject.name} {_ab.jointForce.dofCount}");

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_ab == null)
                return;


            if (_xFractionOvershootText != null && _yFractionOvershootText != null && _zFractionOvershootText != null)
            {
                _xFractionOvershootText.text = $"Ang damp: {_ab.angularDamping}";
                _yFractionOvershootText.text = $"Lin damp: {_ab.linearDamping}";
                _zFractionOvershootText.text = $"Ovrsht mag: {Mathf.Round(_dampController.overshootPercentage.magnitude * 100.0f) * 0.01f}";
            }

            if (_xOvershootText != null && _yOvershootText != null && _zOvershootText != null)
            {
                if (_dampController == null)
                    return;
                _xOvershootText.text = $"x:{Mathf.Round(_dampController.jointPosition.x * Mathf.Rad2Deg * 100f) * 0.01f}";
                _yOvershootText.text = $"y:{Mathf.Round(_dampController.jointPosition.y * Mathf.Rad2Deg * 100f) * 0.01f}";
                _zOvershootText.text = $"z:{Mathf.Round(_dampController.jointPosition.z * Mathf.Rad2Deg * 100f) * 0.01f}";

            }

        }
    }
}