using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(Ground))]
public class GroundInspector : Editor
{
    private LevelEditManager _levelEditManager;
    public static bool DebugSegments = false;
    public override void OnInspectorGUI()
    {
        if(_levelEditManager == null)
        {
            _levelEditManager = FindFirstObjectByType<LevelEditManager>();
        }

        var ground = (Ground)target;

        GUILayout.Label("Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        ground.IsInverted = GUILayout.Toggle(ground.IsInverted, "Inverted");
        ground.IsFloating = GUILayout.Toggle(ground.IsFloating, "Floating");
        ground.HasShadow = GUILayout.Toggle(ground.HasShadow, "Shadow");

        if (EditorGUI.EndChangeCheck())
        {
            _levelEditManager.RefreshSerializable(ground);
        }

        GUILayout.Space(20);
        GUILayout.Label("Targeting", EditorStyles.boldLabel);


        if (GUILayout.Button("Reset Point Targets", GUILayout.ExpandWidth(false)))
        {
            Undo.RecordObject(ground, "Clear Curve Point Targets");
            ClearCurvePointTargets(ground, _levelEditManager);
        }

        if (GUILayout.Button("Populate Default Targets", GUILayout.ExpandWidth(false)))
        {
            Undo.RecordObject(ground, "Clear Curve Point Targets");
            PopulateDefaultTargets(ground, _levelEditManager);
        }

        GUILayout.Label("Segments", EditorStyles.boldLabel);
        GUILayout.Space(20);
        DebugSegments = GUILayout.Toggle(DebugSegments, "Debug Segments");
    }
    public void OnSceneGUI()
    {
        if (_levelEditManager == null)
        {
            _levelEditManager = FindFirstObjectByType<LevelEditManager>();
        }

        var ground = (Ground)target;
        DrawCurvePoints(ground, _levelEditManager);

        if (ground.gameObject.transform.hasChanged)
        {
            ground.gameObject.transform.hasChanged = false;
            _levelEditManager.OnUpdateTransform();
        }
    }

    public static void DrawCurvePoints(Ground ground, LevelEditManager levelEditManager)
    {
        var curvePointChanged = false;
        foreach (var point in ground.CurvePointObjects)
        {
            if (CurvePointObjectInspector.DrawCurvePointHandles(point))
            {
                curvePointChanged = true;
            }
        }

        if (curvePointChanged)
        {
            levelEditManager.RefreshSerializable(ground);
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