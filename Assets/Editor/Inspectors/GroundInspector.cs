using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(Ground))]
public class GroundInspector : Editor
{
    private LevelEditManager _levelEditManager;
    public override void OnInspectorGUI()
    {
        if(_levelEditManager == null)
        {
            _levelEditManager = FindFirstObjectByType<LevelEditManager>();
        }

        var ground = (Ground)target;

        EditorGUI.BeginChangeCheck();

        ground.IsInverted = GUILayout.Toggle(ground.IsInverted, "Inverted");
        ground.IsFloating = GUILayout.Toggle(ground.IsFloating, "Floating");
        ground.HasShadow = GUILayout.Toggle(ground.HasShadow, "Shadow");

        if (EditorGUI.EndChangeCheck())
        {
            _levelEditManager.RefreshSerializable(ground);
        }

        if(GUILayout.Button("Reset Point Targets", GUILayout.ExpandWidth(false)))
        {
            Undo.RecordObject(ground, "Clear Curve Point Targets");
            ClearCurvePointTargets(ground, _levelEditManager);
        }

        if (GUILayout.Button("Populate Default Targets", GUILayout.ExpandWidth(false)))
        {
            Undo.RecordObject(ground, "Clear Curve Point Targets");
            PopulateDefaultTargets(ground, _levelEditManager);
        }
    }
    public void OnSceneGUI()
    {
        var ground = (Ground)target;
        var curvePointChanged = false;

        foreach (var point in ground.CurvePointObjects)
        {
            if (CurvePointObjectInspector.DrawCurvePointHandles(point))
            {
                curvePointChanged = true;
            }
        }

        if (ground.gameObject.transform.hasChanged)
        {
            ground.gameObject.transform.hasChanged = false;
            _levelEditManager.OnUpdateTransform();
        }

        if (curvePointChanged)
        {
            _levelEditManager.RefreshSerializable(ground);
        }
    }


    private static void ClearCurvePointTargets(Ground ground, LevelEditManager levelEditManager)
    {
        foreach(var curvePointObj in ground.CurvePointObjects)
        {
            curvePointObj.RightTargetObjects = new();
            curvePointObj.LeftTargetObjects = new();
            curvePointObj.LinkedCameraTarget.LeftTargets = new();
            curvePointObj.LinkedCameraTarget.RightTargets = new();
        }

        if (levelEditManager != null)
        {
            levelEditManager.UpdateEditorLevel();
        }
    }

    public static void PopulateDefaultTargets(Ground ground, LevelEditManager levelEditManager)
    {
        foreach(var point in ground.CurvePointObjects)
        {
            point.PopulateDefaultTargets();
        }
        if (levelEditManager != null)
        {
            levelEditManager.UpdateEditorLevel();
        }
    }
}