using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class InGameVoices : MonoBehaviour {
    private AudioSource audio;
    public AudioClip[] audioClips;
    public string[] strings;
    private Text TopText;

    private void Start()
    {
        TopText = GameObject.Find("TopCanvas").transform.GetChild(0).GetComponent<Text>();
        audio = GetComponent<AudioSource>();
    }


    public void StartTalking()
    {
        StartCoroutine(Speech());
    }

    public IEnumerator Speech()
    {
        byte i = 0;
        for (i = 0; i < audioClips.Length; i++)
        {
            audio.clip = audioClips[i];
            audio.Play();
            TopText.text = strings[i];
            yield return new WaitForSecondsRealtime(audioClips[i].length);
        }
        TopText.text = "";
    }
}
