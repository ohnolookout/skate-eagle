using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(Ground))]
public class GroundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var ground = (Ground)target;

        if(GUILayout.Button("Reset Point Targets", GUILayout.ExpandWidth(false)))
        {
            Undo.RecordObject(ground, "Clear Curve Point Targets");
            ClearCurvePointTargets(ground);
        }
    }
    public void OnSceneGUI()
    {
        var ground = (Ground)target;

        foreach (var point in ground.CurvePointObjects)
        {
            CurvePointObjectInspector.DrawCurvePointHandles(point);
        }

        if (ground.gameObject.transform.hasChanged)
        {
            Debug.Log("Ground transform has changed, updating targets...");
            ground.gameObject.transform.hasChanged = false;
            var editManager = FindFirstObjectByType<LevelEditManager>();
            editManager.OnUpdateTransform();

        }
    }


    private static void ClearCurvePointTargets(Ground ground)
    {
        foreach(var curvePointObj in ground.CurvePointObjects)
        {
            curvePointObj.RightTargetObjects = new();
            curvePointObj.LeftTargetObjects = new();
            curvePointObj.LinkedCameraTarget.LeftTargets = new();
            curvePointObj.LinkedCameraTarget.RightTargets = new();
        }
    }
}