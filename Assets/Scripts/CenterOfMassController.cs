using SoftHand;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CenterOfMassController : MonoBehaviour
{
   public ArticulationBody _ab;
    public Vector3 cm;
    public GameObject marker;

    public List<ArticulationBody> _bodies;

    private void Awake()
    {
        if (_ab == null)
        _ab = GetComponent<ArticulationBody>();

        if (_ab != null)
        //    _ab.SetCoMInHierarchy();
        _bodies = GetComponentsInChildren<ArticulationBody>().ToList();
        
       // SetBodiesCoM();
       // SetRootCoM();
        // _ab.SetCoMInHierarchy();

    }


    private void SetBodiesCoM()
    {
        foreach (ArticulationBody body in _bodies)
        {
            body.centerOfMass = LocalCoM(body, transform.position);
           // body.inertiaTensor = Vector3.one;

        }

        Vector3 LocalCoM(ArticulationBody ab, Vector3 worldCoM)
        {
            return ab.transform.InverseTransformDirection(worldCoM - ab.transform.position);
        }
    }
    private void SetRootCoM()
    {
        cm = _ab.centerOfMass;
        SumMasses();
        _ab.centerOfMass = cm;
       // _ab.inertiaTensor = Vector3.one;

    }
 

    private void Update()
    {
        if (_ab && marker)
        {
            marker.transform.localPosition = _ab.centerOfMass;
            cm = _ab.centerOfMass;
        }

       
    }

    private void SumMasses()
    {
        Vector3 CoM = Vector3.zero;
        float c = 0f;

        foreach (ArticulationBody body in _bodies)
        {
            CoM += body.worldCenterOfMass * body.mass;
            c += body.mass;
        }

        CoM /= c;
        cm = CoM;
    }

    private void OnDrawGizmos()
    {
        // draw gizmos
        foreach (ArticulationBody body in _bodies)
        {
            Draw(body.transform.position + body.transform.rotation * body.centerOfMass, 0.005f);
        }
    }

    private void Draw(Vector3 center, float radi)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, radi);
    }
}
