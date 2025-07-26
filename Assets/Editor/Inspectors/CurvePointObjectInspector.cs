using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

[CustomEditor(typeof(CurvePointObject))]
public class CurvePointObjectInspector : Editor
{
    private CurvePointObject _curvePointObject;
    private SerializedObject _so;
    private SerializedProperty _serializedLeftTargetObjects;
    private SerializedProperty _serializedRightTargetObjects;
    private FindAdjacentCurvePointWindow _findAdjacentCurvePointWindow;
    private GroundManager _groundManager;
    public override void OnInspectorGUI()
    {
        _curvePointObject = (CurvePointObject)target;
        _so = new SerializedObject(_curvePointObject);
        _serializedLeftTargetObjects = _so.FindProperty("leftTargetObjects");
        _serializedRightTargetObjects = _so.FindProperty("rightTargetObjects");
        _groundManager = FindFirstObjectByType<GroundManager>();


        //Curvepoint settings
        EditorGUI.BeginChangeCheck();

        var isSymmetrical = GUILayout.Toggle(_curvePointObject.CurvePoint.IsSymmetrical, "Symmetrical");
        var mode = (ShapeTangentMode)EditorGUILayout.EnumPopup("Tangent Mode", _curvePointObject.CurvePoint.Mode);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Settings");
            _curvePointObject.SettingsChanged(mode, isSymmetrical);
        }

        if (GUILayout.Button("Reset Tangents"))
        {
            Undo.RecordObject(_curvePointObject, "Reset Curve Point Tangents");
            _curvePointObject.TangentsChanged(
                _curvePointObject.CurvePoint.Position + new Vector3(-1, 0),
                _curvePointObject.CurvePoint.Position + new Vector3(-1, 0));
        }

        //Camera targetting
        EditorGUI.BeginChangeCheck();

        var doTargetLow = GUILayout.Toggle(_curvePointObject.DoTargetLow, "Do Target Low");

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Target Settings");
            _curvePointObject.DoTargetLow = doTargetLow;
            _curvePointObject.PopulateDefaultTargets();
        }

        if (_curvePointObject.DoTargetLow)
        {
            //Show linked camera target settings
        }

        EditorGUI.BeginChangeCheck();

        var doTargetHigh = GUILayout.Toggle(_curvePointObject.DoTargetHigh, "Do Target High");

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Target Settings");
            _curvePointObject.DoTargetHigh = doTargetHigh;
            _curvePointObject.PopulateDefaultTargets();
        }

        if (_curvePointObject.DoTargetLow || _curvePointObject.DoTargetHigh)
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

        if (GUILayout.Button("Set as Start Point", GUILayout.ExpandWidth(false)))
        {
            _groundManager.StartLine.SetStartLine(_curvePointObject.CurvePoint);
        }


        if (GUILayout.Button("Find Next CurvePoints", GUILayout.ExpandWidth(false)))
        {
            _findAdjacentCurvePointWindow = EditorWindow.GetWindow<FindAdjacentCurvePointWindow>();
            _findAdjacentCurvePointWindow.Init(_curvePointObject);
        }
    }
    public void OnSceneGUI()
    {
        var curvePointEditObject = (CurvePointObject)target;

        DrawCurvePointHandles(curvePointEditObject);
    }

    public static void DrawCurvePointHandles(CurvePointObject curvePointObject)
    {
        var objectPosition = curvePointObject.transform.position;
        var groundPosition = curvePointObject.ParentGround.transform.position;


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
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Edit");
            curvePointObject.PositionChanged(positionHandle);
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
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Edit");
            curvePointObject.LeftTangentChanged(leftTangentHandle);
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
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Edit");
            curvePointObject.RightTangentChanged(rightTangentHandle);
        }
    }
    
}