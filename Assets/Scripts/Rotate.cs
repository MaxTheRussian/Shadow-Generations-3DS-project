using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

	[SerializeField] float speed;
	
	void Update () {
		transform.Rotate(speed * Time.deltaTime, 0f, 0f, Space.Self);
	}
}
