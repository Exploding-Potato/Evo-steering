using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualSteering : MonoBehaviour {

	[SerializeField] GameObject car;

	CarSteering steering;

	private void Start()
	{
		steering = car.GetComponent<CarSteering>();
	}
	
	void Update () {
		if (steering != null && steering.gameObject.activeSelf)
		{
			steering.Throttle = Input.GetAxis("Vertical");
			steering.DesiredAngle = Input.GetAxis("Horizontal");
			//steering.Brakes = Input.GetKey(KeyCode.Space) ? 1 : 0;
		}
	}
}
