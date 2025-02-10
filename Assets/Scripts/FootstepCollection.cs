using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "New Footstep Collection", menuName = "Create New Footstep Collection")]
public class FootstepCollection : ScriptableObject
{
    public AudioClip[] footstepSounds = new AudioClip[5];
    public AudioClip LandingSound;
}
