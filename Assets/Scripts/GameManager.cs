using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public Slider BoostSlider;
	public Text Rings;
	public Text Timer;
	public RectTransform SecondTick;
	public RectTransform HourTick;
	public float SecondsPassed;

	
	void Start () {
			
	}
	
	void Update()
	{
		SecondsPassed += Time.deltaTime;
        SecondTick.Rotate(0, 0, -360 * Time.deltaTime);
		HourTick.Rotate(0, 0, -6 * Time.deltaTime);
		Timer.text = SecondsPassed.ToString();

	}

	// Update is called once per frame
	public void UpdateBoost(float val) 
	{
		BoostSlider.value = val;	
	}
}
