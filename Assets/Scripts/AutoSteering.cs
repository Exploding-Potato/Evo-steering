using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSteering : MonoBehaviour {
	
	[SerializeField] int carCount;
	[SerializeField] bool spawnAtPoint;
	[SerializeField] GameObject spawnsParent;

	[SerializeField] GameObject firstTrigger;

	[SerializeField] GameObject prefab;
	[SerializeField] int[] hiddenLayerSizes;

	[SerializeField] float eliminationPercentage = 0.8f;

	[SerializeField] float mutationMagnitude;
	[SerializeField] float heavyMutationRate = 0.03f;
	[SerializeField] float heavyMutationMultiplier = 10;

	[SerializeField] float testTime;

	[SerializeField] float raycastDistance = 100.0f;
	[SerializeField] float defaultRaycastDistance = 100.0f;

	[SerializeField] float timeScale = 1;
	
	Transform[] spawns;
	List<GameObject> carList;

	int generationNumber = 0;

	float FastSigmoid(float x)
	{
		return x / (1 + Mathf.Abs(x));
	}

	// Use this for initialization
	void Start () {
		if (spawnAtPoint)
		{
			spawns = new Transform[carCount];
			for (int i = 0; i < spawns.Length; i++)
			{
				spawns[i] = spawnsParent.transform;
			}
		}
		else
		{
			// To make sure the parent tranform isn't included
			Transform[] spawnsPlusParent = spawnsParent.GetComponentsInChildren<Transform>();
			spawns = new Transform[spawnsPlusParent.Length - 1];
			Array.Copy(spawnsPlusParent, 1, spawns, 0, spawnsPlusParent.Length - 1);
		}

		carList = new List<GameObject>();

		if (!spawnAtPoint && carCount > spawns.Length)
		{
			Debug.LogError("Not enough car spawns! Adjusting number of cars.");
			carCount = spawns.Length;
		}

		Time.timeScale = Math.Abs(timeScale);
		StartCoroutine("LearningLoop");
	}

	private IEnumerator LearningLoop ()
	{
		while (true)
		{
			yield return StartCoroutine("TestGeneration");

			generationNumber++;
		}
	}

	private IEnumerator TestGeneration ()
	{
		if (carList == null || carList.Count == 0)
		{
			// Instantiates car GOs and their Car compontnts
			// Searches from 1 to not take the Transform of parent
			for (int i = 0; i < carCount; i++)
			{
				GameObject newCar = Instantiate(prefab, spawns[i].position, spawns[i].rotation);
				Car newCarComp = newCar.AddComponent<Car>();

				newCarComp.Initialize(hiddenLayerSizes, FastSigmoid, firstTrigger);
				newCarComp.nextTrigger = firstTrigger;

				newCarComp.raycastDistance = raycastDistance;
				newCarComp.defaultDistance = defaultRaycastDistance;

				carList.Add(newCar);
			}
		}
		else
		{
			// Evaluation
			Evaluate(carList);

			// Recreation
			carList = Refill(carList, spawns, carCount);
		}

		print($"Starting evaluating generation {generationNumber}");
		yield return new WaitForSeconds(testTime);
		print($"Evaluating generation {generationNumber} has finished");

	}

	private List<GameObject> Evaluate(List<GameObject> carList)
	{
		// Sorting in ascending order
		carList.Sort((car1, car2) =>
			car2.GetComponent<Car>().Evaluate().CompareTo(
			car1.GetComponent<Car>().Evaluate()));
		
		// Removal
		int remainingCars = (int)(carList.Count * (1 - Mathf.Clamp01(eliminationPercentage)));
		while (carList.Count > remainingCars)
		{
			// Temporary handle
			var temp = carList[remainingCars];
			// From the list
			carList.RemoveAt(remainingCars);
			// From the world
			Destroy(temp);
		}
		
		return carList;
	}

	private List<GameObject> Refill(List<GameObject> carList, Transform[] spawns, int carCount)
	{
		int spawnIdx = 0;
		// The good items aren't destroyed or changed but brought to start
		for (int i = 0; i < carList.Count; i++, spawnIdx++)
		{
			// Reset position
			carList[i].transform.position = spawns[spawnIdx].position;
			carList[i].transform.rotation = spawns[spawnIdx].rotation;
			// Reset points
			carList[i].GetComponent<RoofCounters>().ResetPoints(firstTrigger);
			carList[i].GetComponent<Car>().nextTrigger = firstTrigger;
		}

		int remaining = carList.Count;
		for (int i = 0; carList.Count < carCount; i = (i + 1) % remaining, spawnIdx++)
		{
			GameObject newCar = Instantiate(prefab, spawns[spawnIdx].position, spawns[spawnIdx].rotation);
			Car newCarComp = newCar.AddComponent<Car>();

			float actualMutationMag;
			if (UnityEngine.Random.Range(0.0f, 1.0f) < heavyMutationRate)
				actualMutationMag = mutationMagnitude * heavyMutationMultiplier;
			else
				actualMutationMag = mutationMagnitude;

			print($"original: {carList[i].GetComponent<Car>().WeightsSum()}, i = {i}");

			newCarComp.Initialize(hiddenLayerSizes, FastSigmoid, firstTrigger, 
				NeuralNet.Mutate(new NeuralNet(carList[i].GetComponent<Car>().nnet), actualMutationMag));

			print($"new: {newCarComp.WeightsSum()}, i = {i}");

			newCarComp.nextTrigger = firstTrigger;

			newCarComp.raycastDistance = raycastDistance;
			newCarComp.defaultDistance = defaultRaycastDistance;

			carList.Add(newCar);
		}

		return carList;
	}
}
