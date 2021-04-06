using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ArtDrive;
using static SoftHand.Enums;

public class ArtBodyTargetController : MonoBehaviour
{
    public List<ArticulationBody> bodies = new List<ArticulationBody>();
    public List<ArtDrive> drives = new List<ArtDrive>();
    public List<ArtDriveTarget> driveTargets = new List<ArtDriveTarget>();
    public List<int> newDriveIndexes = new List<int>();
    public List<float> driveTargetValues = new List<float>();
    public List<float> fetchedriveTargetValues = new List<float>();

    public ArticulationBody rootBody;


    #region Init

    public void Awake()
    {
        //bodies = new List<ArticulationBody>();
        if (rootBody == null)
            rootBody = GetComponent<ArticulationBody>();
        FetchBodies();
        FetchDrives();
    }

    [ContextMenu("fetch new drives")]
    private void FetchNewDrives()
    {

        // if (TryGetComponent<ArticulationBody>(out ArticulationBody body))
        //  {

        // get list of indexes
        rootBody.GetDofStartIndices(newDriveIndexes);
        //remove root


        List<float> list = new List<float>();
        //get all target drives
        rootBody.GetDriveTargets(fetchedriveTargetValues);
        ToDegrees(fetchedriveTargetValues);

        //get index for a particular drive
        var i = rootBody.index;
        FetchBodies();
        FetchDrives();
        FetchIndexes();

        //TODO: make drives list to display in inspector
        // and compare its count to newDriveTargets count
        //}
    }

    private void ToDegrees(List<float> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i] *= Mathf.Rad2Deg;
        }
    }

    [ContextMenu("set new drives")]
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
        AddBodiesRecursivelyroot(rootBody.transform);
        // local function
        void AddBodiesRecursivelyroot(Transform root)
        {
            foreach (Transform child in root)
            {
                if (child.TryGetComponent<ArticulationBody>(out ArticulationBody body))
                {
                    bodies.Add(body);
                    if (child != transform)
                        AddBodiesRecursivelyroot(body.transform);
                }
            }
        }
    }


    public void FetchDrives()
    {
        driveTargets = new List<ArtDriveTarget>();
        driveTargetValues = new List<float>();
        drives = new List<ArtDrive>();
        for (int i = 0; i < bodies.Count; i++)
        {
            var drive = new ArtDrive(bodies[i]);
            drives.Add(drive);
            driveTargets.AddRange(drive.targets);
        }
        driveTargetValues.AddRange(driveTargets.Select(x => x.ArtDrive.target));
    }

    public void FetchIndexes()
    {
        //newDriveIndexes = new List<int>();
        //for (int i = 0; i < driveTargets.Count; i++)
        //{
        //    newDriveIndexes.Add(driveTargets[i].Index);
        //}
        //newDriveIndexes.Sort();

    }
    #endregion

    public float GetDriveValue(Drive type, int instanceId)
    {
        var drive = drives.FirstOrDefault(d => d.InstanceId.Equals(instanceId));
        if (drive == null)
            return 0;
        if (type == Drive.Xdrive)
            return drive.body.xDrive.target;
        else if (type == Drive.Ydrive)
            return drive.body.yDrive.target;
        else
            return drive.body.zDrive.target;
    }

    public void SetDriveValue(Drive type, int instanceId, float value)
    {
        var drive = drives.FirstOrDefault(d => d.InstanceId.Equals(instanceId));
        if (drive != null)
        {
            var bodyDrive = new ArticulationDrive();
            if (type == Drive.Xdrive)
            {
                bodyDrive = drive.body.xDrive;
                bodyDrive.target = value;
                drive.body.xDrive = bodyDrive;
            }
            if (type == Drive.Ydrive)
            {
                bodyDrive = drive.body.yDrive;
                bodyDrive.target = value;
                drive.body.yDrive = bodyDrive;
            }
            if (type == Drive.Zdrive)
            {
                bodyDrive = drive.body.zDrive;
                bodyDrive.target = value;
                drive.body.zDrive = bodyDrive;
            }
        }

    }
}

public class ArtDrive
{
    public string id = string.Empty;
    public bool HasX => (int)body.twistLock > 0;
    public bool HasY => (int)body.swingYLock > 0;
    public bool HasZ => (int)body.swingZLock > 0;

    public float Xmin = 0;
    public float Xmax = 0;
    public float Xvalue => Xdrive.target;

    public float Ymin = 0;
    public float Ymax = 0;
    public float Yvalue => Ydrive.target;

    public float Zmin = 0;
    public float Zmax = 0;
    public float Zvalue => Zdrive.target;

    public int InstanceId => body != null ? body.GetInstanceID() : 0;

    public ArticulationBody body;
    public List<ArtDriveTarget> targets;

    ArticulationDrive Xdrive;
    ArticulationDrive Ydrive;
    ArticulationDrive Zdrive;


    public ArtDrive(ArticulationBody body)
    {

        this.body = body;
        id = body.gameObject.name;

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
        targets = SetTargetDrives();
    }

    private bool IsLockFree(ArticulationDofLock artLock) => (int)artLock == 2; //0=locked, 1=limited, 2=free


    private List<ArtDriveTarget> SetTargetDrives()
    {
        List<ArtDriveTarget> drives = new List<ArtDriveTarget>();
        if (HasX)
            drives.Add(new ArtDriveTarget(Xdrive, Drive.Xdrive, InstanceId, body.index, body.name + " X", Xmin, Xmax));
        if (HasY)
            drives.Add(new ArtDriveTarget(Ydrive, Drive.Ydrive, InstanceId, body.index, body.name + " Y", Ymin, Ymax));
        if (HasZ)
            drives.Add(new ArtDriveTarget(Zdrive, Drive.Zdrive, InstanceId, body.index, body.name + " Z", Zmin, Zmax));
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

