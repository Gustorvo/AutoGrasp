using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindChildPos : MonoBehaviour
{
    public float distToChild;
    public GameObject objToMove;
    public GameObject child;
    public Vector3 multiplier;

    private Transform parent;

    void Awake()
    {
        parent = transform;
        distToChild = Vector3.Distance(parent.position, child.transform.position);
    }

    void  Update()
    {

        Method1();
    }

    private void Method3()
    {
        objToMove.transform.position = parent.rotation * objToMove.transform.position;
        objToMove.transform.rotation = parent.rotation * child.transform.localRotation; // this is absolutelly roght
    }

    private void Method2()
    {
        Quaternion newWRot = LocalToWorld(child.transform.localRotation, parent.rotation);
        Vector3 newWPos = LocalToWorld(objToMove.transform.localPosition, new Pose(parent.position, parent.rotation), Vector3.one);
        objToMove.transform.position = newWPos;
        objToMove.transform.rotation = newWRot;
    }

    private void Method1()
    {
     // working method
        Vector3 childPos = parent.position + parent.up * distToChild;
        objToMove.transform.position = childPos;
        objToMove.transform.rotation = parent.rotation;// * child.transform.rotation;
    }

    private void Method0()
    {
        distToChild = Vector3.Distance(parent.position, child.transform.position);
        Vector3 pos = parent.position + transform.up * distToChild;
        objToMove.transform.position = pos;
        objToMove.transform.rotation = parent.rotation * child.transform.localRotation;
    }

    Quaternion LocalToWorld(Quaternion childRot, Quaternion parentRot) => parentRot * childRot;
    Vector3 LocalToWorld(Vector3 childPosition, Pose parentPose, Vector3 parentScale) => parentPose.position + parentPose.rotation * (Vector3.Scale(childPosition, parentScale));
}
