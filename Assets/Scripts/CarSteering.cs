using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSteering : MonoBehaviour
{

	new Rigidbody rigidbody;

	[SerializeField] float motorForce = 30;
	[SerializeField] float reverseFraction = 0.4f;
	[SerializeField] WheelCollider rightFront;
	[SerializeField] WheelCollider leftFront;
	[SerializeField] WheelCollider rightRear;
	[SerializeField] WheelCollider leftRear;
	[SerializeField] float maxAngle = 15;
	[SerializeField] float steeringSpeed = 1;
	[SerializeField] float brakesStrength = 1;

	[SerializeField] float steeringDampening = 4;

	float _throttle;
	float _brakes;
	float _desiredAngle;

	float currentAngle;
	float currentForce;
	float currentBrake;

	Func<float, float, float> steeringFunc = (x, degree) => (float)Math.Pow(1 / (x + 1), 1 / degree);

	public float[] Steering
	{
		set
		{
			if (value.Length < 2)
				Debug.LogError("Input array of a wrong size");

			Throttle = value[0];
			DesiredAngle = value[1];
		}
	}

	public float Throttle
	{
		get { return _throttle; }
		set
		{
			float speed = transform.InverseTransformDirection(rigidbody.velocity).z;
			// Moving speed same as throttle
			if (Mathf.Sign(speed) == Mathf.Sign(value)
				|| speed < 0.001f)
			{
				_throttle = Mathf.Clamp(value, -reverseFraction, 1);
				Brakes = 0;
			}
			else
			{
				_throttle = 0;
				Brakes = Math.Abs(value);
			}
		}
	}

	public float Brakes
	{
		get { return _brakes; }
		set { _brakes = Mathf.Clamp01(value); }
	}

	public float DesiredAngle
	{
		get { return _desiredAngle; }
		set
		{
			_desiredAngle = Mathf.Clamp(value, -maxAngle, maxAngle);
			
			_desiredAngle *= steeringFunc(rigidbody.velocity.magnitude, steeringDampening);
		}
	}

	private void Start()
	{
		rigidbody = this.GetComponent<Rigidbody>();

		rigidbody.centerOfMass += new Vector3(0, -0.45f, 0.2f);
	}

	// Might change it to work on any number of wheels with accelaration and brakes
	// Using fields instead of getters because performance
	void Update()
	{
		// Sets throttle
		currentForce = _throttle * motorForce;
		rightFront.motorTorque = currentForce;
		leftFront.motorTorque = currentForce;
		rightRear.motorTorque = currentForce;
		leftRear.motorTorque = currentForce;

		// Sets brakes
		currentBrake = _brakes * brakesStrength;
		rightFront.brakeTorque = currentBrake;
		leftFront.brakeTorque = currentBrake;
		rightRear.brakeTorque = currentBrake;
		leftRear.brakeTorque = currentBrake;

		// Sets angle of steering
		currentAngle = Mathf.Lerp(currentAngle, _desiredAngle * maxAngle, steeringSpeed * Time.deltaTime);
		rightFront.steerAngle = currentAngle;
		leftFront.steerAngle = currentAngle;
	}

	// Copied from http://projects.edy.es/trac/edy_vehicle-physics/wiki/TheStabilizerBars
	void FixedUpdate()
	{
		float antiRoll = 15000;
		WheelHit hit;
		
		{
			bool groundedL = leftFront.GetGroundHit(out hit);

			float travelL;
			if (groundedL)
				travelL = (-leftFront.transform.InverseTransformPoint(hit.point).y - leftFront.radius) / leftFront.suspensionDistance;
			else
				travelL = 1.0f;

			bool groundedR = rightFront.GetGroundHit(out hit);

			float travelR;
			if (groundedL)
				travelR = (-rightFront.transform.InverseTransformPoint(hit.point).y - rightFront.radius) / rightFront.suspensionDistance;
			else
				travelR = 1.0f;

			var antiRollForce = (travelL - travelR) * antiRoll;

			if (groundedL)
				rigidbody.AddForceAtPosition(transform.up * -antiRollForce, leftFront.transform.position);
			if (groundedR)
				rigidbody.AddForceAtPosition(transform.up * antiRollForce, rightFront.transform.position);
		}
		{
			bool groundedL = leftRear.GetGroundHit(out hit);

			float travelL;
			if (groundedL)
				travelL = (-leftRear.transform.InverseTransformPoint(hit.point).y - leftRear.radius) / leftRear.suspensionDistance;
			else
				travelL = 1.0f;

			bool groundedR = rightRear.GetGroundHit(out hit);

			float travelR;
			if (groundedL)
				travelR = (-rightRear.transform.InverseTransformPoint(hit.point).y - rightRear.radius) / rightRear.suspensionDistance;
			else
				travelR = 1.0f;

			var antiRollForce = (travelL - travelR) * antiRoll;

			if (groundedL)
				rigidbody.AddForceAtPosition(transform.up * -antiRollForce, leftRear.transform.position);
			if (groundedR)
				rigidbody.AddForceAtPosition(transform.up * antiRollForce, rightRear.transform.position);
		}
	}
}
