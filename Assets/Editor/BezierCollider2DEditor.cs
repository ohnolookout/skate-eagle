using UnityEngine;
using UnityEditor;

/*[CustomEditor(typeof(BezierCollider2D))]
public class BezierCollider2DEditor : Editor
{
    BezierCollider2D bezierCollider;
    EdgeCollider2D edgeCollider;

    int lastPointsQuantity = 0;
    Vector3 lastFirstPoint = Vector3.zero;
    Vector3 lastHandlerFirstPoint = Vector3.zero;
    Vector3 lastSecondPoint = Vector3.zero;
    Vector3 lastHandlerSecondPoint = Vector3.zero;

    public override void OnInspectorGUI()
    {
        bezierCollider = (BezierCollider2D)target;

        edgeCollider = bezierCollider.GetComponent<EdgeCollider2D>();

        if (edgeCollider.hideFlags != HideFlags.HideInInspector)
        {
            edgeCollider.hideFlags = HideFlags.HideInInspector;
        }

        if (edgeCollider != null)
        {
            bezierCollider.resolution = EditorGUILayout.IntField("curve points", bezierCollider.resolution, GUILayout.MinWidth(100));
            bezierCollider.firstPoint = EditorGUILayout.Vector3Field("first point", bezierCollider.firstPoint, GUILayout.MinWidth(100));
            bezierCollider.handlerFirstPoint = EditorGUILayout.Vector3Field("handler first Point", bezierCollider.handlerFirstPoint, GUILayout.MinWidth(100));
            bezierCollider.secondPoint = EditorGUILayout.Vector3Field("second point", bezierCollider.secondPoint, GUILayout.MinWidth(100));
            bezierCollider.handlerSecondPoint = EditorGUILayout.Vector3Field("handler secondPoint", bezierCollider.handlerSecondPoint, GUILayout.MinWidth(100));

            EditorUtility.SetDirty(bezierCollider);

            if (bezierCollider.resolution > 0 && !bezierCollider.firstPoint.Equals(bezierCollider.secondPoint) &&
                (
                    lastPointsQuantity != bezierCollider.resolution ||
                    lastFirstPoint != bezierCollider.firstPoint ||
                    lastHandlerFirstPoint != bezierCollider.handlerFirstPoint ||
                    lastSecondPoint != bezierCollider.secondPoint ||
                    lastHandlerSecondPoint != bezierCollider.handlerSecondPoint
                ))
            {
                lastPointsQuantity = bezierCollider.resolution;
                lastFirstPoint = bezierCollider.firstPoint;
                lastHandlerFirstPoint = bezierCollider.handlerFirstPoint;
                lastSecondPoint = bezierCollider.secondPoint;
                lastHandlerSecondPoint = bezierCollider.handlerSecondPoint;
                edgeCollider.points = bezierCollider.calculate2DPoints();
            }

        }
    }

    void OnSceneGUI()
    {
        if (bezierCollider != null)
        {
            Handles.color = Color.grey;

            Handles.DrawLine(bezierCollider.transform.position + (Vector3)bezierCollider.handlerFirstPoint, bezierCollider.transform.position + (Vector3)bezierCollider.firstPoint);
            Handles.DrawLine(bezierCollider.transform.position + (Vector3)bezierCollider.handlerSecondPoint, bezierCollider.transform.position + (Vector3)bezierCollider.secondPoint);

            bezierCollider.firstPoint = Handles.FreeMoveHandle(bezierCollider.transform.position + ((Vector3)bezierCollider.firstPoint), Quaternion.identity, 0.04f * HandleUtility.GetHandleSize(bezierCollider.transform.position + ((Vector3)bezierCollider.firstPoint)), Vector3.zero, Handles.DotHandleCap) - bezierCollider.transform.position;
            bezierCollider.secondPoint = Handles.FreeMoveHandle(bezierCollider.transform.position + ((Vector3)bezierCollider.secondPoint), Quaternion.identity, 0.04f * HandleUtility.GetHandleSize(bezierCollider.transform.position + ((Vector3)bezierCollider.secondPoint)), Vector3.zero, Handles.DotHandleCap) - bezierCollider.transform.position;
            bezierCollider.handlerFirstPoint = Handles.FreeMoveHandle(bezierCollider.transform.position + ((Vector3)bezierCollider.handlerFirstPoint), Quaternion.identity, 0.04f * HandleUtility.GetHandleSize(bezierCollider.transform.position + ((Vector3)bezierCollider.handlerFirstPoint)), Vector3.zero, Handles.DotHandleCap) - bezierCollider.transform.position;
            bezierCollider.handlerSecondPoint = Handles.FreeMoveHandle(bezierCollider.transform.position + ((Vector3)bezierCollider.handlerSecondPoint), Quaternion.identity, 0.04f * HandleUtility.GetHandleSize(bezierCollider.transform.position + ((Vector3)bezierCollider.handlerSecondPoint)), Vector3.zero, Handles.DotHandleCap) - bezierCollider.transform.position;

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }

}
*/