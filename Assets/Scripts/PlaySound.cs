using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour {

    AudioSource ShadowActionSounds;
    public AudioClip[] actionSounds;

    void Start()
    {
        ShadowActionSounds = transform.parent.GetComponent<AudioSource>();
    }

    public void PlayActionSound(int soundID)
    {
        ShadowActionSounds.PlayOneShot(actionSounds[soundID]);
    }
}
