using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoofCounters : MonoBehaviour {

	[SerializeField] GameObject car;
	[SerializeField] GameObject speedField;
	[SerializeField] GameObject pointField;
	
	TextMesh speedometer;
	TextMesh pointCounter;

	new Rigidbody rigidbody;

	float points = 0;
	[SerializeField] int pointsIncrease = 10;
	[SerializeField] float pointTriggerDistance = 40;
	[SerializeField] float pointPenalty = 1;
	int pointsExtra = 0;
	int maxPointIncrease = 0;

	public GameObject nextTrigger;

	// Use this for initialization
	void Start () {
		rigidbody = car.GetComponent<Rigidbody>();

		speedometer = speedField.GetComponent<TextMesh>();
		pointCounter = pointField.GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		float velocity = transform.InverseTransformDirection(rigidbody.velocity).z * 3.6f;
		speedometer.text = string.Format("{0:0}", velocity);

		float distanceToNext = (nextTrigger.transform.position - transform.position).magnitude;
		pointsExtra = pointsIncrease - (int)(pointsIncrease * (distanceToNext / pointTriggerDistance));
		if (pointsExtra > maxPointIncrease)
			maxPointIncrease = pointsExtra;
		
		pointCounter.text = Points.ToString();
	}

	private void OnTriggerEnter(Collider other)
	{
		// If other is the right collider
		if (other.name == "ScoreTrigger"
			&& nextTrigger == other.gameObject)
		{
			points += pointsIncrease;
			
			// Sets nextTrigger
			nextTrigger = other.GetComponentInParent<NextSegment>().nextTrigger.transform.Find("ScoreTrigger").gameObject;

			// MaxPointIncrease to 0
			maxPointIncrease = 0;
		}
	}

	private void OnCollisionStay(Collision other)
	{
		if (other.transform.tag == "Terrain")
		{
			points -= pointPenalty * Time.fixedDeltaTime;
		}
	}

	public void ResetPoints (GameObject firstTrigger)
	{
		points = 0;
		pointsExtra = 0;

		nextTrigger = firstTrigger;
	}

	public int Points
	{
		get { return (int)(points + maxPointIncrease); }
	}
}
