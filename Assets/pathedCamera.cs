using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pathedCamera : MonoBehaviour {

	public AutorunPath AutorunPath;
	public Transform PlayerToFollow;
	public float backwardsOffset = 5;

	public void OnEnable(Transform Player)
	{
		PlayerToFollow = Player;
	}

	// Update is called once per frame
	void FixedUpdate() 
	{
		
	}
}
