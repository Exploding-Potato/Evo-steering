using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
	public float raycastDistance = 100.0f;
	public float defaultDistance = 100.0f;

	[HideInInspector] public NeuralNet nnet;

	public GameObject nextTrigger;
	
	CarSteering steering;
	new Rigidbody rigidbody;

	int rayCount;
	int inputCount;
	int outputCount;
	List<Transform> rays = new List<Transform>();

	private void Awake()
	{
		rigidbody = this.GetComponent<Rigidbody>();
		
		// Adds transforms of all rays into a list
		Transform rayContainer = this.transform.Find("Rays");
		foreach (Transform rayTransform in rayContainer.transform)
		{
			if (rayTransform.gameObject.activeSelf)
				rays.Add(rayTransform);
		}

		// Caches the inputCount and outputCount
		rayCount = rayContainer.childCount;
		inputCount = rayCount + 4;	//Rays, velicity, distance to next
		outputCount = 2;    // Hardcoded: throttle (with brake), desiredAngle
	}

	public float WeightsSum()
	{
		return nnet.Sum();
	}

	public void Initialize(int[] hiddenLayerSizes, Func<float, float> activationFunction, GameObject firstTrigger, NeuralNet newNet = null)
	{
		// Initializes a NeuralNet
		if (newNet == null)
		{
			int[] layerSizes = new int[hiddenLayerSizes.Length + 2];
			layerSizes[0] = inputCount;
			hiddenLayerSizes.CopyTo(layerSizes, 1);
			layerSizes[layerSizes.Length - 1] = outputCount;
			
			nnet = new NeuralNet(layerSizes, activationFunction);
		}
		else
		{
			nnet = newNet;
		}

		// Caches the CarSteering component
		steering = this.GetComponent<CarSteering>();

		this.GetComponent<RoofCounters>().nextTrigger = firstTrigger;
	}

	// Here be place for steering and other bullshit
	void Update()
	{
		// Filling the intermediate array
		// Rays, forward speed, sideways speed, bias
		float[] inputs = new float[inputCount];
		for (int i = 0; i < rayCount; i++)
		{
			RaycastHit hit;
			if (Physics.Raycast(rays[i].position, rays[i].forward, out hit, raycastDistance, 1 << LayerMask.NameToLayer("Default")))
				inputs[i] = hit.distance;
			else
				inputs[i] = defaultDistance;
		}

		// To next trigger
		Vector3 toNextTrigger = transform.InverseTransformDirection(rigidbody.position - nextTrigger.transform.position);
		inputs[inputCount - 5] = toNextTrigger.y;
		inputs[inputCount - 4] = toNextTrigger.z;

		// Forward and sideways velocity 
		inputs[inputCount - 3] = transform.InverseTransformDirection(rigidbody.velocity).z;
		inputs[inputCount - 2] = transform.InverseTransformDirection(rigidbody.velocity).x;
		
		// Pass steering
		steering.Steering = nnet.Predict(inputs);
	}

	public float Evaluate()
	{
		return this.GetComponent<RoofCounters>().Points;
	}
}