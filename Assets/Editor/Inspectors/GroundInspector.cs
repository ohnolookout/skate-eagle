using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(Ground))]
public class GroundInspector : Editor
{
    private EditManager _levelEditManager;
    private bool _showSettings = false;
    public static bool DebugSegments = false;
    private bool _controlHeld = false;
    public override void OnInspectorGUI()
    {
        if (_levelEditManager == null)
        {
            _levelEditManager = FindFirstObjectByType<EditManager>();
        }

        var ground = (Ground)target;
        var defaultColor = GUI.backgroundColor;

        GUILayout.Label("Curve Points", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.skyBlue;
        if (GUILayout.Button("Add To Back", GUILayout.ExpandWidth(true)))
        {
            Selection.activeObject = _levelEditManager.InsertCurvePoint(ground, ground.CurvePoints.Count);
        }
        if (GUILayout.Button("Add To Front", GUILayout.ExpandWidth(true)))
        {
            Selection.activeObject = _levelEditManager.InsertCurvePoint(ground, 0);
        }
        GUI.backgroundColor = defaultColor;

        GUILayout.EndHorizontal();

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

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Populate Defaults", GUILayout.ExpandWidth(true)))
        {
            Undo.RecordObject(ground, "Populate default targets");
            PopulateDefaultTargets(ground, _levelEditManager);
        }

        GUI.backgroundColor = Color.orangeRed;
        if (GUILayout.Button("Clear", GUILayout.ExpandWidth(true)))
        {
            Undo.RecordObject(ground, "Clear Curve Point Targets");
            ClearCurvePointTargets(ground, _levelEditManager);
        }
        GUI.backgroundColor = defaultColor;

        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("View Options", EditorStyles.boldLabel);
        _showSettings = GUILayout.Toggle(_showSettings, "Show Default Inspector");
        DebugSegments = GUILayout.Toggle(DebugSegments, "Show Segment Inspector");

        if (_showSettings)
        {
            DrawDefaultInspector();
        }
    }
    public void OnSceneGUI()
    {
        if (Event.current.control)
        {
            _controlHeld = true;
        }
        else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftControl)
        {
            _controlHeld = false;
        }

        if (_levelEditManager == null)
        {
            _levelEditManager = FindFirstObjectByType<EditManager>();
        }

        var ground = (Ground)target;

        if(ground.lastCPObjCount != ground.curvePointContainer.transform.childCount)
        {
            ground.lastCPObjCount = ground.curvePointContainer.transform.childCount;
            _levelEditManager.RefreshSerializable(ground);
        }

        DrawCurvePoints(ground, _levelEditManager, _controlHeld);

        if (ground.gameObject.transform.hasChanged)
        {
            ground.gameObject.transform.hasChanged = false;
            _levelEditManager.OnUpdateTransform(ground.gameObject);
        }
    }

    public static void DrawCurvePoints(Ground ground, EditManager editManager, bool controlHeld)
    {
        foreach (var point in ground.CurvePointObjects)
        {
            var startPos = point.transform.position;
            if (CurvePointObjectInspector.DrawCurvePointHandles(point))
            {
                if (editManager.editType == EditType.Shift || controlHeld)
                {
                    editManager.ShiftCurvePoints(point, point.transform.position - startPos);
                }
                editManager.RefreshSerializable(ground);
            }
        }
    }

    private static void ClearCurvePointTargets(Ground ground, EditManager editManager)
    {
        foreach(var curvePointObj in ground.CurvePointObjects)
        {
            curvePointObj.RightTargetObjects = new();
            curvePointObj.LeftTargetObjects = new();
            curvePointObj.LinkedCameraTarget.LeftTargets = new();
            curvePointObj.LinkedCameraTarget.RightTargets = new();
        }

        if (editManager != null)
        {
            editManager.UpdateEditorLevel();
        }
    }

    public static void PopulateDefaultTargets(Ground ground, EditManager editManager)
    {
        foreach(var point in ground.CurvePointObjects)
        {
            point.PopulateDefaultTargets();
        }
        if (editManager != null)
        {
            editManager.UpdateEditorLevel();
        }
    }
}