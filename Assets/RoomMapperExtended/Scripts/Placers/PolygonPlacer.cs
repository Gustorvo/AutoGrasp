using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRoom
{
    public class PolygonPlacer : ItemPlacerState
    {
        [SerializeField] List<Vector3> _vertsPositions = new List<Vector3>();
        [SerializeField] Material _material;
        [SerializeField, Range(0.05f, 1f)] float _pointerLength = 0.25f;
        [SerializeField] LineRenderer _selection, _pointer, _testSelection;
        [SerializeField] GameObject _vertPrefab;
        [SerializeField] GameObject _text;

        private List<GameObject> _vertsGOs = new List<GameObject>();
        private Vector3 _cursorPos;
        private Vector3 _lastVertPos = Vector3.zero;
        private bool _selectionLoopClosed;

        private Vector3 _fromRayPosition => _rig.rightControllerAnchor.position;
        private Vector3 _rayDirection => _rig.rightControllerAnchor.forward;
        private float _distanceToFirstVert => _vertsPositions.Count >= 1 ? Vector3.Distance(_vertsPositions[0], _cursorPos) : 0f;
        private bool _canCloseSelectionLoop => _vertsPositions.Count >= 3 && _distanceToFirstVert < 0.025f;
        private Vector3 _selectionCenter => new Vector3(
        _vertsPositions.Average(x => x.x),
        _vertsPositions.Average(x => x.y),
        _vertsPositions.Average(x => x.z));

        private void Start()
        {
            _pointer.positionCount = 2;
            _pointer.loop = false;

            _testSelection.positionCount = 2;
            _testSelection.loop = false;
            _testSelection.gameObject.SetActive(false);
            _text.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            if (_selectionLoopClosed)
            {
                ConstructPolygon();
                CleanSelection();
            }

            VisualizeTestSelection();

            _cursorPos = _fromRayPosition + _rayDirection * _pointerLength;
            _pointer.SetPosition(0, _fromRayPosition);
            _pointer.SetPosition(1, _cursorPos);


            // enable visuals asking if we wanna close the selection loop
            if (_canCloseSelectionLoop)
            {
                _text.transform.position = _cursorPos;
                _text.transform.rotation = Quaternion.LookRotation(_rig.centerEyeAnchor.forward);
                _text.SetActive(true);
            }
            else
            {
                _text.SetActive(false);
            }

            TakeInput();
        }

        private void VisualizeTestSelection()
        {
            if (_lastVertPos != Vector3.zero)
            {
                _testSelection.gameObject.SetActive(true);
                _testSelection.SetPosition(0, _cursorPos);
                _testSelection.SetPosition(1, _lastVertPos);
            }
        }

        void TakeInput()
        {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                if (_canCloseSelectionLoop)
                {
                    _selectionLoopClosed = true;
                }
                else
                {
                    _lastVertPos = _cursorPos;
                    _vertsPositions.Add(_cursorPos);
                    _vertsGOs.Add(Instantiate(_vertPrefab, _cursorPos, Quaternion.identity));
                }
                VisualizeVerts(_canCloseSelectionLoop);
            }
        }
        private void CleanSelection()
        {
            _vertsPositions.Clear();
            _vertsGOs.ForEach(o => Destroy(o));
            _selection.positionCount = 0;
            _selectionLoopClosed = false;
            _lastVertPos = Vector3.zero;
            _testSelection.gameObject.SetActive(false);
        }

        private void VisualizeVerts(bool close)
        {
            _selection.positionCount = _vertsPositions.Count;
            _selection.SetPositions(_vertsPositions.ToArray());
            _selection.loop = close;
            _selection.gameObject.SetActive(true);           
        }

        private void ConstructPolygon()
        {
            Parameters parameters = new Parameters(_selectionCenter, Quaternion.LookRotation(_rig.centerEyeAnchor.forward), Vector3.one, _vertsPositions.ToArray(), -_rig.centerEyeAnchor.forward);
            var polygon = _geometry.BuildRoomItem(ItemType.Polygon, parameters);
            RoomItems.Instance.AddItem(polygon);            
        }
    }
}
