using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class CurvePointObject : MonoBehaviour
{
    private CurvePoint _curvePoint;
    private Action<CurvePointObject> _onCurvePointChange;

    public Action<CurvePointObject> OnCurvePointChange;
    public Transform groundTransform;

    public CurvePoint CurvePoint => _curvePoint;
    public Vector3 WorldPosition => transform.position + groundTransform.position;

    public void SetCurvePoint(CurvePoint curvePoint)
    {
        _curvePoint = curvePoint;
        transform.position = _curvePoint.Position;
    }

    public void TangentsChanged(Vector3 updatedleftTang, Vector3 updatedRightTang)
    {
        _curvePoint.LeftTangent = updatedleftTang - WorldPosition;
        _curvePoint.RightTangent = updatedRightTang - WorldPosition;

        _onCurvePointChange?.Invoke(this);
    }

    public void LeftTangentChanged(Vector3 updatedTang)
    {
        _curvePoint.LeftTangent = updatedTang - WorldPosition;

        if (_curvePoint.IsSymmetrical)
        {
            _curvePoint.RightTangent = -_curvePoint.LeftTangent; // If it's symmetrical, update the right tangent accordingly
        }
        else if (!_curvePoint.IsBroken)
        {
            var rightMagnitude = _curvePoint.RightTangent.magnitude;
            _curvePoint.RightTangent = -_curvePoint.LeftTangent.normalized * rightMagnitude; // Maintain the same magnitude for the left tangent
        }

        _onCurvePointChange?.Invoke(this);
    }

    public void RightTangentChanged(Vector3 updatedTang)
    {

        _curvePoint.RightTangent = updatedTang - WorldPosition;

        if(_curvePoint.IsSymmetrical)
        {
            _curvePoint.LeftTangent = -_curvePoint.RightTangent; // If it's symmetrical, update the right tangent accordingly
        } else if (!_curvePoint.IsBroken)
        {
            var leftMagnitude = _curvePoint.LeftTangent.magnitude;
            _curvePoint.LeftTangent = -_curvePoint.RightTangent.normalized * leftMagnitude; // Maintain the same magnitude for the left tangent
        }
        
        _onCurvePointChange?.Invoke(this);
    }

    public void PositionChanged(Vector3 updatedPosition)
    {
        var localPosition = updatedPosition - groundTransform.position;
        _curvePoint.Position = localPosition;
        transform.position = localPosition;
    }

    public void SettingsChanged(bool isBroken, bool isSymmetrical, bool isCorner)
    {
        _curvePoint.IsBroken = isBroken;
        _curvePoint.IsSymmetrical = isSymmetrical;
        if (isSymmetrical)
        {
            _curvePoint.RightTangent = -_curvePoint.LeftTangent; // If it's symmetrical, update the right tangent accordingly
        }
        else if (!_curvePoint.IsBroken)
        {
            var rightMagnitude = _curvePoint.RightTangent.magnitude;
            _curvePoint.RightTangent = -_curvePoint.LeftTangent.normalized * rightMagnitude; // Maintain the same magnitude for the left tangent
        }
        _onCurvePointChange?.Invoke(this);
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(CurvePointObject))]
public class CurvePointObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var curvePointObject = (CurvePointObject)target;

        //Curvepoint settings
        EditorGUI.BeginChangeCheck();

        var isSymmetrical = GUILayout.Toggle(curvePointObject.CurvePoint.IsSymmetrical, "Symmetrical");
        var isBroken = GUILayout.Toggle(curvePointObject.CurvePoint.IsBroken, "Broken");

        if(EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Settings");
            curvePointObject.SettingsChanged(isBroken, isSymmetrical, curvePointObject.CurvePoint.IsCorner);
        }

        if (GUILayout.Button("Reset Tangents"))
        {
            Undo.RecordObject(curvePointObject, "Reset Curve Point Tangents");
            curvePointObject.TangentsChanged(
                curvePointObject.CurvePoint.Position + new Vector3(-1, 0),
                curvePointObject.CurvePoint.Position + new Vector3(-1, 0));
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
        var groundPosition = curvePointObject.groundTransform.position;
        var worldPosition = curvePointObject.WorldPosition;


        //Position handle

        Handles.color = Color.green;
        EditorGUI.BeginChangeCheck();

        var positionHandle = Handles.FreeMoveHandle(
            worldPosition,
            HandleUtility.GetHandleSize(worldPosition) * .1f,
            Vector3.zero,
            Handles.SphereHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Edit");
            curvePointObject.PositionChanged(positionHandle);
        }


        Handles.color = Color.red;

        Handles.DrawLine(worldPosition, curvePointObject.CurvePoint.LeftTangentPosition + groundPosition);
        Handles.DrawLine(worldPosition, curvePointObject.CurvePoint.RightTangentPosition + groundPosition);

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

#endif
