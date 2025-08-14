using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

[CustomEditor(typeof(CurvePointEditObject))]
public class CurvePointObjectInspector : Editor
{
    private CurvePointEditObject _curvePointObject;
    private SerializedObject _so;
    private SerializedProperty _serializedLeftTargetObjects;
    private SerializedProperty _serializedRightTargetObjects;
    private FindAdjacentCurvePointWindow _findAdjacentCurvePointWindow;
    private GroundManager _groundManager;
    private EditManager _editManager;
    private bool _showTargetObjects = false;
    private bool _showFloorOptions = false;
    public override void OnInspectorGUI()
    {
        _curvePointObject = (CurvePointEditObject)target;
        _so = new SerializedObject(_curvePointObject);
        _serializedLeftTargetObjects = _so.FindProperty("leftTargetObjects");
        _serializedRightTargetObjects = _so.FindProperty("rightTargetObjects");
        var originalColor = GUI.backgroundColor;

        if (_groundManager == null)
        {
            _groundManager = FindFirstObjectByType<GroundManager>();
        }

        if (_editManager == null)
        {
            _editManager = FindFirstObjectByType<EditManager>();
        }


        GUILayout.Label("Tangent Options", EditorStyles.boldLabel);
        //Curvepoint settings
        EditorGUI.BeginChangeCheck();

        var currentMode = (int)_curvePointObject.CurvePoint.Mode;
        var mode = GUILayout.Toolbar(currentMode, Enum.GetNames(typeof(ShapeTangentMode)));
        var isSymmetrical = GUILayout.Toggle(_curvePointObject.CurvePoint.IsSymmetrical, "Symmetrical");

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_curvePointObject, "Curve Point Settings");
            _curvePointObject.TangentSettingsChanged((ShapeTangentMode)mode, isSymmetrical);
            RefreshGround();
        }

        GUILayout.Space(10);

        GUI.backgroundColor = Color.orangeRed;
        if (GUILayout.Button("Reset Tangents", GUILayout.ExpandWidth(false)))
        {
            Undo.RecordObject(_curvePointObject, "Reset Curve Point Tangents");
            _curvePointObject.TangentsChanged(
                _curvePointObject.CurvePoint.Position + new Vector3(-1, 0),
                _curvePointObject.CurvePoint.Position + new Vector3(-1, 0));

            RefreshGround();
        }
        GUI.backgroundColor = originalColor;

        GUILayout.Space(20);

        GUILayout.Label("Add/Remove", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.skyBlue;
        if (GUILayout.Button("Add After", GUILayout.ExpandWidth(true)))
        {
            var ground = _curvePointObject.ParentGround;
            var index = _curvePointObject.transform.GetSiblingIndex() + 1;

            Selection.activeObject = _editManager.InsertCurvePoint(ground, index);
        }

        if (GUILayout.Button("Add Before", GUILayout.ExpandWidth(true)))
        {
            var ground = _curvePointObject.ParentGround;
            var index = _curvePointObject.transform.GetSiblingIndex() - 1;

            Selection.activeObject = _editManager.InsertCurvePoint(ground, index);
        }

        GUI.backgroundColor = Color.orangeRed;
        if (GUILayout.Button("Remove", GUILayout.ExpandWidth(true)))
        {
            var ground = _curvePointObject.ParentGround;
            //Undo.RegisterFullObjectHierarchyUndo(ground, "Curve point destroyed.");
            Undo.DestroyObjectImmediate(_curvePointObject.gameObject);
            Selection.activeGameObject = ground.gameObject;
            return;
        }
        GUI.backgroundColor = originalColor;

        EditorGUILayout.EndHorizontal();


        GUILayout.Space(20);

        GUILayout.Label("Targeting", EditorStyles.boldLabel);
        //Camera targetting
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();

        var doTargetLow = GUILayout.Toggle(_curvePointObject.DoTargetLow, "Target Low", GUILayout.ExpandWidth(false));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Target Settings");
            _curvePointObject.DoTargetLow = doTargetLow;
            _curvePointObject.GenerateTarget();
        }

        EditorGUI.BeginChangeCheck();

        var doTargetHigh = GUILayout.Toggle(_curvePointObject.DoTargetHigh, "Target High", GUILayout.ExpandWidth(false));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Target Settings");
            _curvePointObject.DoTargetHigh = doTargetHigh;
            _curvePointObject.GenerateTarget();
        }

        EditorGUILayout.EndHorizontal();

        if (_curvePointObject.DoTargetLow || _curvePointObject.DoTargetHigh)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Populate Defaults", GUILayout.ExpandWidth(true)))
            {
                Undo.RecordObject(_curvePointObject, "Reseting targets to default");
                _curvePointObject.PopulateDefaultTargets();
                _editManager.UpdateEditorLevel();
            }
            if (GUILayout.Button("Find Next", GUILayout.ExpandWidth(true)))
            {
                if (!_curvePointObject.DoTargetLow)
                {
                    Debug.LogWarning("Curve point not set as camera target. Cannot find adjacent curve points.");
                    return;
                }
                _findAdjacentCurvePointWindow = EditorWindow.GetWindow<FindAdjacentCurvePointWindow>();
                _findAdjacentCurvePointWindow.Init(_curvePointObject);
            }

            EditorGUILayout.EndHorizontal();

            _showTargetObjects = EditorGUILayout.Foldout(_showTargetObjects, "Target Objects");
            if (_showTargetObjects)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(_serializedLeftTargetObjects, true);
                EditorGUILayout.PropertyField(_serializedRightTargetObjects, true);

                if (EditorGUI.EndChangeCheck())
                {
                    _so.ApplyModifiedProperties();
                    _so.Update();
                }
            }
        }


        GUILayout.Space(20);

        GUILayout.Label("Set Start/Finish", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.lightGreen;
        if (GUILayout.Button("Start", GUILayout.ExpandWidth(true)))
        {
            Undo.RecordObject(_groundManager.StartLine, "Set Start Point");
            _groundManager.StartLine.SetStartLine(_curvePointObject.CurvePoint);
        }

        GUI.backgroundColor = Color.orange;
        if (GUILayout.Button("Finish", GUILayout.ExpandWidth(true)))
        {
            Undo.RecordObject(_groundManager.FinishLine, "Set Finish Flag and Backstop");
            _groundManager.FinishLine.SetFlagPoint(_curvePointObject.CurvePoint);

            var nextCurvePoint = NextCurvePoint(_curvePointObject);
            if (nextCurvePoint != null)
            {
                _groundManager.FinishLine.SetBackstopPoint(nextCurvePoint.CurvePoint);
            }
            else
            {
                Debug.LogWarning("No next curve point found to set as backstop.");
            }
        }

        GUI.backgroundColor = Color.softYellow;
        if (GUILayout.Button("Flag", GUILayout.ExpandWidth(true)))
        {
            Undo.RecordObject(_groundManager.FinishLine, "Set Finish Flag Point");
            _groundManager.FinishLine.SetFlagPoint(_curvePointObject.CurvePoint);
        }

        if (GUILayout.Button("Backstop", GUILayout.ExpandWidth(true)))
        {
            Undo.RecordObject(_groundManager.FinishLine, "Set Backstop Point");
            _groundManager.FinishLine.SetBackstopPoint(_curvePointObject.CurvePoint);
        }
        GUI.backgroundColor = originalColor;


        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        _showFloorOptions = EditorGUILayout.Foldout(_showFloorOptions, "Floor Options");

        if (_showFloorOptions)
        {
            EditorGUI.BeginChangeCheck();

            var floorHeight = EditorGUILayout.IntField("Floor Height", _curvePointObject.CurvePoint.FloorHeight);
            var floorAngle = EditorGUILayout.IntField("Floor Angle", _curvePointObject.CurvePoint.FloorAngle);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_curvePointObject, "Change height and angle.");
                _curvePointObject.CurvePoint.FloorHeight = floorHeight;
                _curvePointObject.CurvePoint.FloorAngle = floorAngle;

                RefreshGround();
            }
        }

    }
    public void OnSceneGUI()
    {
        var curvePointEditObject = (CurvePointEditObject)target;

        var startPos = curvePointEditObject.transform.position;

        var handlesChanged = DrawCurvePointHandles(curvePointEditObject);
        if (handlesChanged)
        {
            if (_editManager.doShiftEdits)
            {
                _editManager.ShiftCurvePoints(curvePointEditObject, curvePointEditObject.transform.position - startPos);
            }
            RefreshGround();
        }
    }

    public static bool DrawCurvePointHandles(CurvePointEditObject curvePointObject)
    {
        var objectPosition = curvePointObject.transform.position;
        var groundPosition = curvePointObject.ParentGround.transform.position;

        var handlesChanged = false;
        //Position handle

        if (curvePointObject.DoTargetLow)
        {
            Handles.color = Color.blue;
        }
        else if (curvePointObject.DoTargetHigh)
        {
            Handles.color = Color.green;
        }
        else
        {
            Handles.color = Color.white;
        }
        EditorGUI.BeginChangeCheck();

        var positionHandle = Handles.FreeMoveHandle(
            objectPosition,
            HandleUtility.GetHandleSize(objectPosition) * .1f,
            Vector3.zero,
            Handles.SphereHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curvePointObject, "Curve Point Edit");
            curvePointObject.PositionChanged(positionHandle);
            handlesChanged = true;
        }

        if (curvePointObject.CurvePoint.Mode == ShapeTangentMode.Linear)
        {
            return handlesChanged;
        }

        Handles.color = Color.red;

        Handles.DrawLine(objectPosition, curvePointObject.CurvePoint.LeftTangentPosition + groundPosition);
        Handles.DrawLine(objectPosition, curvePointObject.CurvePoint.RightTangentPosition + groundPosition);

        Handles.color = Color.yellow;

        //Left tangent handle
        EditorGUI.BeginChangeCheck();

        var leftTangentHandle = Handles.FreeMoveHandle(
            curvePointObject.CurvePoint.LeftTangentPosition + groundPosition,
            HandleUtility.GetHandleSize(curvePointObject.CurvePoint.LeftTangentPosition) * .4f,
            Vector3.zero,
            Handles.ArrowHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curvePointObject, "Curve Point Edit");
            curvePointObject.LeftTangentChanged(leftTangentHandle);
            handlesChanged = true;
        }


        //Right tangent handle
        EditorGUI.BeginChangeCheck();

        var rightTangentHandle = Handles.FreeMoveHandle(
            curvePointObject.CurvePoint.RightTangentPosition + groundPosition,
            HandleUtility.GetHandleSize(curvePointObject.CurvePoint.RightTangentPosition) * .4f,
            Vector3.zero,
            Handles.ArrowHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curvePointObject, "Curve Point Edit");
            curvePointObject.RightTangentChanged(rightTangentHandle);
            handlesChanged = true;
        }

        return handlesChanged;
    }

    private static CurvePointEditObject NextCurvePoint(CurvePointEditObject currentCurvePoint)
    {
        var curvePointObjects = currentCurvePoint.ParentGround.CurvePointObjects;
        var index = currentCurvePoint.transform.GetSiblingIndex();

        if (index < curvePointObjects.Length - 1)
        {
            return curvePointObjects[index + 1];
        }

        return null;
    }

    private static CurvePointEditObject PreviousCurvePoint(CurvePointEditObject currentCurvePoint)
    {
        var curvePointObjects = currentCurvePoint.ParentGround.CurvePointObjects;
        var index = currentCurvePoint.transform.GetSiblingIndex();
        if (index > 0)
        {
            return curvePointObjects[index - 1];
        }
        return null;
    }

    private void RefreshGround()
    {
        Undo.RecordObject(_curvePointObject.gameObject, "Refresh ground.");
        _editManager.RefreshSerializable(_curvePointObject.ParentGround);
    }

}