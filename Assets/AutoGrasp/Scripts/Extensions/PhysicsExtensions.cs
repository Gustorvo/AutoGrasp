using System.Collections.Generic;
using UnityEngine;

namespace SoftHand
{
    public static class PhysicsExtensions
    {
        public static void Reset(this ArticulationBody body)
        {
            var zeroed = new ArticulationReducedSpace(0f, 0f, 0f);
            body.jointPosition = zeroed;
            body.jointAcceleration = zeroed;
            body.jointForce = zeroed;
            body.jointVelocity = zeroed;

            body.angularVelocity = Vector3.zero;
            body.velocity = Vector3.zero;           
        }

        public static Vector3 GetArtBodyDriveTargets(this ArticulationBody body)
        {
            return new Vector3(
                body.xDrive.target,
                body.yDrive.target,
                body.zDrive.target
                );
        }

        public static void AddJointForceToMatchTargetRotation(this ArticulationBody body, Quaternion targetLocalRotation, float strength)
        {
            //Quaternion deltaRotation = Quaternion.Normalize(Quaternion.Inverse(body.transform.localRotation) * targetLocalRotation);
            //ArticulationReducedSpace driveTargetForce = new ArticulationReducedSpace(
            //    ((Mathf.DeltaAngle(0, deltaRotation.eulerAngles.x) * Mathf.Deg2Rad) / Time.fixedDeltaTime) * strength,
            //    ((Mathf.DeltaAngle(0, deltaRotation.eulerAngles.y) * Mathf.Deg2Rad) / Time.fixedDeltaTime) * strength,
            //    ((Mathf.DeltaAngle(0, deltaRotation.eulerAngles.z) * Mathf.Deg2Rad) / Time.fixedDeltaTime) * strength);
            Vector3 force = body.ToTargetRotationInReducedSpace(targetLocalRotation);
            ArticulationReducedSpace driveTargetForce = new ArticulationReducedSpace(
                Mathf.DeltaAngle(0, force.x) * Mathf.Deg2Rad / Time.fixedDeltaTime * strength,
                Mathf.DeltaAngle(0, force.y) * Mathf.Deg2Rad / Time.fixedDeltaTime * strength,
                Mathf.DeltaAngle(0, force.z) * Mathf.Deg2Rad / Time.fixedDeltaTime * strength);


            body.jointForce = driveTargetForce;
        }

        /// <summary>
        /// <para>Source: https://digitalopus.ca/site/pd-controllers/ </para>
        /// <para>Calculates the torque required to be applied to an articulation body to achieve the desired rotation. Works with Acceleration ForceMode.</para>
        /// </summary>
        /// <param name="articulationBody">The articulation body that the torque will be applied to</param>
        /// <param name="desiredRotation">The rotation that you'd like the articulation body to have</param>
        /// <param name="frequency">Frequency is the speed of convergence. If damping is 1, frequency is the 1/time taken to reach ~95% of the target value. i.e. a frequency of 6 will bring you very close to your target within 1/6 seconds.</param>
        /// <param name="damping"><para>damping = 1, the system is critically damped</para><para>damping is greater than 1 the system is over damped(sluggish)</para><para>damping is less than 1 the system is under damped(it will oscillate a little)</para></param>
        /// <returns>The torque value to be applied to the articulation body.</returns>         
        public static Vector3 CalculateRequiredTorque(this ArticulationBody articulationBody, Quaternion desiredRotation, float frequency = 6f, float damping = 1f)
        {
            float kp = 6f * frequency * (6f * frequency) * 0.25f;
            float kd = 4.5f * frequency * damping;

            Vector3 x;
            float xMag;
            Quaternion q = desiredRotation * Quaternion.Inverse(articulationBody.transform.rotation);            
            q = q.Shorten();
            q.ToAngleAxis(out xMag, out x);
            x.Normalize();
            x *= Mathf.Deg2Rad;
            Vector3 pidv = kp * x * xMag - kd * articulationBody.angularVelocity;
            Quaternion rotInertia2World = articulationBody.inertiaTensorRotation * articulationBody.transform.rotation;
            pidv = Quaternion.Inverse(rotInertia2World) * pidv;
            pidv.Scale(articulationBody.inertiaTensor);
            pidv = rotInertia2World * pidv;
            return pidv;

        }

        /// <summary>
        /// Sets an articulation body drive target rotation to match a given targetLocalRotation.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetLocalRotation"></param>
        public static Vector3 SetDriveTargetRotation(this ArticulationBody body, Quaternion targetLocalRotation/*, out Vector3 targetRotatioInReducedSpace*/)
        {
            Vector3 targetRotatioInReducedSpace = body.ToTargetRotationInReducedSpace(targetLocalRotation);

            // assign to the drive targets...
            ArticulationDrive xDrive = body.xDrive;
            xDrive.target = targetRotatioInReducedSpace.x;
            body.xDrive = xDrive;

            ArticulationDrive yDrive = body.yDrive;
            yDrive.target = targetRotatioInReducedSpace.y;
            body.yDrive = yDrive;

            ArticulationDrive zDrive = body.zDrive;
            zDrive.target = targetRotatioInReducedSpace.z;
            body.zDrive = zDrive;

            return targetRotatioInReducedSpace;
        }

        /// <summary>
        /// Converts targetLocalRotation into reduced space of this articulation body.
        /// </summary>
        /// <param name="body"> ArticulationBody to apply rotation to </param>
        /// <param name="targetLocalRotation"> target's local rotation this articulation body is trying to mimic </param>
        /// <returns></returns>

        public static Vector3 ToTargetRotationInReducedSpace(this ArticulationBody body, Quaternion targetLocalRotation)
        {
            if (body.isRoot)
                return Vector3.zero;

            // check whether the modulus square is less than the minimum value
            // to prevent Quaternion interpolation error (CompareApproximately (aScalar, 0.0F)),
            // which is caused by the quaternion value is too small
            if (Quaternion.Dot(targetLocalRotation, targetLocalRotation) < Quaternion.kEpsilon)
                return Vector3.zero;
            if (targetLocalRotation == Quaternion.identity) // is OK to compare Quaternions like this, since unity overloads == operator and compares Quaternions' Dot products
                return Vector3.zero;

            Vector3 axis = Vector3.zero;
            float angle = 0f;

            //Convert rotation to angle-axis representation (angles in degrees)
            targetLocalRotation.ToAngleAxis(out angle, out axis);

            // Converts into reduced coordinates and combines rotations (anchor rotation and target rotation)       
            Vector3 rotInReducedSpace = Quaternion.Inverse(body.anchorRotation) * axis * angle;
            return rotInReducedSpace;
        }


        /// <summary>
        /// Assigns articulation body joint drive target rotations for the entire hierarchy
        /// Currently doesn't work/sets wrong joints' rotations because of a bug https://forum.unity.com/threads/featherstones-solver-for-articulations.792294/page-6#post-7145909
        /// </summary>
        /// <param name="bodies"> array of hierarchy of bodies to apply rotations to. </param>
        /// <param name="targetPoses"> array of transforms these art bodies try to mimic </param>
        /// <param name="startIndexes"> is obtained by calling articulationBody.GetDofStartIndices(startIndecies) </param> 
        /// <param name="driveTargets"> is obtained by calling articulationBody.GetDriveTargets(driveTargets) </param>
        public static void SetDriveRotations(this ArticulationBody body, ArticulationBody[] bodies, Pose[] targetPoses, ref List<int> startIndexes, ref List<float> driveTargets)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i].isRoot)
                    continue;
                int j = bodies[i].index;
                int index = startIndexes[j];

                bool rotateX = bodies[i].twistLock != ArticulationDofLock.LockedMotion;
                bool rotateY = bodies[i].swingYLock != ArticulationDofLock.LockedMotion;
                bool rotateZ = bodies[i].swingZLock != ArticulationDofLock.LockedMotion;

                Vector3 targets = bodies[i].ToTargetRotationInReducedSpace(targetPoses[i].rotation);

                int dofIndex = 0;
                if (rotateX)
                {
                    float xClamped = Mathf.Clamp(targets.x, bodies[i].xDrive.lowerLimit, bodies[i].xDrive.upperLimit);
                    driveTargets[index] = xClamped * Mathf.Deg2Rad;
                    dofIndex++;
                }
                if (rotateY)
                {
                    float yClamped = Mathf.Clamp(targets.y, bodies[i].yDrive.lowerLimit, bodies[i].yDrive.upperLimit);
                    driveTargets[index + dofIndex] = yClamped * Mathf.Deg2Rad;
                    dofIndex++;
                }
                if (rotateZ)
                {
                    float zClamped = Mathf.Clamp(targets.z, bodies[i].zDrive.lowerLimit, bodies[i].zDrive.upperLimit);
                    driveTargets[index + dofIndex] = zClamped * Mathf.Deg2Rad;
                }
            }

            body.SetDriveTargets(driveTargets);
        }

        public static void AddForce(this ArticulationBody ab, Vector3 force, ForceMode mode)
        {
            switch (mode)
            {
                case ForceMode.Force:
                    ab.AddForce(force);
                    break;
                case ForceMode.Impulse:
                    ab.AddForce(force / Time.fixedDeltaTime);
                    break;
                case ForceMode.Acceleration:
                    ab.AddForce(force * ab.mass);
                    break;
                case ForceMode.VelocityChange:
                    ab.AddForce(force * ab.mass / Time.fixedDeltaTime);
                    break;
            }
        }

        /// <summary>
        /// If the quaternion is going the long way around the axis, then this function will
        /// find the complementary shorter angle on the axis
        /// </summary>
        /// <param name="value">The original quaternion value</param>
        /// <returns>The shortened quaternion value</returns>
        public static Quaternion Shorten(this Quaternion value)
        {
            //Source: https://answers.unity.com/questions/147712/what-is-affected-by-the-w-in-quaternionxyzw.html
            //"If w is -1 the quaternion defines +/-2pi rotation angle around an undefined axis"
            //So by doing this we check to see if that is true, and if so turn it the other way around
            if (value.w < 0)
            {
                value.x = -value.x;
                value.y = -value.y;
                value.z = -value.z;
                value.w = -value.w;
            }
            return value;
        }

        /// <summary>
        /// <para>Source: https://answers.unity.com/questions/48836/determining-the-torque-needed-to-rotate-an-object.html</para>
        /// <para>Calculates the torque required to be applied to a rigidbody to achieve the desired rotation. Works with Force ForceMode.</para>
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the torque will be applied to</param>
        /// <param name="desiredRotation">The rotation that you'd like the rigidbody to have</param>
        /// <param name="timestep">Time to achieve change in position.</param>
        /// <param name="maxTorque">The max torque the result can have.</param>
        /// <returns>The torque value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredTorqueForRotation(this Rigidbody rigidbody, Quaternion desiredRotation, float timestep = 0.02f, float maxTorque = float.MaxValue)
        {
            Vector3 axis;
            float angle;
            Quaternion rotDiff = desiredRotation * Quaternion.Inverse(rigidbody.transform.rotation);
            rotDiff = rotDiff.Shorten();
            rotDiff.ToAngleAxis(out angle, out axis);
            axis.Normalize();

            angle *= Mathf.Deg2Rad;
            Vector3 desiredAngularAcceleration = axis * angle / (timestep * timestep);

            Quaternion q = rigidbody.rotation * rigidbody.inertiaTensorRotation;
            Vector3 T = q * Vector3.Scale(rigidbody.inertiaTensor, Quaternion.Inverse(q) * desiredAngularAcceleration);
            Vector3 prevT = q * Vector3.Scale(rigidbody.inertiaTensor, Quaternion.Inverse(q) * (rigidbody.angularVelocity / timestep));

            var deltaT = T - prevT;
            if (deltaT.sqrMagnitude > maxTorque * maxTorque)
                deltaT = deltaT.normalized * maxTorque;

            return deltaT;
        }
        public static Vector3 CalculateRequiredTorqueForRotation(this ArticulationBody articulationBody, Quaternion desiredRotation, float timestep = 0.02f, float maxTorque = float.MaxValue)
        {
            Vector3 axis;
            float angle;
            Quaternion rotDiff = desiredRotation * Quaternion.Inverse(articulationBody.transform.rotation);
            rotDiff = rotDiff.Shorten();
            rotDiff.ToAngleAxis(out angle, out axis);
            axis.Normalize();

            angle *= Mathf.Deg2Rad;
            Vector3 desiredAngularAcceleration = axis * angle / (timestep * timestep);

            Quaternion q = articulationBody.transform.rotation * articulationBody.inertiaTensorRotation;
            Vector3 T = q * Vector3.Scale(articulationBody.inertiaTensor, Quaternion.Inverse(q) * desiredAngularAcceleration);
            Vector3 prevT = q * Vector3.Scale(articulationBody.inertiaTensor, Quaternion.Inverse(q) * (articulationBody.angularVelocity / timestep));

            var deltaT = T - prevT;
            if (deltaT.sqrMagnitude > maxTorque * maxTorque)
                deltaT = deltaT.normalized * maxTorque;

            return deltaT;
        }

        /// <summary>
        /// This algorithm first calculates the velocity needed proportionally to the current distance to the target position,
        // then estimates the force necessary to reach the desired velocity - this way it automatically
        // accelerates during most of the time and decelerates when getting near to the target.        
        /// <param name="hand"></param>
        /// <returns></returns>
        public static Vector3 CalculateLinearForce(this ArticulationBody body, Vector3 desiredPosition, float toVelocity = 55f, float maxVelocity = 100f, float maxForce = 100f, float gain = 20f)
        {
            Vector3 delta = desiredPosition - body.transform.position;
            // calc a target velocity proportional to distance (clamped to max velocity)
            Vector3 tgtVel = Vector3.ClampMagnitude(toVelocity * delta, maxVelocity);
            // calculate the velocity error
            Vector3 error = tgtVel - body.velocity;
            // calculate a force proportional to the error (clamped to maxForce)
            return Vector3.ClampMagnitude(gain * error, maxForce) /* * body.mass */;
        }

        public static Vector3 CalculateLinearForce(this Rigidbody body, Vector3 desiredPosition, float toVelocity = 55f, float maxVelocity = 100f, float maxForce = 100f, float gain = 20f)
        {
            Vector3 delta = desiredPosition - body.transform.position;
            // calc a target velocity proportional to distance (clamped to max velocity)
            Vector3 tgtVel = Vector3.ClampMagnitude(toVelocity * delta, maxVelocity);
            // calculate the velocity error
            Vector3 error = tgtVel - body.velocity;
            // calculate a force proportional to the error (clamped to maxForce)
            return Vector3.ClampMagnitude(gain * error, maxForce) /* * body.mass */;
        }
    }
}
