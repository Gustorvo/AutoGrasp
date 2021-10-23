using NaughtyAttributes;
using SoftHand.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static OVRHand;
using static SoftHand.ArtFinger;
using static SoftHand.ArtFinger.ArtDriveTargetPair;
using static SoftHand.Enums;
using static SoftHand.JointLimitsPreset;

namespace SoftHand
{
    // [RequireComponent(typeof(SkeletonMapping))]
    [Serializable]
    public class ArticulatedHand : MonoBehaviour
    {
        [SerializeField] Handedness _handedness;

        public Handedness Handedness => _handedness;
        public event Action<ArticulatedHand> OnInitialized;
        public ArticulationBody[] JointBodies => jointBodies;
        public bool RecordJointMinMax { get; internal set; }
        public bool CollidingWithStatic { get; private set; } // buggy
        public List<ContactPoint> CollisionContactPoints { get; private set; } = new List<ContactPoint>(10);
        public Collision Collision
        {
            get
            {
                return _collision;
            }
            set
            {
                _collision = value;
                if (_collision != null)
                {
                    CollisionContactPoints.Clear();
                    _collision.GetContacts(CollisionContactPoints);
                    if (_collision.rigidbody == null || _collision.rigidbody.isKinematic)
                    {
                        CollidingWithStatic = true;
                        // Debug.Log("Colliding with " + collision.gameObject.name);
                    }
                    else
                        CollidingWithStatic = false;
                }
            }
        }
        public float PerBoneMass { get; private set; } = 0.6f;
        public float PalmMass => Palm != null ? Palm.mass : PerBoneMass * 5f;
        public float TotalMass => AllHandJoints != null ? AllHandJoints.Count * PerBoneMass + PalmMass : PerBoneMass * 17 + PalmMass;

        internal ArticulationBody Palm { get; private set; }
        internal ArtFinger[] Fingers { get; private set; }
        internal bool Initialized { get; private set; }
        internal bool IsTrackingReliable { get; private set; }
      //  internal Vector3 LastRelialbeTargetPosition { get; private set; }
      //  internal Quaternion LastRelialbeTargetRotaion { get; private set; }
        internal Pose LastReliablePose { get; private set; }
        internal Vector3 TargetVelocity { get; private set; }
        internal float TargetSpeed { get; private set; }
        internal float DistanceToTargetSqr { get; private set; } // squared distance between palm of articulated body and target palm        

        internal ArticulationBody[] jointBodies;
        internal Quaternion[] targetRotations;

        private int _palmLayer => gameObject.layer; // for collision matrix
        private Collision _collision;
        // private SkeletonMapping _articulatedSkeleton;
        private Vector3 _palmForwardVector;
        public List<Collider> PalmColliders { get; private set; } = new List<Collider>();
        public List<ArtFinger.ArtDriveTargetPair> AllHandJoints { get; private set; } = new List<ArtFinger.ArtDriveTargetPair>();
        private RaycastHit[] _entries = new RaycastHit[16], _exits = new RaycastHit[16];
        private List<RaycastHit> _intersections = new List<RaycastHit>(32);

        private void Awake()
        {
            _palmForwardVector = _handedness == Handedness.Right ? Vector3.right : Vector3.left;

            InitializeArticulationBodies();
            targetRotations = new Quaternion[HandTrackingDataProvider.Instance.NumberOfBones];

            //if (_handedness == Handedness.Left)
            //    OVRHandsManager.OnLeftSkeletonInitialized += InitializeFingers;
            //else if (_handedness == Handedness.Right)
            //    OVRHandsManager.OnRightSkeletonInitialized += InitializeFingers;

            var colisionDetector = Palm.GetComponent<CollisionDetector>();
            if (colisionDetector) colisionDetector.OnCollision += (col) => Collision = col;

        }


        
        internal void InitializeArticulationBodies()
        {
            // get root / palm
            Palm = GetComponentInChildren<ArticulationBody>();
            // if NOT fund or found but is NOT the root...
            if (!Palm || Palm && !Palm.isRoot)
            {
                Palm = GetComponentsInChildren<ArticulationBody>().FirstOrDefault(b => b.isRoot);
                if (Palm == null)
                {
                    UnityEngine.Debug.LogError("Articulation body component is not found!");
                    return;
                }
            }
            // initialize fingers
            jointBodies = GetArticulationBodiesInHierarchy()?.ToArray();
            // palmPosedBuffer = new UnityEngine.Pose[bufferSize];
            Fingers = new ArtFinger[5];
            Fingers[0].type = Finger.Thumb;
            Fingers[1].type = Finger.Index;
            Fingers[2].type = Finger.Middle;
            Fingers[3].type = Finger.Ring;
            Fingers[4].type = Finger.Pinky;

            Fingers[0].joints = new ArtFinger.ArtDriveTargetPair[SkeletonMapping.GetNumOfJointsInFinger(0)];
            Fingers[1].joints = new ArtFinger.ArtDriveTargetPair[SkeletonMapping.GetNumOfJointsInFinger(1)];
            Fingers[2].joints = new ArtFinger.ArtDriveTargetPair[SkeletonMapping.GetNumOfJointsInFinger(2)];
            Fingers[3].joints = new ArtFinger.ArtDriveTargetPair[SkeletonMapping.GetNumOfJointsInFinger(3)];
            Fingers[4].joints = new ArtFinger.ArtDriveTargetPair[SkeletonMapping.GetNumOfJointsInFinger(4)];


            //List<string> allJoints = new List<string>();
            Transform parent = Palm.transform;
            int firstBone = (int)OVRPlugin.BoneId.Hand_Thumb0;
            int lastBone = (int)OVRPlugin.BoneId.Hand_Pinky3;
            int jointIndex = 0;
            int prevFingerIndex = -1;
            string hand = _handedness.ToString();
            for (int i = firstBone; i < lastBone + 1; ++i)
            {
                int fingerIndex = SkeletonMapping.GetFingerIndexFromBoneId(i);
                string jointName = ((OVRPlugin.BoneId)i).ToString();
                jointIndex++;
                if (prevFingerIndex != fingerIndex)
                {
                    parent = Palm.transform;
                    prevFingerIndex = fingerIndex;
                    jointIndex = 0;
                }

                //  Transform meshTarget = _meshedSkeleton?.CustomBones[i];
                // ArticulationBody body = _articulatedSkeleton.CustomBones[i].GetComponent<ArticulationBody>();
                ArticulationBody body = jointBodies[i - firstBone];
                parent = body.transform.parent;
                Fingers[fingerIndex].joints[jointIndex] = new ArtFinger.ArtDriveTargetPair(this, parent, body, /* meshTarget,*/ jointIndex, fingerIndex, /*hand +*/ jointName);
                //Fingers[fingerIndex].joints[jointIndex].Setup();
               // allJoints.Add(jointName);
            }

            AllHandJoints.AddRange(Fingers[0].joints);
            AllHandJoints.AddRange(Fingers[1].joints);
            AllHandJoints.AddRange(Fingers[2].joints);
            AllHandJoints.AddRange(Fingers[3].joints);
            AllHandJoints.AddRange(Fingers[4].joints);
                      
            PerBoneMass = Fingers[0].joints[0].body.mass;
            PalmColliders = Palm.GetComponents<Collider>().ToList();

            // remove this part!!!
            #region remove
            AllHandJoints.ForEach(x => x.body.centerOfMass = Vector3.zero); 
            Palm.centerOfMass = Vector3.zero; 
            #endregion 

            Initialized = true;
           // UpdateTargetPositionAndRotaions();
            OnInitialized?.Invoke(this);
        }

        private void OnDestroy()
        {
            //OVRHandsManager.OnLeftSkeletonInitialized -= InitializeFingers;
            //OVRHandsManager.OnRightSkeletonInitialized -= InitializeFingers;
        }

       

        /// <summary>
        /// Returns a next joint in hierarchy. ie: input(j0) -> output(j1) etc
        /// </summary>
        public bool TryGetNextJoint(ArtDriveTargetPair thisJoint, out ArtDriveTargetPair nextJoint)
        {
            nextJoint = new ArtDriveTargetPair();
            int nextIndex = AllHandJoints.IndexOf(thisJoint) + 1;
            if (nextIndex < AllHandJoints.Count && AllHandJoints[nextIndex].fingerIndex == thisJoint.fingerIndex)
            {
                nextJoint = AllHandJoints[nextIndex];
                return true;
            }
            return false;
        }

        public Bounds GetPalmBounds()
        {
            Bounds palmBounds = new Bounds();
            if (PalmColliders == null || PalmColliders.Count == 0) return palmBounds;
            palmBounds = PalmColliders[0].bounds;
            PalmColliders.ForEach(c => palmBounds.Encapsulate(c.bounds));
            return palmBounds;
        }

        public Bounds GetHandBounds()
        {
            Bounds bounds = GetPalmBounds();
            GetAlJointsColliders().ForEach(c => bounds.Encapsulate(c.bounds));
            return bounds;
        }


        public int GetCollidingLayerMask()
        {
            int _layerMask = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(_palmLayer, i))
                {
                    _layerMask = _layerMask | 1 << i;
                }
            }
            return _layerMask;
        }
        private void SetupBodyMass()
        {
            float artBodiesMass = 0f;
            var bodies = Palm.GetComponentsInChildren<ArticulationBody>()?.ToList();
            if (bodies.Count <= 1)
                return;
            //bodies.Remove(hand.palmArtBody); // remove root body since it is a base
            // if (handedness == Handedness.Left)
            //  {
            bodies.ForEach(x => x.mass = PerBoneMass);
            //palmMass = perBoneMass * 5f;
            bodies[0].mass = PalmMass;
            //  }


            // accumulate all art bodies' masses in hierarchy
            bodies.ForEach(x => artBodiesMass += x.mass);
            bodies.ForEach(x => x.useGravity = false);

            // since our root art body is attached to a rigid body via fixed joint,
            // we only need to account for rigid body mass when calculation lenear force
            // ... otherwire we'd need accumulated mass
            // totalMass = artBodiesMass;

            //set COM of eaach bone to its collider COM           

            bodies.ForEach(x => x.centerOfMass = Vector3.zero); //  = x.transform.InverseTransformPoint(x.GetComponent<Collider>().bounds.center));
            Palm.centerOfMass = Vector3.zero; //palmArtBody.transform.localPosition; //+ _palmForwardVector * palmpCenterOfMassOffset;
                                              //targetPalmCom.localPosition = _palmForwardVector * palmpCenterOfMassOffset;


            // Bounds palmBounds = bodies[0].transform.GetComponent<Collider>().bounds;

            //if (forcesPoints.Length > 2)
            //{
            //    // find center between force points (average between points)
            //    var sum = Vector3.zero;
            //    forcesPoints.ToList().ForEach(x => sum += x.position);
            //    var center = sum / forcesPoints.Length;
            //    var centerLocal = bodies[0].transform.InverseTransformPoint(center);
            //    bodies[0].centerOfMass = centerLocal;
            //    palmpCenterOfMassOffset = center - bodies[0].transform.position;
            //}
            //else
            // forcesPoints[0].parent.localPosition = bodies[0].transform.localPosition;


        }


        // TODO: call Update from ArtBodyController (to make sure all values are updated before we use them)
        private void Update()
        {
            SetTrackingConfidance();
            UpdateHandJointsData();
            UpdateTargetPositionAndRotaions();

        }

        private void FixedUpdate()
        {
            CollectJointsStatData();
            // SetDynamicFriction();
            //CheckCollisionIntersection();
        }




        private void SetDynamicFriction()
        {
            // TODO: implement dynamic friction using Experimental contacts modification API
            //https://forum.unity.com/threads/experimental-contacts-modification-api.924809/

            throw new NotImplementedException();
        }




        /// <summary>
        ///  Casts bidirection sphere to target (simulationg fist shape), returning closest position (if any) where the fist can freely move towards target
        /// </summary>
        /// <param name="targetPose"></param>
        /// <param name="nearestPose"></param>
        /// <returns></returns>
        public bool TryCheckSphereToTarget(Pose targetPose, out Pose nearestPose)
        {           
            Vector3 newPosition = Vector3.zero;
            float radius = GetFistRadius();
            Vector3 direction = targetPose.position - Palm.transform.position;
            float distance = direction.magnitude;
            direction.Normalize();
            Vector3 offsetToPalmCenter = radius * (_handedness == Handedness.Left ? Vector3.left : Vector3.right);
            Vector3 fromPos = Palm.transform.position + offsetToPalmCenter;
            Vector3 toPos = targetPose.position + offsetToPalmCenter;
            _intersections.Clear();
            Array.Clear(_entries, 0, _entries.Length);
            Array.Clear(_exits, 0, _exits.Length);
            int hitNumEntries, hitNumExits;
            int layer = GetCollidingLayerMask();

            // first check if we can freely match the target without hitting anything
            if (!Physics.CheckSphere(toPos, radius, layer))
            {
                newPosition = toPos - offsetToPalmCenter;
                nearestPose = new Pose(newPosition, targetPose.rotation);
                return true;
            }

            // otherwise do a bidirectional cast and sort hit results based on distance to target and hit normal
            hitNumEntries = Physics.SphereCastNonAlloc(fromPos, radius, direction, _entries, distance, layer);
            hitNumExits = Physics.SphereCastNonAlloc(toPos, radius, -direction, _exits, distance, layer);

            for (int i = 0; i < Mathf.Min(hitNumEntries, _entries.Length); i++)
            {
                _intersections.Add(_entries[i]);
            }

            for (int i = 0; i < Mathf.Min(hitNumExits, _exits.Length); i++)
            {
                _exits[i].distance = distance - _exits[i].distance;
                _intersections.Add(_exits[i]);
            }

            _intersections.Sort((x, y) => x.distance.CompareTo(y.distance));
            RaycastHit closest = _intersections.LastOrDefault(hit => Vector3.Dot(hit.normal, direction) < 0f);
            newPosition = closest.point - offsetToPalmCenter;
            nearestPose = new Pose(newPosition, targetPose.rotation);
            return closest.point != Vector3.zero;
        }

        internal void ToogleColliders(bool active)
        {
            PalmColliders.ForEach(c => c.enabled = active);
            AllHandJoints.ForEach(c => c.collider.enabled = active);
        }

        internal void IgnoreCollisionBetweenNeighboringJoints()
        {
            AllHandJoints.ForEach(j =>
            {
                if (TryGetNextJoint(j, out ArtDriveTargetPair nextJoint))
                    Physics.IgnoreCollision(j.collider, nextJoint.collider);
            });
        }

        /// <summary>
        /// Sets palm and fingers layer to "NoContac". 
        /// NOTE: has some side-effect (probably a bug?), where setting back to its native layer (ignore = true) couses articulation bodies ignore its collision matrix
        /// </summary>
        /// <param name="ignore"></param>
        internal void IgnoreCollision(bool ignore)
        {
            if (ignore)
            {
                int noContactrLayer = LayerMask.NameToLayer("NoContact");
                Palm.gameObject.layer = noContactrLayer;
                AllHandJoints.ForEach(b => b.body.gameObject.layer = noContactrLayer);
            }
            else
            {
                Palm.gameObject.layer = _palmLayer;
                AllHandJoints.ForEach(b => b.body.gameObject.layer = b.layer);
                Fingers.ToList().ForEach(f => f.ResetJoints());

                // IgnoreCollisionBetweenNeighboringJoints();
            }
        }

        /// <summary>
        /// Teleports hand to desired pose (No-physically way). Velocities will be zeroed.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void Teleport(Pose targetPose)
        {
           // Vector3 position, Quaternion rotation
            UnityEngine.Debug.LogWarning($"Teleporting {_handedness} hand");
            Palm.TeleportRoot(targetPose.position, targetPose.rotation);
            Palm.ResetVelocities();
            jointBodies.ToList().ForEach(b => b.ResetVelocities());
        }


        // collect some hand art bodies statistics
        private void CollectJointsStatData()
        {
            if (!Initialized || !IsTrackingReliable)
                return;
            for (int i = 0; i < Fingers.Length; i++)
            {
                //TrackingConfidence fingerConfidence = OVRHandsManager.Instance.GetOVRHand(handedness).GetFingerConfidence((OVRHand.HandFinger)fingers[i].type);
                for (int j = 0; j < Fingers[i].joints.Length; j++)
                {
                    StatsData data = Fingers[i].joints[j].statsData;
                    Vector3 currentDriveTargets = Fingers[i].joints[j].body.GetArtBodyDriveTargets();
                    Vector3 delta = (currentDriveTargets - data.prevDriveTargets).Abs();
                    data.driveTargetTraveledDistanceLocal += delta.x + delta.y + delta.z;
                    data.prevDriveTargets = currentDriveTargets;

                    Vector3 currentJointPositionLocal = Vector3.zero;
                    float dofCount = Fingers[i].joints[j].body.dofCount;
                    if (dofCount >= 1) // x
                        currentJointPositionLocal.x = Fingers[i].joints[j].body.jointPosition[0];
                    if (dofCount >= 2) // y
                        currentJointPositionLocal.y = Fingers[i].joints[j].body.jointPosition[1];
                    if (dofCount == 3) // z
                        currentJointPositionLocal.z = Fingers[i].joints[j].body.jointPosition[2];

                    Vector3 deltaJointPos =(currentJointPositionLocal - data.prevJointPos).Abs();
                    data.traveledJointDistanceLocal += deltaJointPos.x + deltaJointPos.y + deltaJointPos.z;
                    data.prevJointPos = currentJointPositionLocal;


                    if (RecordJointMinMax && Fingers[i].confidence == TrackingConfidence.High)
                    {
                        Fingers[i].joints[j].statsData.runtimeJointMinMax.Set(currentDriveTargets);
                    }

                    // reset statistics data when joints accumulated traveled distance > 100 degrees
                    if (data.driveTargetTraveledDistanceLocal > 100)
                    {
                        data.actualTravelledRatio = data.traveledJointDistanceLocal != 0 ? data.driveTargetTraveledDistanceLocal / data.traveledJointDistanceLocal : 0;
                        data.driveTargetTraveledDistanceLocal = 0;
                        data.traveledJointDistanceLocal = 0;
                        // traveledJointDistanceLocal = 0f;
                        //driveTargetTraveledDistanceLocal = 0f;
                        // ratio = 0f;
                    }
                    data.jointPositionLocal = currentJointPositionLocal;
                    data.jointPositionsSqrMag = currentJointPositionLocal.Abs().sqrMagnitude;
                    // data.maxJointLimitsReached = data.jointPositionsSqrMag > data.maxJointPositionsSqrMagLocal;
                    bool overshootOnX = Mathf.Clamp(currentJointPositionLocal.x, data.lowerLimitsRad.x, data.upperLimitsRad.x) != currentJointPositionLocal.x;
                    bool overshootOnY = Mathf.Clamp(currentJointPositionLocal.y, data.lowerLimitsRad.y, data.upperLimitsRad.y) != currentJointPositionLocal.y;
                    bool overshootOnZ = Mathf.Clamp(currentJointPositionLocal.z, data.lowerLimitsRad.z, data.upperLimitsRad.z) != currentJointPositionLocal.z;
                    if (overshootOnX || overshootOnY || overshootOnZ)
                    {
                        // TODO: calculate the overshoot value and compare it against joint limits + some threshold
                        // raise event only if overshoot is significant

                        float overlimitRad = Mathf.Sqrt(data.jointPositionsSqrMag - data.maxJointPositionsSqrMagLocal);
                        // if (data.IsOvershooting(currentJointPositionLocal, out Vector3 overshoot))
                        //  Debug.LogWarning($"{fingers[i].joints[j].jointName} is over limit {overshoot * Mathf.Rad2Deg} degrees");
                        // Debug.Break();
                    }

                    Fingers[i].joints[j].statsData = data;
                }
            }
        }


        private void SetTrackingConfidance()
        {
            if (OVRHandsManager.Instance != null && OVRHandsManager.Instance.Simulate)
                IsTrackingReliable = true;

            else if (Initialized)
            {
                IsTrackingReliable = HandTrackingDataProvider.Instance.IsHandReliable(_handedness);
                for (int i = 0; i < Fingers.Length; i++)
                {
                    Fingers[i].confidence = HandTrackingDataProvider.Instance.GetFingerConfidence(_handedness, (OVRHand.HandFinger)Fingers[i].type);
                }
            }
        }

        private void UpdateHandJointsData()
        {
            if (!Initialized) return;
            for (int i = 0; i < Fingers.Length; i++)
            {
                for (int j = 0; j < Fingers[i].joints.Length; j++)
                {
                    Fingers[i].joints[j].velocity = (Fingers[i].joints[j].body.transform.position - Fingers[i].joints[j].prevPosition) / Time.fixedDeltaTime;
                    Fingers[i].joints[j].prevPosition = Fingers[i].joints[j].body.transform.position;
                }
            }
        }

        /// <summary>
        /// Rought approximation of hand fist radius.
        /// Returns the sphere radius between wrist and second joint of middle finger.
        /// </summary>
        /// <returns></returns>
        public float GetFistRadius()
        {
            // TODO: implement a propper (accurate) way to calculate a fist radius
            return Vector3.Distance(Palm.transform.position, Fingers[2].joints[1].body.transform.position) * 0.5f;
        }

        public List<Collider> GetAlJointsColliders()
        {
            return AllHandJoints.ConvertAll(c => c.collider);
        }

        /// <summary>
        /// Returns the average weighted center of mass in world space for the hand (palm + fingers)
        /// </summary>
        /// <returns></returns>
        private Vector3 GetHandCenterOfMass()
        {
            Vector3 CoM = Vector3.zero;
            float c = 0f;
            foreach (var finger in Fingers)
            {
                foreach (var joint in finger.joints)
                {
                    CoM += joint.body.worldCenterOfMass * PerBoneMass;
                    c += PerBoneMass;
                }
            }
            CoM += Palm.worldCenterOfMass;
            c += Palm.mass;
            return CoM / c;
        }

        private void UpdateTargetPositionAndRotaions()
        {
            if (Initialized /*&& IsTrackingReliable*/)
            {
                if (!IsTrackingReliable)
                {
                    return;
                }
                // update velocity vector and speed 
                UnityEngine.Pose palmPose = HandTrackingDataProvider.Instance.GetPalmPose(_handedness);
                Vector3 delta = palmPose.position - LastReliablePose.position;
                TargetVelocity = delta / Time.fixedDeltaTime;
                TargetSpeed = delta.magnitude / Time.fixedDeltaTime;
                DistanceToTargetSqr = palmPose.position.DistanceSquared(Palm.transform.position);

                // update position and rotation               
               // LastRelialbeTargetPosition = palmPose.position;
               // LastRelialbeTargetRotaion = palmPose.rotation;
                LastReliablePose = palmPose;

                // update bones              
                Array.Copy(HandTrackingDataProvider.Instance.GetBoneRotaions(_handedness), targetRotations, targetRotations.Length);

                int index = 0;
                Transform parent = Palm.transform;
                float distToParent = 0;
                for (int i = 0; i < Fingers.Length; i++)
                {
                    for (int j = 0; j < Fingers[i].joints.Length; j++)
                    {
                        parent = Fingers[i].joints[j].parant;
                        distToParent = Fingers[i].joints[j].distanceToParent;
                        Vector3 jointPosition = parent.position + parent.up * distToParent;
                        Pose newPose = new Pose(jointPosition, targetRotations[index]);
                        Fingers[i].joints[j].targetPose = newPose;
                        index++;
                    }
                }
            }
        }

        List<ArticulationBody> GetArticulationBodiesInHierarchy()
        {
            List<ArticulationBody> articulatedBonesUnsorted = Palm.GetComponentsInChildren<ArticulationBody>().ToList();
            List<ArticulationBody> articulatedBonesSorted = new List<ArticulationBody>();

            OVRPlugin.BoneId start = OVRPlugin.BoneId.Hand_Thumb0;
            OVRPlugin.BoneId end = OVRPlugin.BoneId.Hand_Pinky3;

            for (int bi = (int)start; bi < (int)end + 1; ++bi)
            {
                string fbxBoneName = SkeletonMapping.FbxBoneNameFromBoneIndex(_handedness, (OVRSkeleton.BoneId)(OVRPlugin.BoneId)bi);
                ArticulationBody ab = articulatedBonesUnsorted.FirstOrDefault(b => b.gameObject.name.Contains(fbxBoneName));

                if (ab != null)
                {
                    articulatedBonesSorted.Add(ab);
                }
            }

            return articulatedBonesSorted;
        }

    }

    [Serializable]
    public struct ArtFinger
    {
        public Finger type;
        public ArtDriveTargetPair[] joints; // joints in this finger       
        public TrackingConfidence confidence; // finger tracking confidance


        internal void ResetJoints()
        {
            for (int i = 0; i < joints.Length; i++)
            {
                var zeroed = new ArticulationReducedSpace(0f, 0f, 0f);
                joints[i].body.jointPosition = zeroed;
                joints[i].body.jointAcceleration = zeroed;
                joints[i].body.jointForce = zeroed;
                joints[i].body.jointVelocity = zeroed;

                // reset stats
                joints[i].statsData.Reset();
            }
        }


        //internal void ResetToLimits(int jointIndex, Vector3 jointPositoin, Vector3 overshoot, Vector3 lowerLimitsRad, Vector3 upperLimitsRad)
        //{
        //    // overshoot *= Mathf.Deg2Rad;
        //    Vector3 newJointPos = new Vector3
        //        (
        //            overshoot.x > 0 ? upperLimitsRad.x : overshoot.x < 0 ? lowerLimitsRad.x : jointPositoin.x,
        //            overshoot.y > 0 ? upperLimitsRad.y : overshoot.y < 0 ? lowerLimitsRad.y : jointPositoin.y,
        //            overshoot.z > 0 ? upperLimitsRad.z : overshoot.z < 0 ? lowerLimitsRad.z : jointPositoin.z
        //        );

        //    ArticulationReducedSpace newJointPositionsReduced = new ArticulationReducedSpace
        //    (newJointPos.x, newJointPos.y, newJointPos.z);

        //    //for (int i = 0; i < joints[jointIndex].body.dofCount; i++)
        //    //{
        //    //    float jointOvershoot = i == 0 ? overshoot.x : i == 1 ? overshoot.y : overshoot.z;
        //    //    += jointOvershoot;
        //    //}  
        //    joints[jointIndex].body.jointPosition = newJointPositionsReduced;
        //    joints[jointIndex].body.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
        //    joints[jointIndex].body.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
        //    joints[jointIndex].body.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
        //}


        [Serializable]
        public struct ArtDriveTargetPair
        {
            public Transform parant;
            public float distanceToParent;
            public ArticulatedHand hand; // the hand this pair belongs to
            public int layer;
            public string jointName;
            public int index;
            public int fingerIndex;
            public ArticulationBody body;
            public Pose targetPose;
            public Vector3 velocity;
            public Vector3 prevPosition;
            public Vector3 targetCoM; // center of mass of target bone
            public StatsData statsData;
            public Collider collider;
            public float colliderRadius;
            public UnityEngine.Pose[] poseBuffer;
            // public Transform meshTarget;


            public ArtDriveTargetPair(ArticulatedHand hand, Transform parent, ArticulationBody articulationBody, /*Transform meshTarget, */int jointIndex, int fingerIndex, string jointName) : this()
            {
                this.parant = parent;
                this.hand = hand;
                this.body = articulationBody;
                this.index = jointIndex;
                this.fingerIndex = fingerIndex;
                this.jointName = jointName;
                this.statsData = new StatsData(articulationBody);
                this.collider = body.transform.GetComponent<Collider>();
                this.layer = body.gameObject.layer;

                distanceToParent = Vector3.Distance(parent.position, body.transform.position);
                Vector3 initialRotationInRedcedSpace = body.ToTargetRotationInReducedSpace(targetPose.rotation);
                statsData.runtimeJointMinMax = new DriveMinMax(initialRotationInRedcedSpace, jointName);
                // poseBuffer = new UnityEngine.Pose[bufferSize];
                // this.meshTarget = meshTarget;
            }

            //internal void Setup(int bufferSize = 3)
            //{
            //}

            public bool IsColliding()
            {
                int layerMask = 0;
                // Get the layers that are alowed to collide with this joint     
                for (int i = 0; i < 32; i++)
                {
                    if (!Physics.GetIgnoreLayerCollision(layer, i))
                    {
                        layerMask = layerMask | 1 << i;
                    }
                }
                CapsuleCollider col = (CapsuleCollider)collider;
                Vector3 direction = hand.Handedness == Handedness.Right ? Vector3.right : Vector3.left;
                Vector3 endLocal = body.transform.localPosition + (col.height * direction);
                Vector3 endWorld = body.transform.TransformPoint(endLocal);
                return Physics.CheckCapsule(body.transform.position, endWorld, col.radius, layerMask);

            }

            public bool IsOvershooting(out Vector3 overshootRadians)
            {
                overshootRadians = Vector3.zero;
                float threshold = 1.5f * Mathf.Deg2Rad;

                if (statsData.jointPositionLocal.x > statsData.upperLimitsRad.x + threshold)
                    overshootRadians.x = statsData.jointPositionLocal.x - statsData.upperLimitsRad.x;
                else if (statsData.jointPositionLocal.x < statsData.lowerLimitsRad.x - threshold)
                    overshootRadians.x = statsData.jointPositionLocal.x - statsData.lowerLimitsRad.x;

                if (statsData.jointPositionLocal.y > statsData.upperLimitsRad.y + threshold)
                    overshootRadians.y = statsData.jointPositionLocal.y - statsData.upperLimitsRad.y;
                else if (statsData.jointPositionLocal.y < statsData.lowerLimitsRad.y - threshold)
                    overshootRadians.y = statsData.jointPositionLocal.y - statsData.lowerLimitsRad.y;

                if (statsData.jointPositionLocal.z > statsData.upperLimitsRad.z + threshold)
                    overshootRadians.z = statsData.jointPositionLocal.z - statsData.upperLimitsRad.z;
                else if (statsData.jointPositionLocal.z < statsData.lowerLimitsRad.z - threshold)
                    overshootRadians.z = statsData.jointPositionLocal.z - statsData.lowerLimitsRad.z;

                return overshootRadians != Vector3.zero;
            }

            internal void ResetToLimits(Vector3 overshoot)
            {
                Vector3 newJointPos = new Vector3
                    (
                        overshoot.x > 0 ? statsData.upperLimitsRad.x : overshoot.x < 0 ? statsData.lowerLimitsRad.x : statsData.jointPositionLocal.x,
                        overshoot.y > 0 ? statsData.upperLimitsRad.y : overshoot.y < 0 ? statsData.lowerLimitsRad.y : statsData.jointPositionLocal.y,
                        overshoot.z > 0 ? statsData.upperLimitsRad.z : overshoot.z < 0 ? statsData.lowerLimitsRad.z : statsData.jointPositionLocal.z
                    );

                ArticulationReducedSpace newJointPositionsReduced = new ArticulationReducedSpace(newJointPos.x, newJointPos.y, newJointPos.z);

                body.jointPosition = newJointPositionsReduced;
                body.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
                body.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
                body.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
            }



            public struct StatsData
            {
                public DriveMinMax runtimeJointMinMax; // record joints' max/min values 
                public Vector3 jointPositionLocal;
                public float jointPositionsSqrMag;
                //  public bool maxJointLimitsReached;
                public int dofCount;
                public bool hasXDrive, hasYDrive, hasZDrive;
                public float maxJointPositionsSqrMagLocal; // the maximum possible squared magnitude of this joint in local space (in radians)
                public Vector3 lowerLimitsRad, upperLimitsRad; // in radians              
                public Vector3 lowerLimitsDeg, upperLimitsDeg; // in degrees
                public Vector3 prevJointPos;
                public Vector3 prevDriveTargets;
                public float traveledJointDistanceLocal;
                public float driveTargetTraveledDistanceLocal; // in radians
                public float actualTravelledRatio; // driveTargetTraveledDistanceLocal / traveledJointDistanceLocal. used to be ~ 1 radian (57) for a 'healthy' joint, or > 500 for a buggy one
                                                   //  private ArticulationBody articulationBody;

                public StatsData(ArticulationBody articulationBody) : this()
                {
                    // this.articulationBody = articulationBody;
                    dofCount = articulationBody.dofCount;

                    lowerLimitsDeg.x = dofCount >= 1 ? articulationBody.xDrive.lowerLimit : 0;
                    lowerLimitsDeg.y = dofCount >= 2 ? articulationBody.yDrive.lowerLimit : 0;
                    lowerLimitsDeg.z = dofCount == 3 ? articulationBody.zDrive.lowerLimit : 0;

                    upperLimitsDeg.x = dofCount >= 1 ? articulationBody.xDrive.upperLimit : 0;
                    upperLimitsDeg.y = dofCount >= 2 ? articulationBody.yDrive.upperLimit : 0;
                    upperLimitsDeg.z = dofCount == 3 ? articulationBody.zDrive.upperLimit : 0;

                    lowerLimitsRad = lowerLimitsDeg * Mathf.Deg2Rad;
                    upperLimitsRad = upperLimitsDeg * Mathf.Deg2Rad;

                    maxJointPositionsSqrMagLocal = (lowerLimitsRad.Abs() + (upperLimitsRad).Abs()).sqrMagnitude;

                    hasXDrive = articulationBody.twistLock != ArticulationDofLock.LockedMotion && articulationBody.jointType != ArticulationJointType.FixedJoint;
                    hasYDrive = articulationBody.swingYLock != ArticulationDofLock.LockedMotion && articulationBody.jointType != ArticulationJointType.FixedJoint;
                    hasZDrive = articulationBody.swingZLock != ArticulationDofLock.LockedMotion && articulationBody.jointType != ArticulationJointType.FixedJoint;
                }

                //public bool IsOvershooting(out Vector3 overshootRadians)
                //{
                //    overshootRadians = Vector3.zero;
                //    float threshold = 1.5f * Mathf.Deg2Rad;

                //    if (jointPositionLocal.x > upperLimitsRad.x + threshold)
                //        overshootRadians.x = jointPositionLocal.x - upperLimitsRad.x;
                //    else if (jointPositionLocal.x < lowerLimitsRad.x - threshold)
                //        overshootRadians.x = jointPositionLocal.x - lowerLimitsRad.x;

                //    if (jointPositionLocal.y > upperLimitsRad.y + threshold)
                //        overshootRadians.y = jointPositionLocal.y - upperLimitsRad.y;
                //    else if (jointPositionLocal.y < lowerLimitsRad.y - threshold)
                //        overshootRadians.y = jointPositionLocal.y - lowerLimitsRad.y;

                //    if (jointPositionLocal.z > upperLimitsRad.z + threshold)
                //        overshootRadians.z = jointPositionLocal.z - upperLimitsRad.z;
                //    else if (jointPositionLocal.z < lowerLimitsRad.z - threshold)
                //        overshootRadians.z = jointPositionLocal.z - lowerLimitsRad.z;

                //    return overshootRadians != Vector3.zero;
                //}

                internal void Reset()
                {
                    traveledJointDistanceLocal = 0f;
                    driveTargetTraveledDistanceLocal = 0f;
                    actualTravelledRatio = 0f;
                }
            }
        }

    }
}
