using SoftHand.Extensions;
using UnityEngine;

public class TwoBoneIK : MonoBehaviour
{
    public Transform UpperDummy;//root of upper arm
    public Transform MiddleDummy;//root of lower arm
    public Transform EndDummy;//root of hand
    public Transform Target;//target position of hand
    public Transform Pole;//direction to bend towards 
    public float UpperElbowRotation = 108f;//Rotation offsetts
    public float MiddleElbowRotation = 106.5f;
    [SerializeField, Range(-120f, 160)]
    float _wristTwistAngle = 0f;
    [SerializeField, Range(-30f, 30)]
    float _wristSwingYAngle = 0f;
    [SerializeField, Range(-90f, 90)]
    float _wristSwingZAngle = 0f;

   

    public float distanceToMiddle;

    [SerializeField] bool _useArticulations;
    [SerializeField] ArticulationBody _upperJoint, _middleJoint, _endJoint;

    private float a;//values for use in cos rule
    private float b;
    private float c;
    private Vector3 en;//Normal of plane we want our arm to be on
    private Quaternion _startUpperRot, _startLowerRot;


    private Vector3 _upperReduced, _lowerReduced, _endReduced;

    private void Awake()
    {
        
        _startUpperRot = UpperDummy.rotation;
        _startLowerRot = MiddleDummy.rotation;
        Vector3 axis;
        float angle;
        UpperDummy.localRotation.ToAngleAxis(out angle, out axis);
        Debug.Log($"angle: {angle}, axis: {axis}, multiplied: {angle * axis}");
        //UpperElbowRotation = -Upper.localEulerAngles.z;
    }

    void Update()
    {
        //a = Lower.localPosition.magnitude;
        //b = End.localPosition.magnitude;
        //c = Vector3.Distance(Upper.position, Target.position);
        //en = Vector3.Cross(Target.position - Upper.position, Pole.position - Upper.position);
        //// Debug.Log("The angle is: " + CosAngle(a, b, c));
        //Debug.DrawLine(Upper.position, Target.position);
        //Debug.DrawLine((Upper.position + Target.position) / 2, Lower.position);

        ////Set the rotation of the upper arm
        //Upper.rotation = Quaternion.LookRotation(Target.position - Upper.position, Quaternion.AngleAxis(UpperElbowRotation, Lower.position - Upper.position) * (en));
        //Upper.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, Lower.localPosition));
        //Upper.rotation = Quaternion.AngleAxis(-CosAngle(a, c, b), -en) * Upper.rotation;

        ////set the rotation of the lower arm
        //Lower.rotation = Quaternion.LookRotation(Target.position - Lower.position, Quaternion.AngleAxis(LowerElbowRotation, End.position - Lower.position) * (en));
        //Lower.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, End.localPosition));


        Solve();
    }

    //function that finds angles using the cosine rule 
    float CosAngle(float a, float b, float c)
    {
        if (!float.IsNaN(Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (-2 * a * b)) * Mathf.Rad2Deg))
        {
            return Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (2 * a * b)) * Mathf.Rad2Deg;
        }
        else
        {
            return 1;
        }
    }

    public void Solve()
    {
        Vector3 middle =  Vector3.Lerp(UpperDummy.position, EndDummy.position, 0.5f);
        Vector3 dir = (MiddleDummy.position - middle).normalized;
        distanceToMiddle = Vector3.Distance(middle, MiddleDummy.position);
        Vector3 middlePole =  Vector3.Lerp(middle + dir * (distanceToMiddle + 0.25f), MiddleDummy.position + -MiddleDummy.up * 0.25f, 0.5f);
        Pole.position = middlePole;//MiddleDummy.position + dir * (distanceToMiddle + 0.25f);
        Quaternion upperWorld = Quaternion.identity;
        Quaternion middleWorld = Quaternion.identity;

        a = MiddleDummy.localPosition.magnitude;
        b = EndDummy.localPosition.magnitude;
        c = Vector3.Distance(UpperDummy.position, Target.position);
        en = Vector3.Cross(Target.position - UpperDummy.position, Pole.position - UpperDummy.position);
        // Debug.Log("The angle is: " + CosAngle(a, b, c));
        Debug.DrawLine(UpperDummy.position, Target.position);
        Debug.DrawLine((UpperDummy.position + Target.position) / 2, MiddleDummy.position);
        Debug.DrawLine(middle, middle + dir * (distanceToMiddle + 0.25f), Color.red);
        Debug.DrawLine(MiddleDummy.position, MiddleDummy.position + -MiddleDummy.up * 0.25f, Color.green);
        Debug.DrawLine(EndDummy.position, EndDummy.position + EndDummy.right *0.25f, Color.black);
        Debug.DrawLine(EndDummy.position, EndDummy.position + EndDummy.forward *0.25f, Color.black);
        Debug.DrawLine(EndDummy.position, EndDummy.position + EndDummy.up *0.25f, Color.black);

        //Set the rotation of the upper arm
        upperWorld = Quaternion.LookRotation(Target.position - UpperDummy.position, Quaternion.AngleAxis(UpperElbowRotation, MiddleDummy.position - UpperDummy.position) * (en));
        upperWorld *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, MiddleDummy.localPosition));
        upperWorld = Quaternion.AngleAxis(-CosAngle(a, c, b), -en) * upperWorld;
         UpperDummy.rotation = upperWorld;
        

        //set the rotation of the middle arm
        middleWorld = Quaternion.LookRotation(Target.position - MiddleDummy.position, Quaternion.AngleAxis(MiddleElbowRotation, EndDummy.position - MiddleDummy.position) * (en));
        middleWorld *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, EndDummy.localPosition));
        MiddleDummy.rotation = middleWorld;
        EndDummy.localRotation =  Quaternion.AngleAxis( _wristTwistAngle, Vector3.right);
        EndDummy.localRotation *=  Quaternion.AngleAxis( _wristSwingYAngle, Vector3.up);
        EndDummy.localRotation *=  Quaternion.AngleAxis( _wristSwingZAngle, Vector3.forward);
       
        MiddleDummy.localRotation = MiddleDummy.localRotation * Quaternion.AngleAxis(_wristTwistAngle * 0.05f, Vector3.right) * Quaternion.AngleAxis(_wristSwingYAngle * 0.05f, Vector3.up) * Quaternion.AngleAxis(_wristSwingZAngle* 0.05f, Vector3.forward);
        // EndDummy.localRotation = Target.rotation * Quaternion.Inverse(EndDummy.parent.rotation);

        if (_middleJoint != null && _useArticulations)
        {
           // Quaternion local = Quaternion.Inverse(LowerDummy.parent.rotation) * lowerWorld;
            _middleJoint.SetDriveTargetRotation(MiddleDummy.localRotation);
        }

        if (_upperJoint != null && _useArticulations)
        {
            //Quaternion local = Quaternion.Inverse(UpperDummy.parent.rotation) * upperWorld ;
            _upperJoint.SetDriveTargetRotation(UpperDummy.localRotation);
        }
        Vector3 endRotLocal = Vector3.zero;
        if (_endJoint != null && _useArticulations)
            _endJoint.SetDriveTargetRotation(EndDummy.localRotation);
    }
   
}
