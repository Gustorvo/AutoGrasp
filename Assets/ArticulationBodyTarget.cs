using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ArticulationBody))]
public class ArticulationBodyTarget : MonoBehaviour
{
    ArticulationBody articulationBody;
    public Transform target;
    Quaternion startRotation;
    public Vector3 axis = Vector3.right;
    public Vector3 secondaryAxis = Vector3.forward;

    // Start is called before the first frame update
    void Start()
    {
        articulationBody = GetComponent<ArticulationBody>();
        startRotation = Quaternion.identity;// this.transform.localRotation;

        articulationBody.xDrive = InitDrive(articulationBody.xDrive);
        articulationBody.yDrive = InitDrive(articulationBody.yDrive);
        articulationBody.zDrive = InitDrive(articulationBody.zDrive);

    }

    // Update is called once per frame
    void Update()
    {
        var targetRotation = target.localRotation;

        // Calculate the rotation expressed by the joint's axis and secondary axis
        var right = Vector3.right;// axis;// joint.axis;
        var forward = Vector3.forward;// Vector3.Cross(axis, secondaryAxis).normalized;// joint.axis, joint.secondaryAxis).normalized;
        var up = Vector3.Cross(forward, right).normalized;
        Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

        // Transform into world space
        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);

        // Counter-rotate and apply the new local rotation.
        // Joint space is the inverse of world space, so we need to invert our value
        //if (space == Space.World)
        //{
        //    resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
        //}
        //else
        //{
            resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
       // }

        // Transform back into joint space
        resultRotation *= worldToJointSpace;

        // Set target rotation to our newly calculated rotation
        //joint.targetRotation = resultRotation;
        var rot = resultRotation.eulerAngles;
        rot = FixRot(rot);
        rotTarget = rot;
        articulationBody.xDrive = SetDrive(articulationBody.xDrive, rot.x);
        articulationBody.yDrive = SetDrive(articulationBody.yDrive, rot.y);
        articulationBody.zDrive = SetDrive(articulationBody.zDrive, rot.z);

        //  In Unity these rotations are performed around the Z axis, the X axis, and the Y axis, in that order. 
        //articulationBody.xDrive = SetDrive(articulationBody.xDrive, rot.z);
        //articulationBody.yDrive = SetDrive(articulationBody.yDrive, rot.y);
        //articulationBody.zDrive = SetDrive(articulationBody.zDrive, rot.x);
    }

    Vector3 FixRot(Vector3 r)
    {
        while (r.x > 180) { r.x -= 360; }
        while (r.y > 180) { r.y -= 360; }
        while (r.z > 180) { r.z -= 360; }
        return -r; // invert
    }

    public Vector3 rotTarget; 

    ArticulationDrive SetDrive(ArticulationDrive drive, float targetValue)
    {
        drive.target = targetValue;
        return drive;
    }

    ArticulationDrive InitDrive(ArticulationDrive drive )
    {
        drive.stiffness = 10000;
        drive.damping = 10;
        return drive;
    }
}
