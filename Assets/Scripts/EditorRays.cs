using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EditorRays : MonoBehaviour {

	List<Transform> rayTransforms = new List<Transform>();

	// Use this for initialization
	void Start () {
		Transform rayContainer = this.transform.Find("Rays");
		foreach (Transform rayTransform in rayContainer.transform)
		{
			rayTransforms.Add(rayTransform);
		}
	}
	
	// Update is called once per frame
	void Update () {
		foreach (Transform tra in rayTransforms)
		{
			RaycastHit hit;
			if (Physics.Raycast(tra.position, tra.forward, out hit, 100.0f, 1 << LayerMask.NameToLayer("Default")))
				Debug.DrawRay(tra.position, tra.forward * hit.distance);
		}
	}
}
