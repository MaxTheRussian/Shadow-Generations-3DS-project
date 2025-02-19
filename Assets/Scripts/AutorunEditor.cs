#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutorunPath))]
public class AutorunEditor : Editor {
    AutorunPath rail;

    private void OnSceneGUI()
    {
        rail = target as AutorunPath;

        if (rail.referencePoints.Length < 2)
            return;
        Handles.color = Color.red;
        for (int i = 0; i < rail.referencePoints.Length - 1; i++)
        {
            Handles.DrawLine(ShowPoint(i), ShowPoint(i + 1));
        }
    }

    private Vector3 ShowPoint(int index)
    {
        Transform handleTransform = rail.transform;

        Vector3 point = handleTransform.TransformPoint(rail.referencePoints[index]);
        EditorGUI.BeginChangeCheck();
        point = Handles.DoPositionHandle(point, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(this, "Move Point");
            EditorUtility.SetDirty(this);
            rail.referencePoints[index] = handleTransform.InverseTransformPoint(point);
        }
        return point;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
#endif
