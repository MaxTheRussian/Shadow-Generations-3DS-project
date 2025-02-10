#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Rail))]
public class RailEditor : Editor {
    Rail rail;

    private void OnSceneGUI()
    {
        rail = target as Rail;

        if (rail.points.Count < 2)
            return;
        Handles.color = Color.red;
        for (int i = 0; i < rail.points.Count - 1; i++)
        {
            Handles.DrawLine(ShowPoint(i), ShowPoint(i + 1));
        }
        DrawRail();
    }

    private Vector3 ShowPoint(int index)
    {
        Transform handleTransform = rail.transform;

        Vector3 point = handleTransform.TransformPoint(rail.points[index]);
        EditorGUI.BeginChangeCheck();
        point = Handles.DoPositionHandle(point, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(this, "Move Point");
            EditorUtility.SetDirty(this);
            rail.points[index] = handleTransform.InverseTransformPoint(point);
            rail.DrawMesh();
        }
        return point;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        rail = target as Rail;
        if (GUILayout.Button("Add Point"))
        {
            Undo.RecordObject(rail, "Add Curve");
            rail.AddPoint();
            EditorUtility.SetDirty(rail);
        }
    }

    private void AddPoint()
    {
        rail.points.Add(rail.points[rail.points.Count - 1] + Vector3.one);
    }

    private void DrawRail()
    {
        Mesh mesh = new Mesh();

    }
}
#endif
