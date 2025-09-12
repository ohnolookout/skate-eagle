using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(Ground))]
public class GroundInspector : Editor
{
    private EditManager _editManager;
    private bool _showSettings = false;
    public static bool DebugSegments = false;
    private bool _controlHeld = false;
    private bool _altHeld = false;
    private bool _aHeld = false;
    private bool _showCPTransform = true;
    public override void OnInspectorGUI()
    {
        if (_editManager == null)
        {
            _editManager = FindFirstObjectByType<EditManager>();
        }

        var ground = (Ground)target;
        var defaultColor = GUI.backgroundColor;

        GUILayout.Label("Curve Points", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.skyBlue;
        if (GUILayout.Button("Add Before", GUILayout.ExpandWidth(true)))
        {
            _editManager.InsertCurvePoint(ground, 0);
        }
        if (GUILayout.Button("Add After", GUILayout.ExpandWidth(true)))
        {
            _editManager.InsertCurvePoint(ground, ground.CurvePoints.Count);
        }
        GUI.backgroundColor = defaultColor;

        GUILayout.EndHorizontal();

        if (ground.CurvePointObjects.Length > 0)
        {
            GUILayout.BeginHorizontal();

            GUI.backgroundColor = Color.orangeRed;
            if (GUILayout.Button("Remove Before", GUILayout.ExpandWidth(true)))
            {
                _editManager.RemoveCurvePoint(ground.CurvePointObjects[0], false);
            }
            if (GUILayout.Button("Remove After", GUILayout.ExpandWidth(true)))
            {
                _editManager.RemoveCurvePoint(ground.CurvePointObjects[^1], false);
            }
            GUI.backgroundColor = defaultColor;

            GUILayout.EndHorizontal();
        }
        GUILayout.Space(10);
        GUILayout.Label("Floor", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();

        var floorType = (FloorType)EditorGUILayout.EnumPopup("Floor Type", ground.FloorType);
        if(EditorGUI.EndChangeCheck())
        {
            if(!EditorUtility.DisplayDialog("Change Floor Type", "Changing the floor type may reset some curve point floor settings. Are you sure you want to continue?", "Yes", "No"))
            {
                return;
            }
            Undo.RecordObject(ground, "Change Floor Type");
            ground.FloorType = floorType;

            //Update floor points based on type
            ground.CurvePoints[0].FloorPointType = FloorPointType.Set;
            ground.CurvePoints[^1].FloorPointType = FloorPointType.Set;

            if (floorType != FloorType.Segmented)
            {
                for(int i = 1; i < ground.CurvePoints.Count - 1; i++)
                {
                    ground.CurvePoints[i].FloorPointType = FloorPointType.None;
                }
            } else
            {
                if (ground.CurvePoints[0].FloorHeight == 0)
                {
                    ground.CurvePoints[0].FloorHeight = ground.StartFloorHeight;
                    ground.CurvePoints[0].FloorAngle = ground.StartFloorAngle;
                }

                if (ground.CurvePoints[^1].FloorHeight == 0)
                {
                    ground.CurvePoints[^1].FloorHeight = ground.EndFloorHeight;
                    ground.CurvePoints[^1].FloorAngle = ground.EndFloorAngle;
                }
            }

            _editManager.RefreshSerializable(ground);
        }

        EditorGUI.BeginChangeCheck();
        int startFloorHeight = ground.StartFloorHeight;
        int startFloorAngle = ground.StartFloorAngle;
        int endFloorHeight = ground.EndFloorHeight;
        int endFloorAngle = ground.EndFloorAngle;

        if (ground.FloorType == FloorType.Flat)
        {
            startFloorHeight = EditorGUILayout.IntField("Floor Height", startFloorHeight);
        }

        if(ground.FloorType == FloorType.Slanted)
        {
            startFloorHeight = EditorGUILayout.IntField("Start Floor Height", startFloorHeight);
            startFloorAngle = EditorGUILayout.IntField("Start Floor Angle", startFloorAngle);
            endFloorHeight = EditorGUILayout.IntField("Start Floor Height", endFloorHeight);
            endFloorAngle = EditorGUILayout.IntField("Start Floor Angle", endFloorAngle);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(ground, "Change Floor Settings");
            ground.StartFloorHeight = startFloorHeight;
            ground.StartFloorAngle = startFloorAngle;
            ground.EndFloorHeight = endFloorHeight;
            ground.EndFloorAngle = endFloorAngle;
            _editManager.RefreshSerializable(ground);
        }


        GUILayout.Space(10);
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        ground.IsInverted = GUILayout.Toggle(ground.IsInverted, "Inverted");
        ground.HasShadow = GUILayout.Toggle(ground.HasShadow, "Shadow");

        if (EditorGUI.EndChangeCheck())
        {
            _editManager.RefreshSerializable(ground);
        }

        GUILayout.Space(10);
        GUILayout.Label("Targeting", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Populate Defaults", GUILayout.ExpandWidth(true)))
        {
            Undo.RecordObject(ground, "Populate default targets");
            PopulateDefaultTargets(ground, _editManager);
        }

        GUI.backgroundColor = Color.orangeRed;
        if (GUILayout.Button("Clear", GUILayout.ExpandWidth(true)))
        {
            Undo.RecordObject(ground, "Clear Curve Point Targets");
            ClearCurvePointTargets(ground, _editManager);
        }
        GUI.backgroundColor = defaultColor;

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Add Start", GUILayout.ExpandWidth(false)))
        {
            _editManager.AddStart(ground);
        }
        if (GUILayout.Button("Add Finish", GUILayout.ExpandWidth(false)))
        {
            _editManager.AddFinish(ground);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("View Options", EditorStyles.boldLabel);
        _showCPTransform = GUILayout.Toggle(_showCPTransform, "Show CP Transforms");
        _showSettings = GUILayout.Toggle(_showSettings, "Show Default Inspector");
        DebugSegments = GUILayout.Toggle(DebugSegments, "Allow Segment Selection");

        if (_showSettings)
        {
            DrawDefaultInspector();
        }
    }
    public void OnSceneGUI()
    {
        //Check key held bools
        if (Event.current.control)
        {
            _controlHeld = true;
        }
        else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftControl)
        {
            _controlHeld = false;
        }

        if (Event.current.alt)
        {
            _altHeld = true;
        }
        else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftAlt)
        {
            _altHeld = false;
        }

        if(Event.current.keyCode == KeyCode.A && Event.current.type == EventType.KeyDown)
        {
            _aHeld = true;
        } else if(Event.current.keyCode == KeyCode.A && Event.current.type == EventType.KeyUp)
        {
            _aHeld = false;
        }

        if (_editManager == null)
        {
            _editManager = FindFirstObjectByType<EditManager>();
        }

        var ground = (Ground)target;

        if(!Application.isPlaying && ground.lastCPObjCount != ground.curvePointContainer.transform.childCount)
        {
            ground.lastCPObjCount = ground.curvePointContainer.transform.childCount;
            _editManager.RefreshSerializable(ground);
        }


        if (_showCPTransform && !_aHeld)
        {
            DrawCurvePoints(ground, _editManager, _controlHeld, _altHeld);
        }

        if (ground.gameObject.transform.hasChanged)
        {
            ground.gameObject.transform.hasChanged = false;
            _editManager.OnUpdateTransform(ground.gameObject);
        }

        if (_aHeld)
        {
            DrawSelectAndDoTargetButtons(ground);
        }

    }

    public static void DrawCurvePoints(Ground ground, EditManager editManager, bool controlHeld, bool altHeld)
    {
        foreach (var point in ground.CurvePointObjects)
        {
            var startPos = point.transform.position;
            if (CurvePointObjectInspector.DrawCurvePointHandles(point,  altHeld))
            {
                if (editManager.editType == EditType.Shift || controlHeld)
                {
                    editManager.ShiftCurvePoints(point, point.transform.position - startPos);
                }
                editManager.RefreshSerializable(ground);
            }
        }
    }

    public static void DrawCamTargetOptions(Ground ground, CurvePointEditObject currentCPObject)
    {
        if(currentCPObject == null || !currentCPObject.DoTargetLow)
        {
            return;
        }

        foreach(var cpObj in ground.CurvePointObjects)
        {
            if(cpObj == currentCPObject)
            {
                continue;
            }

            CurvePointObjectInspector.DrawCamTargetButtons(currentCPObject, cpObj);
        }
    }

    public static void DrawSelectAndDoTargetButtons(Ground ground)
    {
        foreach (var cpObj in ground.CurvePointObjects)
        {
            CurvePointObjectInspector.DrawSelectAndDoTargetButtons(cpObj);
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