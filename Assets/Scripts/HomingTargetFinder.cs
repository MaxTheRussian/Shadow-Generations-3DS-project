using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingTargetFinder : MonoBehaviour {

    [SerializeField] ShadowController HomingTargetTo;

    void Start()
    {
        //HomingTargetTo = GameObject.FindGameObjectWithTag("Player").GetComponent<ShadowController>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (Vector3.Distance(HomingTargetTo.HomingTarget.position, HomingTargetTo.transform.position) > Vector3.Distance(HomingTargetTo.HomingTarget.position, collider.transform.position))
            HomingTargetTo.HomingTarget = collider.transform;
    }
}