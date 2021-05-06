using UnityEngine;
using System.Collections;
using SoftHand;
using System.Collections.Generic;

public class PIDTest : MonoBehaviour {

	public PID pid;
	public float speed;
	public Rigidbody body;
	//public Rigidbody rb;
	public float _totalAttachedMasses;
	public int _totalAttachedBodies;
    public int totalAttachedBodies { get { return _totalAttachedBodies > 0 ? _totalAttachedBodies : 1; } set { _totalAttachedBodies = value; } } // make sure we never retur 0
	public Transform setpoint; // target
	public float toVel = 20f;
	public float maxVel = 100f;
	public float maxForce = 100f;
	public float gain = 35f;
	public ForceMode forceMode;

	public float alignmentSpeed = 0.025f;
	public float alignmentDamping = 0.2f;

	private float timer;


	private void Awake()
    {
		

	}
    void Update () {
		//setpoint.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * speed, 0, 0);
		//actual.Translate(
		//	pid.Update(setpoint.position.x, actual.position.x, Time.deltaTime),
		//	pid.Update(setpoint.position.y, actual.position.y, Time.deltaTime),
		//	pid.Update(setpoint.position.z, actual.position.z, Time.deltaTime));
		if (Physics.autoSimulation)
			return; // do nothing if the automatic simulation is enabled

		timer += Time.deltaTime;

		// Catch up with the game time.
		// Advance the physics simulation in portions of Time.fixedDeltaTime
		// Note that generally, we don't want to pass variable delta to Simulate as that leads to unstable results.
		while (timer >= Time.fixedDeltaTime)
		{
			timer -= Time.fixedDeltaTime;
			Physics.Simulate(Time.fixedDeltaTime);
		}

		// Here you can access the transforms state right after the simulation, if needed
	}

	private void FixedUpdate()
    {
		if (!body)
			return;
		// cacl distance
		Vector3 dist = setpoint.transform.position - body.transform.position;
		// calc a target vel proportional to distance (clamped to maxVel)
		Vector3 tgtVel = Vector3.ClampMagnitude(toVel * dist, maxVel);
		// calculate the velocity error
		Vector3 error = tgtVel - body.velocity;
		// calc a force proportional to the error (clamped to maxForce)
		Vector3 force = Vector3.ClampMagnitude(gain * error, maxForce);

		Quaternion deltaRotation = Quaternion.Inverse(body.transform.rotation) * setpoint.transform.rotation;
		Vector3 deltaAngles = GetRelativeAngles(deltaRotation.eulerAngles);
		Vector3 worldDeltaAngles = body.transform.TransformDirection(deltaAngles);
		// rootBody.
		Vector3 GetRelativeAngles(Vector3 angles)
		{
			Vector3 relativeAngles = angles;
			if (relativeAngles.x > 180f)
				relativeAngles.x -= 360f;
			if (relativeAngles.y > 180f)
				relativeAngles.y -= 360f;
			if (relativeAngles.z > 180f)
				relativeAngles.z -= 360f;

			return relativeAngles;
		}

		body.AddForce(force * (body.mass + (_totalAttachedMasses/totalAttachedBodies)));
		body.AddTorque((body.mass + _totalAttachedMasses) * alignmentSpeed * worldDeltaAngles - alignmentDamping * body.angularVelocity * (body.mass + _totalAttachedMasses ));
		

		//Vector3 target = new Vector3(
		//	pid.Update(setpoint.position.x, actualRb.transform.position.x, Time.fixedDeltaTime), // x
		//	pid.Update(setpoint.position.y, actualRb.transform.position.y, Time.fixedDeltaTime), // y
		//	pid.Update(setpoint.position.z, actualRb.transform.position.z, Time.fixedDeltaTime)); // z
	}
}
	