using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingSpinAnimation : MonoBehaviour {


    void Update()
    {
        transform.Rotate(0f, Time.deltaTime * 180f, 0f);
    }

}
