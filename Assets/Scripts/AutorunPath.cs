using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutorunPath : MonoBehaviour {

	public Vector3[] referencePoints;
	public Vector3 GizmosTestPoint;
	[SerializeField] bool done;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Vector3 GetClosestPoint(Vector3 point, float epsilon = 3f)
	{ int i = 0, j = 0; Vector3 returnPoint = Vector3.zero;
		float currRecordedAngle = 1f, lastrecorded = 0, t = 0f;
        for (; i < referencePoints.Length; i++) //GetClosest j
        {
			Debug.Log(SquareDist(referencePoints[i], point) + " " + i);

            if (SquareDist(referencePoints[i], point) < SquareDist(referencePoints[j], point))
				j = i;
        }




        return referencePoints[j] + transform.position;
	}

	float SquareDist(Vector3 a, Vector3 b)
	{
		return (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y) + (b.z - a.z) * (b.z - a.z);
	}

	private void OnDrawGizmosSelected()
	{
		//if (done) return;
		done = true;
		Debug.DrawLine(GizmosTestPoint, GetClosestPoint(GizmosTestPoint), Color.blue);
	}
}
