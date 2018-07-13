using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScale : MonoBehaviour {

	[Range(0.01f, 100f)] public float timeScale = 1;

	private void Start()
	{
		Time.timeScale = timeScale;
	}
}
