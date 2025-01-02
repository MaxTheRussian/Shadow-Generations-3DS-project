using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public Text FpsText;
    float polliingTime = 1f;
    float time;
    int frameCount;

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        frameCount++;
        if(time >= polliingTime)
        {
            int framerate = Mathf.RoundToInt(frameCount / time);
            FpsText.text = framerate.ToString() + " FPS";

            time -= polliingTime;
            frameCount = 0;
        }
    }
}
