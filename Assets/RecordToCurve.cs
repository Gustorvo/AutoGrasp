using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordToCurve : MonoBehaviour
{
    public AnimationCurve linearVelocityCurve, angularVelocityCurve;
    private ArticulationBody _body;
    private int _length;
    public float maxLinVel, maxAngVel;
    //public AnimationClip clip;

    private void Awake()
    {
        _body = GetComponent<ArticulationBody>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_body != null)
        {
            _length++;
            if (_length > 5)
            {
                linearVelocityCurve.AddKey(Time.time, _body.velocity.magnitude);
                angularVelocityCurve.AddKey(Time.time, _body.angularVelocity.magnitude);


                if (_body.angularVelocity.magnitude > maxAngVel)
                    maxAngVel = _body.angularVelocity.magnitude;


                if (_body.velocity.magnitude > maxLinVel)
                    maxLinVel = _body.velocity.magnitude;
            }
            if (_length > 20000)
                linearVelocityCurve = new AnimationCurve();
        }
    }

    private void OnDisable()
    {
        //clip.SetCurve
    }
}
