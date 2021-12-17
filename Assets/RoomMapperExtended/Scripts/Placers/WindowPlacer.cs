using System.Collections.Generic;
using UnityEngine;

namespace VRoom
{    public class WindowPlacer : ItemPlacerState
    {
        [SerializeField] LineRenderer _pointer;
        [SerializeField] LineRenderer _selection;
        [SerializeField] Transform _cursor;      

        private Vector3 _firstCorner;
        private Vector3 _secondCorner;
        private RaycastHit _hit;
        private bool _firstCornerSet = false;
        private Vector3 _horizontal;
        private Vector3 _vertical;
        private LayerMask _targetLayer => LayerMask.NameToLayer(ItemType.Wall.Name);

        protected override void Awake()
        {
            base.Awake();        
        }
        
        protected override void Update()
        {
            base.Update();
            //scan:          
            if (Physics.Raycast(_rig.rightControllerAnchor.position, _rig.rightControllerAnchor.forward, out _hit, maxDistance: 99f, layerMask: 1 << _targetLayer))
            {
                //pointer:
                _pointer.gameObject.SetActive(true);
                _pointer.SetPosition(0, _rig.rightControllerAnchor.position);
                _pointer.SetPosition(1, _hit.point);

                //cursor:
                _cursor.gameObject.SetActive(true);
                _cursor.position = _hit.point + _hit.normal * .001f; //otherwise there will be z sorting with the surface
                _cursor.rotation = Quaternion.LookRotation(_hit.normal); //orient to surface

                // window mapper
                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
                {
                    // first click sets first corner and activates selection
                    if (!_firstCornerSet)
                    {
                        _firstCorner = _hit.point;
                        PlaceFirstCorner();
                    }
                    else
                    { // second click finalizes and closes selection
                        MakeWindow();
                    }
                }
                _secondCorner = _hit.point;
                MakeSelection();
            }
            else
            {
                DisableRays();
            }
        }

        public void SaveWindows()
        {
            throw new System.NotImplementedException();
        }

        private void MakeWindow()
        {
            _firstCornerSet = false;

            // instantiate window prefab
            Vector3 center = (_secondCorner + _firstCorner) / 2;
            Quaternion rotation = Quaternion.LookRotation(-_hit.normal);
            Vector3 scale = new Vector3(Vector3.Distance(_firstCorner, _horizontal), Vector3.Distance(_firstCorner, _vertical), 1f);
            Parameters parameters = new Parameters(center, rotation, scale);
            var window = _geometry.BuildRoomItem(ItemType.Window, parameters);
            RoomItems.Instance.AddItem(window);
        }

        private void PlaceFirstCorner()
        {
            _selection.positionCount = 4;
            _selection.loop = true;
            _selection.SetPositions(new Vector3[] { _firstCorner, _firstCorner });
            _selection.gameObject.SetActive(true);
            _firstCornerSet = true;
        }

        private void MakeSelection()
        {
            if (!_firstCornerSet) return;

            Vector3 diagonalDir = (_secondCorner - _firstCorner).normalized;
            Vector3 wallForward = _hit.normal;
            Vector3 wallUp = Vector3.zero;
            Vector3 wallRight = Vector3.zero;
            Vector3.OrthoNormalize(ref wallForward, ref wallUp, ref wallRight);

            float dist = Vector3.Distance(_firstCorner, _secondCorner);
            float angle = Vector3.SignedAngle(diagonalDir, wallRight, wallForward) * Mathf.Deg2Rad;
            _horizontal = _firstCorner + wallRight * dist * Mathf.Cos(angle);
            _vertical = _firstCorner + wallUp * dist * Mathf.Sin(angle);

            _selection.SetPosition(0, _firstCorner);
            _selection.SetPosition(1, _horizontal);
            _selection.SetPosition(2, _secondCorner);
            _selection.SetPosition(3, _vertical);
        }

        void DisableRays()
        {
            _pointer.gameObject.SetActive(false);
            _cursor.gameObject.SetActive(false);
            _selection.gameObject.SetActive(false);
            _firstCornerSet = false;
        }
    }
}