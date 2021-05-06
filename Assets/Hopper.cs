using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hopper : MonoBehaviour
{
    public ArticulationBody rotationBody;
    public ArticulationBody pistonBody;

    [Range(-45, 45)]
    public float rotationDrive;

    [Range(-0.5f,0.5f)]
    public float positionDrive;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var xAxis = Input.GetAxis("Horizontal");
        var jump = Input.GetAxis("Vertical");
        rotationDrive = Mathf.Lerp(-20, 20, Mathf.InverseLerp(-1, 1, xAxis));
        positionDrive = Mathf.Lerp(-0.8f, 0.8f, Mathf.InverseLerp(-1, 1, jump));

        var drive = rotationBody.xDrive;
        drive.target = rotationDrive;
        rotationBody.xDrive = drive;

        var drive2 = pistonBody.xDrive;
        drive2.target = positionDrive;
        pistonBody.xDrive = drive2;
    }
 
}
