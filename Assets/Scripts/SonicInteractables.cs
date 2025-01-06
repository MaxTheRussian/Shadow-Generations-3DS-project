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
    public float detail = 120;


# if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        switch (ObjectType)
        {
            case Type.DashPad:
                Debug.DrawLine(transform.position, transform.position + transform.forward * Power * LockTime, Color.red);
                break;
            case Type.Spring:
                
                Vector3[] HoverPoints = GetPoints();
                //Debug.Log(HoverPoints[0] + " " +HoverPoints[60]);
                for (int i = 0; i < HoverPoints.Length - 1; i++)
                    Debug.DrawLine(HoverPoints[i], HoverPoints[i+1], Color.green);
                break;
        }
    }

    Vector3[] GetPoints()
    {
        Vector3[] points = new Vector3[(int)(LockTime / detail)];
        Vector3 InitialSpeed = transform.transform.up * Power;
        float time = 0;
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Vector3(InitialSpeed.x * time, InitialSpeed.y * time - 0.375f * time * time, InitialSpeed.z * time);
            points[i] += transform.position;
            time += .02f;
        }
        return points;
    }

#endif
    }
