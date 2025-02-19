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
        AutoRunSection,
        jumpPanel,
        UpReel,
    }

    public float LockTime;
    public float Power;
    public bool KeepVelocity;
    public bool isSpecial;


#if UNITY_EDITOR
    public int PointsAmount = 90;

    public void OnDrawGizmos()
    {
        switch (ObjectType)
        {
            case Type.DashPad:
                Debug.DrawLine(transform.position, transform.position + transform.forward * Power * LockTime, Color.red);
                break;
            case Type.Spring:
                DrawParabola(transform.up);
                break;
            case Type.DashRing:
                DrawParabola(transform.forward);
                break;
            case Type.jumpPanel:
                DrawParabola(-transform.forward);
                break;

        }
    }

    private void DrawParabola(Vector3 Direction)
    {
        Vector3 speed = Direction * Power;
        Vector3 prevPoint = transform.position;
        Vector3 nextPoint = transform.position;
        float time = 0f;
        for (int i = 1; i < PointsAmount; i++)
        {
            time += 0.02f;
            nextPoint = transform.position + speed * time + Vector3.down * .25f * time * time / 0.02f;
            Debug.DrawLine(prevPoint, nextPoint, time < LockTime ? Color.green : Color.red, 0f, true);
            prevPoint = nextPoint;
        }
    }

#endif
    }
