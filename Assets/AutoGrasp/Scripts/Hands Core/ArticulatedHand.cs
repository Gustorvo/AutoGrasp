using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static SoftHand.Enums;

using System.Collections.ObjectModel;
using NaughtyAttributes;

namespace SoftHand
{

    [Serializable]
    public class ArticulatedHand : MonoBehaviour, IArticulatedHand
    {
        [SerializeField] Handedness _handedness = Handedness.None;
        [SerializeField, OnValueChanged("ReinitializeTrackingProvider")] HandTrackingDataProvider _handTrackingProvider = HandTrackingDataProvider.Oculus;      
        [SerializeField] ForceSettings _forceSettings = null;
        [SerializeField] TorqueSettings _torqueSettings = null;

        public Handedness Handedness => _handedness;      
        public IForceSettings ForceSettings { get; private set; }
        public ITorqueSettings TorqueSettings { get; private set; }
        public IHandTrackingDataProvider Tracking { get; private set; }
        public IJointStatsController RuntimeStats { get; private set; }
        public bool Initialized { get; private set; } = false;
        public float TotalMass { get; private set; } //=> Palm.mass + Joints.Sum(x => x.ArticulationBody.mass);                                                    
        public Action OnDriveTargetsSet { get; private set; }

        #region Things to refactor
        public bool RecordJointMinMax { get; internal set; }
        #endregion

        public float TargetHandBoundingSphereRadius => GetHandBounds().extents.magnitude;
        public List<Collider> PalmColliders { get; private set; } = new List<Collider>();

        internal Pose[] _targetJointsPoseBuffer;
        private int _handLayer => gameObject.layer; // for collision matrix        


        public ITrackable BodyData { get; private set; } = new TrackableTarget("root");
        public ITrackable TargetData { get; private set; } = new TrackableTarget("OVR hand");
        public ArticulationBody ArticulationBody { get; private set; } // the root (palm/wrist)

        private List<ArticulatedJoint> _joints = new List<ArticulatedJoint>();
        public ReadOnlyCollection<IJoint> Joints { get; private set; }
        public float SqrDistanceToTarget => BodyData.Position.DistanceSquared(TargetData.Position);
        public event Action<IArticulatedHand> OnInitialized;
        public Action OnTeleport { get; private set; }

        public int InstanceId => gameObject.GetInstanceID();

        private List<ArticulationBody> _jointArticulationBodies;
        private int _firstBone = (int)OVRPlugin.BoneId.Hand_Thumb0;
        private int _lastBone = (int)OVRPlugin.BoneId.Hand_Pinky3;

        //private void OnValidate()
        //{
        //    bool prefabModeIsActive = String.IsNullOrEmpty(gameObject.scene.path) && String.IsNullOrEmpty(gameObject.scene.name);
        //    if (!prefabModeIsActive)
        //    {
        //        Init();
        //    }
        //}
        public void Init()
        {
            if (Initialized)
            {
               // OnInitialized?.Invoke(this);
                return;
            }

            Assert.IsTrue(_handedness != Handedness.None, $"The handedness (right or left) is not defined for this hand: {gameObject}");
          //  Assert.IsNotNull(_config);
            if (TryGetComponent(out IJointStatsController stats))
            {
                RuntimeStats = stats;
            }
            else
            {
                RuntimeStats = gameObject.AddComponent<JointStatsRecorder>();
            }            

            //TODO: add checks for rest of the properties!

            // check hierarchy           
            if (!TryGetArticulationBodiesInHierarchy(out var bodyList))
            {
                UnityEngine.Debug.LogWarning("Failed to initialize Articulation body component(s) in the hierarchy!");
                return;
            }
            _jointArticulationBodies = bodyList;
            ConstructHand();
            Initialized = true;
            HandsCore.HandsController.TryAdd(this);
            OnInitialized?.Invoke(this);
        }
        private void Start()
        {
            Init();
        }

        private void Awake()
        {
            if (_forceSettings == null)
            {
                _forceSettings = (ForceSettings)ScriptableObject.CreateInstance(typeof(ForceSettings));
                UnityEngine.Debug.LogWarning($"Force settings for {gameObject} is not specified. Default values will be used!");
            }
            if (_torqueSettings == null)
            {
                _torqueSettings = (TorqueSettings)ScriptableObject.CreateInstance(typeof(TorqueSettings));
                UnityEngine.Debug.LogWarning($"Torque settings for {gameObject} is not specified. Default values will be used!");
            }

           // Config = _config;
            ForceSettings = _forceSettings;
            TorqueSettings = _torqueSettings;
            OnTeleport -= ResetVelocities;
            OnTeleport += ResetVelocities;
            Tracking = HandsCore.GetHandTrackingDataProvider(_handTrackingProvider);
            _targetJointsPoseBuffer = new Pose[Tracking.GetNumberOfJoints()];
        }

        private void Destroy()
        {
            OnTeleport -= ResetVelocities;
        }

        private void ReinitializeTrackingProvider()
        {
            Tracking = HandsCore.GetHandTrackingDataProvider(_handTrackingProvider);
            UnityEngine.Debug.Log($"Hand tracking provider for {_handedness} hand changed to {_handTrackingProvider}");
        }

        private void ResetVelocities()
        {
            ArticulationBody.Reset();            
            _joints.ForEach(j => j.Reset());
        }

        private void ConstructHand()
        {
           // Transform parent = ArticulationBody.transform;
           // ITrackable trackableParent = BodyData;
            int firstBone = (int)BoneId.Hand_Thumb0;
            int lastBone = (int)BoneId.Hand_Pinky3;
            int jointIndex = 0;
            int prevFingerIndex = -1;
            string hand = _handedness.ToString();
            for (int i = firstBone; i < lastBone + 1; ++i)
            {
                int fingerIndex = SkeletonMapping.GetFingerIndexFromBoneId(i);
                string jointName = ((BoneId)i).ToString();
                jointIndex++;
                ArticulationBody body = _jointArticulationBodies[i - firstBone];
              //  parent = body.transform.parent;
                if (prevFingerIndex != fingerIndex)
                {
              //      parent = ArticulationBody.transform;
              //      trackableParent = BodyData;
                    prevFingerIndex = fingerIndex;
                    jointIndex = 0;
                }
                else
                {
               //     trackableParent = _joints[_joints.Count - 1].BodyData;
                }


                _joints.Add(new ArticulatedJoint(body, /*hand +*/ jointName, jointIndex, fingerIndex));
            }
            Joints = _joints.OfType<IJoint>().ToList().AsReadOnly();
            PalmColliders = ArticulationBody.GetComponents<Collider>().ToList();
        }

        //[NaughtyAttributes.ShowIf("_shouldCreateNewHierarchy"),
        // NaughtyAttributes.Button("Create Articulation Body Hierarchy",
        // NaughtyAttributes.EButtonEnableMode.Always)]
        //private ArticulationBody[] ConstructArticulationBodyHierarchy()
        //{
        //    //TODO:
        //    throw new NotImplementedException();
        //    // ...and then do:
        //    // ConstructHand();
        //}

        /// <summary>
        /// Returns a next joint in hierarchy. ie: input(j0) -> output(j1) etc
        /// </summary>
        public bool TryGetNextJointInChain(IJoint thisJoint, out IJoint nextJoint)
        {
            nextJoint = new ArticulatedJoint();
            int nextIndex = thisJoint.Index + 1;
            if (nextIndex < Joints.Count && Joints[nextIndex].FingerIndex == thisJoint.FingerIndex)
            {
                nextJoint = Joints[nextIndex];
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

        public bool TryGetTargetHandBoundingSphere(out Vector3 center, out float radius)
        {
            Pose pose = Tracking.GetLastReliableRootPose(Handedness);
            Bounds bounds = GetHandBounds();
            Vector3 localCenter = ArticulationBody.transform.InverseTransformPoint(bounds.center);
            center = pose.position + pose.rotation * localCenter;
            radius = bounds.extents.magnitude;
            return Initialized && radius > 0f;
        }

        /// <summary>
        ///  Returns the layers that collide with this hand
        /// </summary>
        /// <returns></returns>
        public int GetCollidingLayerMask()
        {
            int layerMask = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(_handLayer, i))
                {
                    layerMask = layerMask | 1 << i;
                }
            }
            return layerMask;
        }
        private void SetupBodyMass()
        {
            float artBodiesMass = 0f;
            var bodies = ArticulationBody.GetComponentsInChildren<ArticulationBody>()?.ToList();
            if (bodies.Count <= 1)
                return;
            //bodies.Remove(hand.palmArtBody); // remove root body since it is a base
            // if (handedness == Handedness.Left)
            //  {
            //  bodies.ForEach(x => x.mass = PerBoneMass);
            //palmMass = perBoneMass * 5f;
            //  bodies[0].mass = PalmMass;
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
            ArticulationBody.centerOfMass = Vector3.zero; //palmArtBody.transform.localPosition; //+ _palmForwardVector * palmpCenterOfMassOffset;
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

        private void SetDynamicFriction()
        {
            // TODO: implement dynamic friction using Experimental contacts modification API
            //https://forum.unity.com/threads/experimental-contacts-modification-api.924809/

            throw new NotImplementedException();
        }

        public bool CanTeleport(Pose targetPose)
        {
            return !Physics.CheckSphere(targetPose.position, TargetHandBoundingSphereRadius, GetCollidingLayerMask());
        }

        internal void ToogleColliders(bool active)
        {
            PalmColliders.ForEach(c => c.enabled = active);
            _joints.ForEach(c => c.Collider.enabled = active);
        }

        internal void IgnoreCollisionBetweenNeighboringJoints()
        {
            _joints.ForEach((j =>
            {
                if (TryGetNextJointInChain(j, out IJoint nextJoint))
                    Physics.IgnoreCollision(j.Collider, nextJoint.Collider);
            }));
        }

        /// <summary>
        /// Sets palm and fingers layer to "NoContac". 
        /// Use with caution!
        /// NOTE: has some side-effect (probably a bug?), where setting back to its native layer (ignore = true) couses articulation bodies ignore its collision matrix
        /// </summary>
        /// <param name="ignore"></param>
        internal void IgnoreCollision(bool ignore)
        {
            if (ignore)
            {
                int noContactrLayer = LayerMask.NameToLayer("NoContact");
                ArticulationBody.gameObject.layer = noContactrLayer;
                _joints.ForEach(b => b.ArticulationBody.gameObject.layer = noContactrLayer);
            }
            else
            {
                ArticulationBody.gameObject.layer = _handLayer;
                _joints.ForEach(b => b.ArticulationBody.gameObject.layer = b.ArticulationBody.gameObject.layer);
            }
        }

        public List<Collider> GetAlJointsColliders()
        {
            //TODO: consider cashing colliders if using this funtion  extensivelly
            return _joints.ConvertAll(c => c.Collider).ToList();
        }

        /// <summary>
        /// Returns the average weighted center of mass in world space for the hand (palm + fingers)
        /// </summary>
        /// <returns></returns>
        //private Vector3 GetHandCenterOfMass()
        //{
        //    Vector3 CoM = Vector3.zero;
        //    float c = 0f;
        //    foreach (var joint in Joints)
        //    {
        //        CoM += joint.ArticulationBody.worldCenterOfMass * Config.Joint.Mass;// PerBoneMass;
        //        c += Config.Joint.Mass;//PerBoneMass;
        //    }

        //    CoM += ArticulationBody.worldCenterOfMass;
        //    c += ArticulationBody.mass;
        //    return CoM / c;
        //}

        private bool TryGetArticulationBodiesInHierarchy(out List<ArticulationBody> bodies)
        {
            bodies = new List<ArticulationBody>();
            List<ArticulationBody> articulatedBonesUnsorted = GetComponentsInChildren<ArticulationBody>().ToList();
            ArticulationBody root = articulatedBonesUnsorted.FirstOrDefault(b => b.isRoot);
            if (root == null)
            {
                return false;
            }
            ArticulationBody = root;

            BoneId start = BoneId.Hand_Thumb0;
            BoneId end = BoneId.Hand_Pinky3;
            for (int bi = (int)start; bi < (int)end + 1; ++bi)
            {
                string fbxBoneName = SkeletonMapping.FbxBoneNameFromBoneIndex(_handedness, (BoneId)bi);
                ArticulationBody ab = articulatedBonesUnsorted.FirstOrDefault(b => b.gameObject.name.Contains(fbxBoneName));
                if (ab != null)
                {
                    bodies.Add(ab);
                }
            }
            return bodies.Count > 0;
        }

        public void UpdateData()
        {
            // update root (palm) data
            Pose handPose = new Pose(ArticulationBody.transform.position, ArticulationBody.transform.rotation);
            Pose targetPose = Tracking.GetLastReliableRootPose(_handedness);
            BodyData.Update(handPose);
            TargetData.Update(targetPose);

            // update joints (fingers) data          
            UpdateTargetJointsPoses();
        }
        private void UpdateTargetJointsPoses()
        {
            
            if (Initialized && Tracking.IsReliable(_handedness))
            {              
                Array.Copy(Tracking.GetBonesPoses(_handedness), _firstBone, _targetJointsPoseBuffer, 0, _targetJointsPoseBuffer.Length);
                for (int i = 0; i < Joints.Count; i++)
                {
                    Pose newBodyPose = new Pose(Joints[i].ArticulationBody.transform.position, Joints[i].ArticulationBody.transform.rotation);

                    Joints[i].TargetData.Update(_targetJointsPoseBuffer[i]);
                    Joints[i].BodyData.Update(newBodyPose);
                }
            }
        }
    }
}
