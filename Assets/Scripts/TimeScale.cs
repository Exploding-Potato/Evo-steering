using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeScale : MonoBehaviour {

	private float timeScale;
	[SerializeField] private GameObject textGO;
	[SerializeField] private GameObject sliderGO;
	private Text text;

	private void Start()
	{
		text = textGO.GetComponent<Text>();
		
		Scale = sliderGO.GetComponent<Slider>().value;
	}

	public float Scale
	{
		get { return timeScale; }
		set
		{
			timeScale = value;

			Time.timeScale = timeScale;
			text.text = $"Timescale: {timeScale}";
		}
	}
}
