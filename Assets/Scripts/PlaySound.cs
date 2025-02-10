using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour {

    AudioSource ShadowActionSounds;
    public AudioClip[] actionSounds;
    [Header("Footsteps")]
    public AudioClip[] footsteps;
    public AudioClip landingSound;

    void Start()
    {
        ShadowActionSounds = transform.parent.GetComponent<AudioSource>();
    }

    public void PlayActionSound(int soundID)
    {
        ShadowActionSounds.PlayOneShot(actionSounds[soundID]);
    }

    public void PlayFootstepSound()
    {
        ShadowActionSounds.PlayOneShot(footsteps[Time.frameCount % 5]);
    }

    public void PlayLandSound()
    {
        ShadowActionSounds.PlayOneShot(landingSound);
    }

    public void ChangeCollection(FootstepCollection collection)
    {
        for (int i = 0; i < collection.footstepSounds.Length; i++)
            footsteps[i] = collection.footstepSounds[i];
        landingSound = collection.LandingSound;
    }
}
