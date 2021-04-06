using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmMover : MonoBehaviour
{
    private ArticulationBody _palm;
    private float _mass = 0;
    private BoxCollider _collider;


    public Transform _root;

    private void Awake()
    {
        _palm = GetComponent<ArticulationBody>();
        _mass = _palm.mass;
        _collider = GetComponent<BoxCollider>();
    }   

    public void MovePalm(Vector3 direction, Quaternion torque)
    {

        _palm.AddForce(direction);

        Quaternion rotation = torque * Quaternion.Inverse(transform.rotation);
        Vector3 angularVelocity = Vector3.ClampMagnitude((new Vector3(
          Mathf.DeltaAngle(0, rotation.eulerAngles.x),
          Mathf.DeltaAngle(0, rotation.eulerAngles.y),
          Mathf.DeltaAngle(0, rotation.eulerAngles.z)) / Time.fixedDeltaTime) * Mathf.Deg2Rad, 45f /** _strength*/);
        
        _palm.angularVelocity = angularVelocity;
        _palm.angularDamping = 50f;
        

        //// Apply tracking position velocity; force = (velocity * mass) / deltaTime
      
        //Vector3 palmDelta = (hand_.PalmPosition.ToVector3() +
        //  (hand_.Rotation.ToQuaternion() * Vector3.back * 0.0225f) +
        //  (hand_.Rotation.ToQuaternion() * Vector3.up * 0.0115f)) - _palmBody.worldCenterOfMass;
        //// Setting velocity sets it on all the joints, adding a force only adds to root joint
        ////_palmBody.velocity = Vector3.zero;
        //float alpha = 0.05f; // Blend between existing velocity and all new velocity
        //_palmBody.velocity *= alpha;
        //_palmBody.AddForce(Vector3.ClampMagnitude((((palmDelta / Time.fixedDeltaTime) / Time.fixedDeltaTime) * (_palmBody.mass + (_perBoneMass * 5))) * (1f - alpha), 1000f * _strength));

      
    }    
}
