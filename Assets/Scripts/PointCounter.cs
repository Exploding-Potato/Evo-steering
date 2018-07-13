using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCounter : MonoBehaviour {
	
	[SerializeField] private int pointsPerCheckpoint = 10;
	[SerializeField] private int crashPenalty = 1;
	[SerializeField] private float pointTriggerDistance = 40;
	private float pointsBase = 0;
	private float maxPointsExtra = 0;

	public GameObject nextTrigger;

	public int Points
	{
		get { return (int)(pointsBase + maxPointsExtra); }
	}

	// Update is called once per frame
	void Update () {
		float distanceToNext = (nextTrigger.transform.position - transform.position).magnitude;
		float pointsExtra = pointsPerCheckpoint - pointsPerCheckpoint * (distanceToNext / pointTriggerDistance);

		if (pointsExtra > maxPointsExtra)
			maxPointsExtra = pointsExtra;
	}

	private void OnTriggerEnter(Collider other)
	{
		// If other is the right collider
		if (other.name == "ScoreTrigger"
			&& nextTrigger == other.gameObject)
		{
			pointsBase += pointsPerCheckpoint;
			
			// Sets nextTrigger
			nextTrigger = other.GetComponentInParent<NextSegment>().nextTrigger.transform.Find("ScoreTrigger").gameObject;

			// MaxPointIncrease to 0
			maxPointsExtra = 0;
		}
	}

	private void OnCollisionStay(Collision other)
	{
		if (other.transform.tag == "Terrain")
		{
			pointsBase -= crashPenalty * Time.fixedDeltaTime;
		}
	}

	public void ResetPoints (GameObject firstTrigger)
	{
		pointsBase = 0;
		maxPointsExtra = 0;

		nextTrigger = firstTrigger;
	}
}
