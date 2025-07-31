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

        if (_curvePointObject.LinkedCameraTarget.RightTargets.Count > 0)
        {
            GUILayout.Label($"First right target transform != null: {_curvePointObject.LinkedCameraTarget.RightTargets[0].Target.TargetTransform != null}");
        }
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
            Undo.RecordObject(_groundManager.StartLine, "Set Start Point");
            _groundManager.StartLine.SetStartLine(_curvePointObject.CurvePoint);
        }

        if (GUILayout.Button("Set as Finish Flag Point", GUILayout.ExpandWidth(false)))
        {
            Undo.RecordObject(_groundManager.FinishLine, "Set Finish Flag Point");
            _groundManager.FinishLine.SetFlagPoint(_curvePointObject.CurvePoint);
        }

        if (GUILayout.Button("Set as Finish Backstop Point", GUILayout.ExpandWidth(false)))
        {
            Undo.RecordObject(_groundManager.FinishLine, "Set Backstop Point");
            _groundManager.FinishLine.SetBackstopPoint(_curvePointObject.CurvePoint);
        }

        if(GUILayout.Button("Set Finish Flag and Backstop", GUILayout.ExpandWidth(false)))
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

        if (GUILayout.Button("Find Next CurvePoints", GUILayout.ExpandWidth(false)))
        {
            if (!_curvePointObject.DoTargetLow)
            {
                Debug.LogWarning("Curve point not set as camera target. Cannot find adjacent curve points.");
                return;
            }
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

    private static CurvePointObject NextCurvePoint(CurvePointObject currentCurvePoint)
    {
        var curvePointObjects = currentCurvePoint.ParentGround.CurvePointObjects;
        var index = curvePointObjects.IndexOf(currentCurvePoint);

        if (index < curvePointObjects.Count - 1)
        {
            return curvePointObjects[index + 1];
        }

        return null;
    }

    private static CurvePointObject PreviousCurvePoint(CurvePointObject currentCurvePoint)
    {
        var curvePointObjects = currentCurvePoint.ParentGround.CurvePointObjects;
        var index = curvePointObjects.IndexOf(currentCurvePoint);
        if (index > 0)
        {
            return curvePointObjects[index - 1];
        }
        return null;
    }

}