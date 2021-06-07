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
    public class ArtBodyTargetController : MonoBehaviour
    {        
       
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
      
        private int _layerMask;
        public float totalMass = 0f;
        [Header("Angular force values:")]
        public float alignmentSpeed = 7f;
        public float alignmentDamping = 5f;
        public bool applyCounterTorque = false;
        public bool applyTorque = false;

        [Header("Linear force values:")]
        public float minDistance = 0.002f;
        public float toVel = 25f;
        public float maxVel = 1500f;
        public float maxForce = 1500f;
        public float gain = 60f;
        public bool applyCounterLinearForces = false;
        public bool applyLinearForces = false;

        public Vector3 linearForce, angularForce;
        public float deltaAngle;      
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
        }


        private void FetchColliders()
        {
            _colliders.Clear();
            bodies.ForEach(x => _colliders.Add(x.GetComponent<Collider>()));
            _colliders.Add(rootBody.GetComponent<BoxCollider>());
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
      
        #endregion

        public void FixedUpdate()
        {
            FetchArtDriveTargets();
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

