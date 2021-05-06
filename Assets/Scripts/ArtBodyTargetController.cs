using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ArtDrive;
using static SoftHand.Enums;

namespace SoftHand
{
    public class ArtBodyTargetController : MonoBehaviour, OvrToArtDriveDataProvider.IOvrDataConsumer
    {
        [SerializeField]
        OvrToArtDriveDataProvider.IOvrDataProvider _dataProvider;
        public List<ArticulationBody> bodies = new List<ArticulationBody>();
        public List<Collider> _colliders = new List<Collider>();
        public List<ArtDrive> drives = new List<ArtDrive>();
        public List<ArtDriveTarget> driveTargets = new List<ArtDriveTarget>();
        public List<int> newDriveIndexes = new List<int>();
        public List<int> dofStartIndices = new List<int>();
        public List<float> fetchedDriveTargetValues = new List<float>();
        public List<float> driveTargetValues = new List<float>();
        public List<float> driveTargetValuesToSet = new List<float>();
        [SerializeField]
        SkeletonMapping _skeletionMapping;

        public ArticulationBody rootBody;
        public Rigidbody _bodyToMove;
        // public Rigidbody rootRb;
        private int _lastFrameTeleport = 0;
        private Vector3[] _boneRotationsBuffer;
        private Vector3[] _boneRotations;
        private int _layerMask;
        public float totalMass = 0f;
        [Header("Angular force values:")]
        public float alignmentSpeed = 11.11f;
        public float alignmentDamping = 11.3f;
        public bool applyCounterTorque = false;
        public bool applyTorque = false;

        [Header("Linear force values:")]
        public float minDistance = float.Epsilon;
        public float toVel = 25f;
        public float maxVel = 1500f;
        public float maxForce = 1500f;
        public float gain = 60f;
        public bool applyCounterLinearForces = false;
        public bool applyLinearForces = false;

        public Vector3 linearForce, angularForce;
        public float deltaAngle;
        private bool _ghosted;
        public float _thisBodyMaxSqrMag;
        public float _thisBodyMaxTgtVel;




        #region Init

        public void Awake()
        {
            //bodies = new List<ArticulationBody>();
            if (rootBody == null)
                rootBody = GetComponent<ArticulationBody>();
            if (rootBody)
            {
                rootBody.SetupRootBody(false);
                rootBody.solverIterations = 255;
                rootBody.solverVelocityIterations = 255;

                FetchArtDriveTargets();
                FetchBodies();
                FetchColliders();
                FetchDrives();
                FetchIndexes();
            }
            if (_bodyToMove != null && rootBody != null)
            {
                _bodyToMove.useGravity = rootBody.useGravity;
                _bodyToMove.maxAngularVelocity *= 2f;
            }
            if (_dataProvider == null)
            {
                _dataProvider = GetComponent<OvrToArtDriveDataProvider.IOvrDataProvider>();
            }
            //create lookup table
            InitializeBuffer();
        }


        private void FetchColliders()
        {
            _colliders.Clear();
            bodies.ForEach(x => _colliders.Add(x.GetComponent<Collider>()));
            _colliders.Add(rootBody.GetComponent<BoxCollider>());
        }

        private void InitializeBuffer()
        {
            var data = _dataProvider.GetRotationData();
            if (!data.IsEmpty)
            {
                _boneRotationsBuffer = new Vector3[data.Length];
                _boneRotations = new Vector3[data.Length];
            }
        }

        [ContextMenu("manual fetch custom drives")]
        private void FetchNewDrives()
        {
            FetchArtDriveTargets();
            if (rootBody)
            {
                FetchBodies();
                FetchDrives();
                FetchIndexes();
            }
        }

        [ContextMenu("setup art drives")]
        private void MoveDriveTargets()
        {
            if (TryGetComponent<ArticulationBody>(out ArticulationBody body))
            {
                List<float> targets = driveTargetValues;
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i] = 0;
                }

                body.SetDriveTargets(targets);
            }
        }

        public void FetchBodies()
        {
            bodies = new List<ArticulationBody>();
            bodies = rootBody.GetComponentsInChildren<ArticulationBody>().ToList();
            bodies.Remove(rootBody); // remove root body since it is a base

            // sort by index, as in the reduced coordinate data buffer
            bodies.Sort((x, y) => x.index.CompareTo(y.index));

            // accumulate all art bodies' masses in hierarchy
            bodies.ForEach(x => totalMass += x.mass);

            rootBody.SetCoMInHierarchy();

            // Get the layers that collide with this hand
            int myLayer = rootBody.gameObject.layer;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(myLayer, i))
                {
                    _layerMask = _layerMask | 1 << i;
                }
            }
        }


        public void FetchDrives()
        {
            driveTargets = new List<ArtDriveTarget>();
            driveTargetValues = new List<float>();
            rootBody.GetJointPositions(driveTargetValues);
            driveTargetValues.Clear();
            drives = new List<ArtDrive>();
            for (int i = 0; i < bodies.Count; i++)
            {
                var drive = new ArtDrive(bodies[i]);
                drives.Add(drive);
                driveTargets.AddRange(drive.Targets);
            }
            driveTargetValues.AddRange(driveTargets.Select(x => x.ArtDrive.target));
        }

        public void FetchIndexes()
        {
            newDriveIndexes = new List<int>();
            for (int i = 0; i < bodies.Count; i++)
            {
                newDriveIndexes.Add(bodies[i].index);
            }
            if (rootBody)
            {
                rootBody.GetDofStartIndices(dofStartIndices);
                dofStartIndices.RemoveAt(0); // remove root body
            }
        }
        #endregion

        #region Getters
        public float GetDriveValue(Drive type, int instanceId)
        {
            var drive = drives.FirstOrDefault(d => d.InstanceId.Equals(instanceId));
            if (drive.Body.IsSleeping())
                Debug.Log("body is sleeping: " + drive.Id);
            if (drive == null)
                return 0;
            if (type == Drive.Xdrive)
                return drive.Body.xDrive.target;
            else if (type == Drive.Ydrive)
                return drive.Body.yDrive.target;
            else
                return drive.Body.zDrive.target;
        }

        private void FetchArtDriveTargets()
        {
            if (rootBody)
            {
                //get all target drives
                rootBody.GetDriveTargets(fetchedDriveTargetValues);
                ToDegrees(fetchedDriveTargetValues);
            }
        }

        private void ToDegrees(List<float> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] *= Mathf.Rad2Deg;
            }
        }
        private void ToRadians(List<float> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] *= Mathf.Deg2Rad;
            }
        }

        #endregion

        #region Setters
        public void SetDriveValue(Drive type, int instanceId, float value)
        {
            var drive = drives.FirstOrDefault(d => d.InstanceId.Equals(instanceId));
            if (drive != null)
            {
                // we should regulate drive damping based on % done on target. 99% done = 100% damping, 0% done = 0% damping
                var bodyDrive = new ArticulationDrive();
                if (type == Drive.Xdrive)
                {
                    bodyDrive = drive.Body.xDrive;
                    bodyDrive.target = value;
                    drive.Body.xDrive = bodyDrive;
                }
                if (type == Drive.Ydrive)
                {
                    bodyDrive = drive.Body.yDrive;
                    bodyDrive.target = value;
                    drive.Body.yDrive = bodyDrive;
                }
                if (type == Drive.Zdrive)
                {
                    bodyDrive = drive.Body.zDrive;
                    bodyDrive.target = value;
                    drive.Body.zDrive = bodyDrive;
                }
            }

        }

        private void CalculateJointForces(ArticulationDrive drive, ArticulationBody ab, float targetPos)
        {
            // Calculate the delta between the current rotation and the desired rotation
            Quaternion deltaRotation = Quaternion.Normalize(Quaternion.Inverse(ab.transform.localRotation) * transform.rotation);

            // Calculate drive velocity necessary to undo this delta in one fixed timestep
            ArticulationReducedSpace driveTargetForce = new ArticulationReducedSpace(
              ((Mathf.DeltaAngle(0, deltaRotation.eulerAngles.x) * Mathf.Deg2Rad) / Time.fixedDeltaTime),
              ((Mathf.DeltaAngle(0, deltaRotation.eulerAngles.y) * Mathf.Deg2Rad) / Time.fixedDeltaTime),
              ((Mathf.DeltaAngle(0, deltaRotation.eulerAngles.z) * Mathf.Deg2Rad) / Time.fixedDeltaTime));

            // Apply the force in local space (unlike AddTorque which is global space)
            // Ideally we'd use inverse dynamics or jointVelocity, but jointVelocity is bugged in 2020.1a22
            ab.jointForce = driveTargetForce;


            //Effect = stiffness * (drivePosition - targetPosition) - damping * (driveVelocity - targetVelocity);
            //float stiffness = drive.stiffness;
            //float drivePosition = drive.target;
            //float targetPosition = targetPos;
            //float damping = drive.damping;
            //var driveVelocity = ab.angularVelocity;
            //float targetVelocity = drive.targetVelocity;
            //var effect = stiffness * (drivePosition - targetPosition) - damping * (driveVelocity - targetVelocity);

        }
        #endregion

        public void FixedUpdate()
        {
            FetchArtDriveTargets();
        }

        private void MoveRootBody(OvrHandData data)
        {
            if (_bodyToMove != null)
            {
                if (_bodyToMove.IsSleeping())
                    Debug.Log("main body is sleeping");
                if (rootBody.IsSleeping())
                    Debug.Log("root body is sleeping");
                Vector3 dist = data.HandWorldPosition - _bodyToMove.transform.position;


                // calc a target vel proportional to distance (clamped to maxVel)
                Vector3 tgtVel = Vector3.ClampMagnitude(toVel * dist, maxVel);
                // calculate the velocity error
                Vector3 error = tgtVel - _bodyToMove.velocity;
                // calc a force proportional to the error (clamped to maxForce)
                linearForce = Vector3.ClampMagnitude(gain * error, maxForce);
                // accounter for body masses
                linearForce *= _bodyToMove.mass + totalMass;

                // handle distance overshooting
                if (dist.sqrMagnitude < minDistance && applyCounterLinearForces)
                {
                    // calculate counter Force, where F = Mass * (0 - Vel) / dTime
                    Vector3 counterForce = (_bodyToMove.mass + totalMass) * (Vector3.zero - _bodyToMove.velocity) / Time.fixedDeltaTime;
                    _bodyToMove.AddForce(counterForce);                    
                    // return;
                }
                // calc delta rotation
                Quaternion deltaRotation = Quaternion.Inverse(_bodyToMove.transform.rotation) * data.HandWorldRotation;
                Vector3 deltaAngles = GetRelativeAngles(deltaRotation.eulerAngles);
                Vector3 worldDeltaAngles = _bodyToMove.transform.TransformDirection(deltaAngles);
                angularForce = (_bodyToMove.mass + totalMass) * alignmentSpeed * worldDeltaAngles - alignmentDamping * _bodyToMove.angularVelocity * (_bodyToMove.mass + totalMass);

                // check for Nan
                // TODO: should be another way to handle this
                angularForce = float.IsNaN(angularForce.sqrMagnitude) ? Vector3.zero : angularForce;                
                linearForce = float.IsNaN(linearForce.sqrMagnitude) ? Vector3.zero : linearForce;

                // handle overshoot for torque forces
                deltaAngle = Quaternion.Angle(_bodyToMove.transform.rotation, data.HandWorldRotation);
                // apply counter torque forces, where  T = I * (0 - A) / dTime
                if (deltaAngle <= 10f && applyCounterTorque)
                {
                    Vector3 torque = _bodyToMove.inertiaTensorRotation * (Vector3.zero - _bodyToMove.angularVelocity) / Time.fixedDeltaTime;
                    _bodyToMove.AddTorque(torque);
                }

                if (!_bodyToMove.IsSleeping())
                {
                    // Apply linear and angular forces
                    if (applyLinearForces)
                        _bodyToMove.AddForce(linearForce);
                    if (applyTorque)
                        _bodyToMove.AddTorque(angularForce);

                }
               
                Vector3 GetRelativeAngles(Vector3 angles)
                {
                    Vector3 relativeAngles = angles;
                    if (relativeAngles.x > 180f)
                        relativeAngles.x -= 360f;
                    if (relativeAngles.y > 180f)
                        relativeAngles.y -= 360f;
                    if (relativeAngles.z > 180f)
                        relativeAngles.z -= 360f;

                    return relativeAngles;
                }

                ////Fix the hand if it gets into a bad situation by teleporting and holding in place until its bad velocities disappear
                //if (Vector3.Distance(rootBody.transform.position, data.HandWorldPosition) > 1.0f)
                //{
                //    if (_bodyToMove != null)
                //    {
                //        //_bodyToMove.isKinematic = true;
                //        //_bodyToMove.transform.position = data.HandWorldPosition;
                //        //_bodyToMove.transform.rotation = data.HandWorldRotation;
                //        //_bodyToMove.velocity = Vector3.zero;
                //        //_bodyToMove.angularVelocity = Vector3.zero;
                //    }

                //    rootBody.immovable = true;
                //    rootBody.TeleportRoot(data.HandWorldPosition, data.HandWorldRotation);
                //    rootBody.velocity = Vector3.zero;
                //    rootBody.angularVelocity = Vector3.zero;
                //    _lastFrameTeleport = Time.frameCount;

                //    foreach (var collider in _colliders) collider.enabled = false;
                //    for (int i = 0; i < bodies.Count; i++)
                //    {
                //        //_articulationBodies[i].jointVelocity   = new ArticulationReducedSpace(0f, 0f, 0f);
                //        bodies[i].velocity = Vector3.zero;
                //        bodies[i].angularVelocity = Vector3.zero;
                //    }
                //    _ghosted = true;
                //}
                //if (Time.frameCount - _lastFrameTeleport >= 1)
                //{
                //    rootBody.immovable = false;
                //    rootBody.WakeUp();
                //    //  _bodyToMove.isKinematic = false;
                //    _bodyToMove.WakeUp();

                //    //loPolyHandRenderer.enabled = true;
                //}
                //if (Time.frameCount - _lastFrameTeleport >= 2 && _ghosted &&
                //    !Physics.CheckSphere(rootBody.worldCenterOfMass, 0.1f, _layerMask))
                //{
                //    foreach (var collider in _colliders)
                //    {
                //        collider.enabled = true;
                //        collider.isTrigger = true;
                //    }
                //    _ghosted = false;
                //}


            }
        }

        private void MoveAndRotateFingerJoints()
        {   
            if (driveTargets.Count > 0)
            {
                for (int i = 0; i < driveTargets.Count; i++)
                {
                    var drive = driveTargets[i];
                    float newTargetValue = driveTargetValues[i];

                    float driveValue = GetDriveValue(drive.DriveType, drive.InstanceId);
                    //if (Mathf.Abs(sliderValue - driveValue) > float.Epsilon) // don't move if value hasn't changed (for now disabled)
                    SetDriveValue(drive.DriveType, drive.InstanceId, newTargetValue);
                }
            }

            // use batch mode
            // buggy for now, therefore disabled
            //if (rootBody != null && fetchedDriveTargetValues.Count > 0)
            //{
            //    ToRadians(driveTargetValues);
            //    rootBody.SetDriveTargets(driveTargetValues);
            //}
        }

        public void ConsumeData()
        {
            var data = _dataProvider.GetRotationData();
            TransferOvrData(data);
            //  if (!rootBody.immovable)
            MoveRootBody(data);
            MoveAndRotateFingerJoints();
        }

        void TransferOvrData(OvrHandData data)
        {
            if (_skeletionMapping)
            {
                // copy bone rotation data to buffer
                Array.Copy(data.DeltaRotationEuler, _boneRotationsBuffer, data.Length);

                for (int i = 0; i < _boneRotationsBuffer.Length; i++)
                {
                    if (_skeletionMapping.flippedYVectorList[i] == true)
                    {
                        _boneRotationsBuffer[i] = _boneRotationsBuffer[i].FlippX2Z();
                        // apply inversion for Y axis
                        _boneRotationsBuffer[i] = _boneRotationsBuffer[i].ToFlippedYVector3();
                    }
                    // apply invertion if needed for some vectors
                    if (_skeletionMapping.invertedVectorList[i] == true)
                    {
                        _boneRotationsBuffer[i] = _boneRotationsBuffer[i].ToFlippedXZVector3();

                    }
                }
                for (int i = 0; i < _boneRotationsBuffer.Length; i++)
                {
                    // transfer buffer data to boneRotations array
                    int index = _skeletionMapping.LookupDic.Lookup(i);
                    _boneRotations[i] = _boneRotationsBuffer[index];
                }

                for (int i = 0; i < driveTargets.Count; i++)
                {
                    Drive type = driveTargets[i].DriveType;
                    float v = 0;
                    if (type == Drive.Xdrive)
                    {
                        v = _boneRotations[driveTargets[i].Index - 1].x;
                    }
                    if (type == Drive.Ydrive)
                    {
                        v = _boneRotations[driveTargets[i].Index - 1].y;
                    }
                    if (type == Drive.Zdrive)
                    {
                        v = _boneRotations[driveTargets[i].Index - 1].z;
                    }

                    driveTargetValues[i] = v;
                }

            }
        }
    }
}

public class ArtDrive
{
    public string Id = string.Empty;
    public bool HasX =>/* Body.jointType != ArticulationJointType.SphericalJoint && */Body.twistLock != ArticulationDofLock.LockedMotion;// || Body.jointType != ArticulationJointType.FixedJoint;
    public bool HasY => Body.jointType == ArticulationJointType.SphericalJoint;
    public bool HasZ => Body.jointType == ArticulationJointType.SphericalJoint;

    public float Xmin = 0;
    public float Xmax = 0;
    public float Xvalue => Xdrive.target;

    public float Ymin = 0;
    public float Ymax = 0;
    public float Yvalue => Ydrive.target;

    public float Zmin = 0;
    public float Zmax = 0;
    public float Zvalue => Zdrive.target;

    public int InstanceId => Body != null ? Body.GetInstanceID() : 0;

    public int FingerBoneIndex; // the index this articulation drive has in the local finger bone list.
    public ArticulationBody Body;
    public List<ArtDriveTarget> Targets;

    ArticulationDrive Xdrive;
    ArticulationDrive Ydrive;
    ArticulationDrive Zdrive;


    public ArtDrive(ArticulationBody body)
    {

        this.Body = body;
        Id = body.gameObject.name;

        Xdrive = body.xDrive;
        Xmin = Xdrive.lowerLimit;
        Xmax = Xdrive.upperLimit;


        Ydrive = body.yDrive;
        Ymin = Ydrive.lowerLimit;
        Ymax = Ydrive.upperLimit;


        Zdrive = body.zDrive;
        Zmin = Zdrive.lowerLimit;
        Zmax = Zdrive.upperLimit;


        if (IsLockFree(body.twistLock))
        {
            Xmin = -180;
            Xmax = 180;
        }
        if (IsLockFree(body.swingZLock))
        {
            Zmin = -180;
            Zmax = 180;
        }

        if (IsLockFree(body.swingYLock))
        {
            Ymin = -180;
            Ymax = 180;
        }
        Targets = SetTargetDrives();
    }

    private bool IsLockFree(ArticulationDofLock artLock) => (int)artLock == 2; //0=locked, 1=limited, 2=free


    private List<ArtDriveTarget> SetTargetDrives()
    {
        List<ArtDriveTarget> drives = new List<ArtDriveTarget>();
        if (HasX)
            drives.Add(new ArtDriveTarget(Xdrive, Drive.Xdrive, InstanceId, Body.index, Body.name + " X", Xmin, Xmax));
        if (HasY)
            drives.Add(new ArtDriveTarget(Ydrive, Drive.Ydrive, InstanceId, Body.index, Body.name + " Y", Ymin, Ymax));
        if (HasZ)
            drives.Add(new ArtDriveTarget(Zdrive, Drive.Zdrive, InstanceId, Body.index, Body.name + " Z", Zmin, Zmax));
        return drives;

    }

    public struct ArtDriveTarget
    {
        public ArticulationDrive ArtDrive;
        public Drive DriveType;// = Drive.Invalid;
        public int InstanceId;// = 0;
        public int Index;// = 0;
        public string Name;
        public float LowerLimit;
        public float UpperLimit;


        public ArtDriveTarget(ArticulationDrive drive, Drive type, int id, int index, string name, float lowLim, float upLim)
        {
            ArtDrive = drive;
            DriveType = type;
            InstanceId = id;
            Index = index;
            Name = name;
            LowerLimit = lowLim;
            UpperLimit = upLim;
        }
    }
}

