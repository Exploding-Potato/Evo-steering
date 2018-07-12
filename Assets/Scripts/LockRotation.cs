using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRotation : MonoBehaviour {

	[SerializeField] bool lockX = false;
	[SerializeField] bool lockY = false;
	[SerializeField] bool lockZ = false;

	Vector3 startingRotation;

	// Use this for initialization
	void Awake () {
		startingRotation = transform.rotation.eulerAngles;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 newRotation = transform.rotation.eulerAngles;

		if (lockX)
			newRotation.x = startingRotation.x;

		if (lockY)
			newRotation.y = startingRotation.y;

		if (lockZ)
			newRotation.z = startingRotation.z;

		transform.rotation = Quaternion.Euler(newRotation);
	}
}
