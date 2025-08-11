using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(Ground))]
public class GroundInspector : Editor
{
    private LevelEditManager _levelEditManager;
    private bool _showSettings = false;
    public static bool DebugSegments = false;
    public override void OnInspectorGUI()
    {
        if (_levelEditManager == null)
        {
            _levelEditManager = FindFirstObjectByType<LevelEditManager>();
        }

        var ground = (Ground)target;

        GUILayout.Label("Curve Points", EditorStyles.boldLabel);
        if(GUILayout.Button("Add Curve Point", GUILayout.ExpandWidth(false)))
        {
            Selection.activeObject = _levelEditManager.InsertCurvePoint(ground, ground.CurvePoints.Count);
        }
        if (GUILayout.Button("Add Curve Point To Front", GUILayout.ExpandWidth(false)))
        {
            Selection.activeObject = _levelEditManager.InsertCurvePoint(ground, 0);
        }

        GUILayout.Space(20);
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

        GUILayout.Space(20);
        GUILayout.Label("Segments", EditorStyles.boldLabel);
        DebugSegments = GUILayout.Toggle(DebugSegments, "Debug Segments");    

        GUILayout.Space(20);
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        _showSettings = GUILayout.Toggle(_showSettings, "Show Settings");

        if (_showSettings)
        {
            DrawDefaultInspector();
        }
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
            _levelEditManager.OnUpdateTransform(ground.gameObject);
        }
    }

    public static void DrawCurvePoints(Ground ground, LevelEditManager levelEditManager)
    {
        foreach (var point in ground.CurvePointObjects)
        {
            var startPos = point.transform.position;
            if (CurvePointObjectInspector.DrawCurvePointHandles(point))
            {
                if (levelEditManager.doShiftEdits)
                {
                    levelEditManager.ShiftCurvePoints(point, point.transform.position - startPos);
                }
                levelEditManager.RefreshSerializable(ground);
            }
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