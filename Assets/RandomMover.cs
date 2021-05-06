using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMover : MonoBehaviour
{
    public float fractionOfJourney, fractionOfRotJourney;
    private Rigidbody _rb;
    private Vector3 _position;
    private Quaternion _rotation;
    public float waitForSec;
    public float rotateSpeed = 0.02f;
    public float moveSpeed = 0.02f;
    public bool run = true;
    // Time when the movement started.
    [Range(0.1f, 3f)]
    public float minRange;
    [Range(1f, 5f)]
    public float maxRange;

    // Total distance between the markers.
    public float startTime;
    public float deltaPos;
    private Quaternion deltaRot;
    public bool rotate = true, move = false;
    public static event System.Action OnStartMoving;


    private void Awake()
    {
        //minMax.min = 1f;
        //minMax.max = 3f;
        _rb = GetComponent<Rigidbody>();
        // Keep a note of the time the movement started.
        startTime = Time.time;

        // Calculate the journey length.
        deltaPos = Vector3.Distance(transform.position, _position);
    }
    private void Start()
    {
        StartCoroutine(SetRandomlValuesOverTime());
    }

    Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f));
    }

    Quaternion GetRandomRotation()
    {
        return Random.rotation;
    }

    private void FixedUpdate()
    {
        if (_rb)
        {
            //_rb.MovePosition(_position * Time.deltaTime * speed);
            //Quaternion deltaRotation = Quaternion.Euler(_euler_angle * Time.fixedDeltaTime * speed);
            //_rb.transform.Rota(_rb.rotation * deltaRotation);
        }
    }

    private void Update()
    {
        deltaPos = Vector3.Distance(transform.position, _position);
       
        // Distance moved equals elapsed time times speed..     
        float distCovered = (Time.time - startTime) * moveSpeed;

        // Fraction of journey completed equals current distance divided by total distance.
         fractionOfJourney = distCovered / deltaPos;       

        // Set our position as a fraction of the distance between the markers.
       if (move)
        transform.position = Vector3.Lerp(transform.position, _position, fractionOfJourney * 0.5f); // * 0.5f will slow down the movement by half the rotation speed
        if (rotate)
        transform.rotation = Quaternion.Lerp(transform.rotation, _rotation, fractionOfJourney);
    }

    IEnumerator SetRandomlValuesOverTime()
    {
        while (run)
        {
            _position = GetRandomPosition();
            _rotation = GetRandomRotation();
            waitForSec = Random.Range(minRange, maxRange);
            startTime = Time.time;
            yield return new WaitForSeconds(0.1f);
            OnStartMoving?.Invoke();
            yield return new WaitForSeconds(waitForSec);
        }
    }
}

