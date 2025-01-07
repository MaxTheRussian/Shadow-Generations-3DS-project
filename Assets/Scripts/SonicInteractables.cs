using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonicInteractables : MonoBehaviour {

    public Type ObjectType;
    public enum Type
    {
        DashPad,
        Spring,
        DashRing,
        BoostCapsule_VibOnly,
        GravityPlatform,

    }

    public float LockTime;
    public float Power;
    public bool KeepVelocity;
    public bool isSpecial;


# if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        switch (ObjectType)
        {
            case Type.DashPad:
                Debug.DrawLine(transform.position, transform.position + transform.forward * Power * LockTime, Color.red);
                break;
            case Type.Spring:
                Vector3 speed = transform.up * Power;
                Vector3[] HoverPoints = new Vector3[45];
                HoverPoints[0] = transform.position;
                for (int i = 1; i < HoverPoints.Length; i++)
                {
                    float time = (float)i / (float)HoverPoints.Length;
                    HoverPoints[i] = transform.position + speed * time + Vector3.down * .25f * time * time / 0.02f;
                    Debug.DrawLine(HoverPoints[i - 1], HoverPoints[i], Color.green, 0f, true);
                }

                break;
        }
    }



#endif
    }
