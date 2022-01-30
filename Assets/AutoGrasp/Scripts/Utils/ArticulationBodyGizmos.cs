using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace SoftHand.Debug
{
    public class ArticulationBodyGizmos : MonoBehaviour
    {
        private List<ArticulationBody> _bodies;
        [SerializeField] ArticulationBody _ab;
        [SerializeField] bool _showCenterOfMass;
        [SerializeField] bool _showPalmBoundingBox;
        [SerializeField] bool _showHandBoundingBox;
        [SerializeField] bool _showHandBoundingSphere;
        [SerializeField] bool _showTargetJointPosition;
        [SerializeField] bool _createGameObjectsForJointPos;
        [SerializeField] Transform palmComObj, targetPalmComObj;
        [SerializeField] TMP_Text palmDeltaText;
        [SerializeField] ArticulatedHand hand;
        [SerializeField] GameObject _jointVizPrefab;

        private List<GameObject> _joints = new List<GameObject>();

        private void Awake()
        {
            if (_ab == null)
                _ab = GetComponent<ArticulationBody>();

            if (_ab != null)
                _bodies = GetComponentsInChildren<ArticulationBody>().ToList();

        }

        private void Start()
        {
            if (_createGameObjectsForJointPos)
            {
                //create game objects for visual debuging
                for (int i = 0; i < hand.Joints.Count; i++)
                {
                    GameObject newJointGO = Instantiate(_jointVizPrefab);
                    newJointGO.name = $"{hand.Joints[i].Name} {i}";
                    _joints.Add(newJointGO);
                }

            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || hand == null || !hand.Initialized)
                return;
            Pose pose = hand.Tracking.GetLastReliableRootPose(hand.Handedness);
            if (_bodies != null)
            {
                Gizmos.color = Color.red;
                if (_showCenterOfMass)
                {
                    // draw root body/palm
                    DrawSphere(hand.ArticulationBody.transform.position + hand.ArticulationBody.transform.rotation * hand.ArticulationBody.centerOfMass, 0.010f, Color.red);
                    if (palmComObj != null)
                        palmComObj.position = hand.ArticulationBody.transform.position;
                    if (targetPalmComObj != null)
                        targetPalmComObj.position = pose.position;

                    // draw fingers
                    foreach (ArticulationBody body in _bodies)
                    {
                        DrawSphere(body.transform.position + body.transform.rotation * body.centerOfMass, 0.005f, Color.red);
                    }
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

                    Bounds bounds = hand.GetHandBounds();
                    // colliders.ForEach(c => bounds.Encapsulate(c.bounds));
                    Vector3 localCenter = hand.ArticulationBody.transform.InverseTransformPoint(bounds.center);
                    Gizmos.DrawCube(pose.position + pose.rotation * localCenter, bounds.size);
                }
                if (_showHandBoundingSphere)
                {
                    if (hand.TryGetTargetHandBoundingSphere(out Vector3 center, out float radius))
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(center, radius);
                    }
                }
                if (_showTargetJointPosition)
                {
                    for (int i = 0; i < hand.Joints.Count; i++)
                    {
                        Color color = hand.Joints[i].Index == 0 ? Color.red : Color.green;
                        float size = Mathf.Clamp(0.0075f / hand.Joints[i].Index, 0.001f, 0.007f);
                        DrawSphere(hand.Joints[i].TargetData.Position, size, color);
                    }
                }
                if (_createGameObjectsForJointPos)
                {
                    for (int i = 0; i < _joints.Count; i++)
                    {
                        _joints[i].transform.SetPositionAndRotation(hand.Joints[i].TargetData.Position, hand.Joints[i].TargetData.Rotation);
                    }
                }
            }
            //if (_showBoundingCollider)
            //{
            //   if (hand.boundingSphere.GetType() == typeof(SphereCollider))
            //        DrawRedSphere(hand.boundingSphere.bounds.center)
            //}
        }

        private void DrawSphere(Vector3 center, float radius, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(center, radius);

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
                float roundedDist = Mathf.Round(Vector3.Distance(hand.BodyData.Position, hand.TargetData.Position) * 100f) * 0.01f;
                palmDeltaText.text = $"Palm delta: {roundedDist.ToString()}";

            }
        }
    }
}