using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class Rail : MonoBehaviour {
    public int i;
    [HideInInspector] public Vector3 position;
    public float ColDist;
	public List<Vector3> points;
    [SerializeField] Vector3[] vertex;
    [SerializeField] List<int> trianges;

    void Start()
    {
        position = transform.position;
    }

    public void AddPoint()
    {
        points.Add(points[points.Count-1] + (points[points.Count - 1] - points[points.Count - 2]).normalized * 10f);
    }
# if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + points[i], .5f);
        }
    }
#endif
    public void DrawMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        mesh.name = "generatedRail" + gameObject.name;

        vertex = new Vector3[2 * points.Count];
        int i = 0;
        int j = 0;
        for (;i < points.Count; i++)
        {
            vertex[j++] = points[i] + Vector3.left * 0.2f;
            vertex[j++] = points[i] + Vector3.right * 0.2f;
        }
        
        mesh.vertices = vertex;

        trianges = new List<int>();
        j = 0;
        for (i = 0; i < vertex.Length - 2; i+=2)
        {
            trianges.Add(i + 3);
            trianges.Add(i + 1);
            trianges.Add(i + 0);

            trianges.Add(i + 2);
            trianges.Add(i + 3);
            trianges.Add(i + 0);
        }

        // Should Do UVs here

        mesh.triangles = trianges.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
