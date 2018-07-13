using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDisplays : MonoBehaviour {

	[SerializeField] private GameObject speedField;
	[SerializeField] private GameObject pointField;

	private TextMesh speedometer;
	private TextMesh pointCounter;

	private PointCounter pointScript;

	private new Rigidbody rigidbody;

	// Use this for initialization
	void Start()
	{
		rigidbody = transform.root.GetComponent<Rigidbody>();

		speedometer = speedField.GetComponent<TextMesh>();
		pointCounter = pointField.GetComponent<TextMesh>();

		pointScript = GetComponent<PointCounter>();
	}

	// Update is called once per frame
	void Update()
	{
		float velocity = transform.InverseTransformDirection(rigidbody.velocity).z * 3.6f;
		speedometer.text = string.Format("{0:0}", velocity);
		
		pointCounter.text = pointScript.Points.ToString();
	}
}
