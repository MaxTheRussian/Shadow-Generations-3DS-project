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

	
	public void Start () 
	{
		Transform ringsCollection = GameObject.Find("Rings").transform;
        SecondsPassed = 0;
        StartCoroutine(UpdateTime());
        for (int i = 0; i < ringsCollection.childCount; i++)
			ringsCollection.GetChild(i).gameObject.SetActive(true);

	}
	
	IEnumerator UpdateTime()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			SecondsPassed += Time.deltaTime;
			SecondTick.Rotate(0, 0, -360 * Time.deltaTime);
			HourTick.Rotate(0, 0, -6 * Time.deltaTime);
			Timer.text = SecondsPassed.ToString();
		}
	}

	public void StopTimer()
	{
		StopCoroutine(UpdateTime());
	}

	// Update is called once per frame
	public void UpdateBoost(float val) 
	{
		BoostSlider.value = val;	
	}

    public void UpdateRings(uint val)
    {
        Rings.text = val.ToString();
    }
}
