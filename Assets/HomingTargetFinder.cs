using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingTargetFinder : MonoBehaviour {

    ShadowController HomingTargetTo;

    void Start()
    {
        HomingTargetTo = GameObject.FindGameObjectWithTag("Player").GetComponent<ShadowController>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Homable"))
        {
            HomingTargetTo.HomingTarget = collider.transform;
        }
    }
}
