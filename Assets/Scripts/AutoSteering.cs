using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSteering : MonoBehaviour {

	[Space(10)]
	[SerializeField] private int carCount;
	[SerializeField] private bool spawnAtPoint;
	[SerializeField] private GameObject spawnsParent;
	[SerializeField] private GameObject prefab;
	[SerializeField] private GameObject firstTrigger;

	[Space(10)]
	[SerializeField] private int[] hiddenLayerSizes;

	[Space(10)]
	[SerializeField] private float mutationMagnitude = 1;
	[SerializeField] private float heavyMutationRate = 0.1f;
	[SerializeField] private float heavyMutationMultiplier = 10;

	[Space(10)]
	[SerializeField] private float testTime;
	[SerializeField] private float eliminationPercentage = 0.8f;

	[Space(10)]
	[SerializeField] private float raycastDistance = 100.0f;

	private Transform[] spawns;
	private List<GameObject> carList;

	private int generationNumber = 0;

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
				spawns[i] = spawnsParent.transform;
		}
		else
		{
			// To make sure the parent tranform isn't included we ignore the first element
			Transform[] spawnsPlusParent = spawnsParent.GetComponentsInChildren<Transform>();

			// And put it into separate array. Done once, so not horrible
			spawns = new Transform[spawnsPlusParent.Length - 1];
			Array.Copy(spawnsPlusParent, 1, spawns, 0, spawnsPlusParent.Length - 1);
		}

		carList = new List<GameObject>();

		if (!spawnAtPoint && carCount > spawns.Length)
		{
			Debug.LogError("Not enough car spawns! Adjusting number of cars.");
			carCount = spawns.Length;
		}
		
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
			for (int i = 0; i < carCount; i++)
				carList.Add(SpawnCar(spawns[i]));
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
			carList[i].GetComponent<PointCounter>().ResetPoints(firstTrigger);
			carList[i].GetComponent<Car>().NextTrigger = firstTrigger;
		}

		int remaining = carList.Count;
		for (int i = 0; carList.Count < carCount; i = (i + 1) % remaining, spawnIdx++)
		{
			float actualMutation = (UnityEngine.Random.value < heavyMutationRate) ?
				mutationMagnitude * heavyMutationMultiplier :
				mutationMagnitude;

			carList.Add(
				SpawnCar(spawns[i], carList[i].GetComponent<Car>(), actualMutation));
		}

		return carList;
	}

	GameObject SpawnCar (Transform spawn, Car car = null, float mutationMagnitude = 0)
	{
		GameObject newCar = Instantiate(prefab, spawn.position, spawn.rotation);
		Car newCarComp = newCar.AddComponent<Car>();

		newCarComp.Initialize(hiddenLayerSizes, FastSigmoid, raycastDistance, firstTrigger, car);

		// No point of mutating if magnitude == 0
		if (mutationMagnitude != 0)
			newCarComp.Mutate((float)mutationMagnitude);

		return newCar;
	}
}
