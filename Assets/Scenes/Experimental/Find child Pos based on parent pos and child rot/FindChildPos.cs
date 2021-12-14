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
        prevParentPosition = parent.position;
        prevParentROtaion = parent.rotation;
        distToChild = Vector3.Distance(parent.position, objToMove.transform.position);
        childLocalPosition = child.transform.position - parent.position;
    }

    void  Update()
    {

        Method3();
    }

    Vector3 prevParentPosition;
    Quaternion prevParentROtaion;
    Vector3 childLocalPosition;
    private void Method3()
    {
        var parentRotDelta = prevParentROtaion * Quaternion.Inverse(parent.rotation);
        var parentPosDelta =  parent.position - prevParentPosition;
        prevParentPosition = parent.position;
       // prevParentROtaion = parent.rotation;
      
        objToMove.transform.rotation = parent.rotation * child.transform.localRotation; // this is absolutelly roght
        objToMove.transform.position = parent.position + parent.rotation * Vector3.Scale(childLocalPosition, parent.lossyScale);
        objToMove.transform.localScale = parent.lossyScale;
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
