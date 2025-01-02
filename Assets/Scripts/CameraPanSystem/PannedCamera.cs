using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PannedCamera : MonoBehaviour {

    public PanStyles panStyle;
	public Transform Follow;
    public Transform LookAt;
    public Vector3 FollowOffset;
    public Vector3 LookAtOffset;

	public enum PanStyles
	{
		None,
		SimpleFollow,
		StaticLookAt
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		switch (panStyle)
		{
			case PanStyles.StaticLookAt:
				transform.rotation = Quaternion.LookRotation(LookAt.position - transform.position + LookAtOffset, Vector3.up);
				break;
			default:
				break;
		}
	}

	public void GetFollowData(Transform pl)
	{
		if (Follow == null) Follow = pl;
		if (LookAt == null) LookAt = pl;
	}
}
