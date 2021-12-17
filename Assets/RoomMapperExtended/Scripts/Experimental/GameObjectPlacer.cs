using Pixelplacement.XRTools;
using System;
using System.Linq;
using UnityEngine;

// Make sure the "Queries hit backfaces" is checked in project settings (Physics tab)
// as we do a bidirectional raycast!
public class GameObjectPlacer : MonoBehaviour
{
    [SerializeField] GameObject _objToPlace;
    [SerializeField] PlacementPos _placementPos = PlacementPos.hitPoint;
    [SerializeField] PlacementTarget _target = PlacementTarget.none;
    [SerializeField] float _maxDistanceToTarget = 20f;
    [SerializeField, Tooltip(placeToCenterTolltip)] bool _placeToCenterIfHitFails = true;

    // public GameObject _debugTarget;
    // public LayerMask _debugTargetLayerMask;
    public bool WasPlaced => _wasPlaced;
    private bool _wasPlaced = false;
    private const string placeToCenterTolltip = "this will make sure the obj will be placed even if the hit misses the target";

    private void OnEnable()
    {
        Place();
    }

    GameObject GetTarget()
    {
        switch (_target)
        {
            case PlacementTarget.floor:
                return RoomMapper.Instance?.Floor;
            case PlacementTarget.wallBefind:
            case PlacementTarget.wallInfront:
                // the wall whose forward direction is the same as our gameobject is most likely our target
                // ... however, might not be the case if our object has wrong heading vector
                return RoomMapper.Instance?.Walls.ToList().OrderByDescending(w => Vector3.Dot(_objToPlace.transform.forward, w.transform.forward)).First();
            case PlacementTarget.ceiling:
                throw new NotImplementedException();
            case PlacementTarget.none:
            default:
                return null;
        }
    }

    private void Place()
    {
        if (_target == PlacementTarget.none || _wasPlaced)
        {
            return;
        }
        Vector3 oldPos = _objToPlace.transform.position;
        Vector3 newPos = Vector3.zero;
        Vector3 forward = Vector3.zero;
        GameObject target = GetTarget();// : _debugTarget;    
        float aboveOrUnder = Mathf.Sign(Vector3.Dot(_objToPlace.transform.up, target.transform.position - _objToPlace.transform.position)); // is the target above or under us
        Vector3 dirToTarget =
            _target == PlacementTarget.floor ? _objToPlace.transform.up * aboveOrUnder :
            _target == PlacementTarget.wallBefind ? -_objToPlace.transform.forward :
            _target == PlacementTarget.wallInfront ? _objToPlace.transform.forward :
            Vector3.zero;

        if (Physics.Raycast(_objToPlace.transform.position, dirToTarget, out RaycastHit hit, maxDistance: _maxDistanceToTarget, layerMask: 1 << target.layer))
        {
            forward = _target == PlacementTarget.floor ? hit.transform.forward : hit.normal;
            newPos = hit.point;
        }
        // we couldn't hit the target, should we snap to target insted?
        else if (_placeToCenterIfHitFails)
        {
            _placementPos = PlacementPos.center;
            forward = target.transform.forward;
            newPos = target.transform.position;
        }
        _objToPlace.transform.forward = forward;
        _objToPlace.transform.position = newPos;

        _wasPlaced = newPos != oldPos;
    }

    //private void Update()
    //{
    //    Place();

    //}
}