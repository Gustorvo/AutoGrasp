using SoftHand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalizeMass : MonoBehaviour
{
    public ArtBodyTargetController controller;
    public float totalMass;
    public bool removeRbForDebugging;
    public bool modifyInetriaTensor;

    private void Awake()
    {
        if (removeRbForDebugging && TryGetComponent<FixedJoint>(out FixedJoint joint))
        {
            Destroy(joint);
            if (TryGetComponent<Rigidbody>(out Rigidbody body))
                Destroy(body);
        }
        if (modifyInetriaTensor && TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.inertiaTensor =  Vector3.one;
    }
    private void Apply(Transform root)
    {
        var j = root.GetComponent<Joint>();

        // Apply the inertia scaling if possible
        if (j && (j.connectedBody || j.connectedArticulationBody))
        {
            // Make sure that both of the connected bodies will be moved by the solver with equal speed
            float mass = root.TryGetComponent<Rigidbody>(out Rigidbody rb) ? rb.mass : root.GetComponent<ArticulationBody>().mass;
            j.massScale = j.connectedBody ? j.connectedBody.mass / mass + totalMass : j.connectedArticulationBody.mass / mass + totalMass;
            j.connectedMassScale = 1f;
        }

        // Continue for all children...
        for (int childId = 0; childId < root.childCount; ++childId)
        {
            Apply(root.GetChild(childId));
        }
    }

    public void Start()
    {
        if (controller)
            totalMass = controller.totalMass;
        Apply(this.transform);
    }
}
