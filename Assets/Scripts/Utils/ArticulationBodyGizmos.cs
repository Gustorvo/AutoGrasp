using SoftHand;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ArticulationBodyGizmos : MonoBehaviour
{

    private List<ArticulationBody> _bodies;
    [SerializeField] ArticulationBody _ab;
    [SerializeField] bool _showCenterOfMass;
    [SerializeField] bool _showPalmBoundingBox;
    [SerializeField] bool _showHandBoundingBox;
    [SerializeField] Transform palmComObj, targetPalmComObj;
    [SerializeField] TMP_Text palmDeltaText;
    [SerializeField] ArticulatedHand hand;

    private void Awake()
    {
        if (_ab == null)
            _ab = GetComponent<ArticulationBody>();

        if (_ab != null)
            _bodies = GetComponentsInChildren<ArticulationBody>().ToList();

    }

    private void Start()
    {

    }

    private void OnDrawGizmos()
    {
        if (hand == null || !hand.Initialized)
            return;
        if (_showCenterOfMass && _bodies != null)
        {
            // draw root body/palm
            DrawRedSphere(hand.Palm.transform.position + hand.Palm.transform.rotation * hand.Palm.centerOfMass, 0.010f);
            if (palmComObj != null)
                palmComObj.position = hand.Palm.transform.position;
            if (targetPalmComObj != null)
                targetPalmComObj.position = hand.LastReliablePose.position;

            // draw fingers
            foreach (ArticulationBody body in _bodies)
            {
                DrawRedSphere(body.transform.position + body.transform.rotation * body.centerOfMass, 0.005f);
            }

            if (_showPalmBoundingBox)
            {
                // draw palm bounding box
                Bounds bounds = hand.GetPalmBounds();
                Gizmos.DrawCube(bounds.center, bounds.size);
            }
            if (_showHandBoundingBox)
            {
                // draw palm bounding box
                var colliders = hand.GetAlJointsColliders();
                if (colliders.Count == 0) return;

                Bounds bounds = hand.GetPalmBounds();
                colliders.ForEach(c => bounds.Encapsulate(c.bounds));
                Vector3 localCenter = hand.Palm.transform.InverseTransformPoint(bounds.center);
                Gizmos.DrawCube(hand.LastReliablePose.position + hand.LastReliablePose.rotation * localCenter, bounds.size);
            }
        }
        //if (_showBoundingCollider)
        //{
        //   if (hand.boundingSphere.GetType() == typeof(SphereCollider))
        //        DrawRedSphere(hand.boundingSphere.bounds.center)
        //}
    }

    private void DrawRedSphere(Vector3 center, float radi)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, radi);
    }

    private void Update()
    {
        if (!hand)
            return;
        UpdateText();
    }

    private void UpdateText()
    {

        if (palmDeltaText != null)
        {
            float roundedDist = Mathf.Round(Mathf.Sqrt(hand.DistanceToTargetSqr) * 100f) * 0.01f;
            palmDeltaText.text = $"Palm delta: {roundedDist.ToString()}";

        }
    }
}
