using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
	private CarSteering steering;
	private new Rigidbody rigidbody;
	
	private List<Transform> rays = new List<Transform>();
	private float raycastDistance = 100.0f;

	private NeuralNet nnet;
	private int inputCount;
	private int outputCount;
	private GameObject nextTrigger;

	public GameObject NextTrigger
	{
		get { return nextTrigger; }
		set { nextTrigger = value; GetComponent<PointCounter>().nextTrigger = value; }
	}

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		
		// Adds transforms of all active rays into a list
		Transform rayContainer = transform.Find("Rays");
		foreach (Transform rayTransform in rayContainer.transform)
			if (rayTransform.gameObject.activeSelf)
				rays.Add(rayTransform);

		// Caches the inputCount and outputCount
		inputCount = rays.Count + 3;	//Rays, speed, distance to next
		outputCount = 2;				// Hardcoded: throttle (with brake), desiredAngle
	}

	public float WeightsSum()
	{
		return nnet.Sum();
	}

	public void Initialize(int[] hiddenLayerSizes, Func<float, float> activationFunction, float raycastDistance, GameObject firstTrigger, Car oldCar = null)
	{
		// Initializes a NeuralNet
		if (oldCar == null)
		{
			int[] layerSizes = new int[hiddenLayerSizes.Length + 2];
			layerSizes[0] = inputCount;
			hiddenLayerSizes.CopyTo(layerSizes, 1);
			layerSizes[layerSizes.Length - 1] = outputCount;
			
			nnet = new NeuralNet(layerSizes, activationFunction);
		}
		else
		{
			nnet = new NeuralNet(oldCar.nnet);
		}

		// Caches the CarSteering component
		steering = GetComponent<CarSteering>();

		NextTrigger = firstTrigger;
	}

	// Here be place for steering and other bullshit
	void Update()
	{
		// Filling the intermediate array: rays, directions to next, forward speed
		float[] inputs = new float[inputCount];
		for (int i = 0; i < rays.Count; i++)
		{
			RaycastHit hit;
			// Yes, I'm using ternary operators. Bite me.
			inputs[i] = Physics.Raycast(rays[i].position, rays[i].forward, out hit, raycastDistance, 1 << LayerMask.NameToLayer("Default")) ?
				hit.distance : raycastDistance;
		}

		// To next trigger
		Vector3 toNextTrigger = transform.InverseTransformDirection(rigidbody.position - nextTrigger.transform.position);
		inputs[inputCount - 3] = toNextTrigger.y;
		inputs[inputCount - 2] = toNextTrigger.z;

		// Forward speed
		inputs[inputCount - 1] = transform.InverseTransformDirection(rigidbody.velocity).z;

		// Pass steering
		steering.Steering = nnet.Predict(inputs);
	}

	public float Evaluate()
	{
		return GetComponent<PointCounter>().Points;
	}

	public void Mutate (float magnitude)
	{
		nnet.Mutate(magnitude);
	}

	public void Reset(Transform transform)
	{
		rigidbody.position = transform.position;
		rigidbody.rotation = transform.rotation;

		rigidbody.velocity = new Vector3();
	}
}