using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.N3DS;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour {


	void Update () {
		if (GamePad.GetButtonTrigger(N3dsButton.Start) || Input.GetKeyDown(KeyCode.Space))
			SceneManager.LoadScene(1);
	}
}
