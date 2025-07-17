using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class CurvePointEditObject : MonoBehaviour
{
    private CurvePoint _curvePoint;
    private Action<CurvePointEditObject> _onCurvePointChange;

    public Action<CurvePointEditObject> OnCurvePointChange;
    public Transform groundTransform;

    public CurvePoint CurvePoint => _curvePoint;

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + groundTransform.position, 1);
    }
    public void SetCurvePoint(CurvePoint curvePoint)
    {
        _curvePoint = curvePoint;
        transform.position = _curvePoint.Position;
    }

    public void TangentsChanged(Vector3 updatedleftTang, Vector3 updatedRightTang)
    {
        _curvePoint.LeftTangent = updatedleftTang - _curvePoint.Position;
        _curvePoint.RightTangent = updatedRightTang - _curvePoint.Position;

        _onCurvePointChange?.Invoke(this);
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(CurvePointEditObject))]
public class CurvePointEditObjectEditor : Editor
{
    public void OnSceneGUI()
    {
        var curvePointEditObject = (CurvePointEditObject)target;
        var objectPosition = curvePointEditObject.transform.position;
        var groundPosition = curvePointEditObject.groundTransform.position;
        var worldPosition = objectPosition + groundPosition;

        Handles.color = Color.red;

        Handles.DrawLine(worldPosition, curvePointEditObject.CurvePoint.LeftTangentPosition + groundPosition);
        Handles.DrawLine(worldPosition , curvePointEditObject.CurvePoint.RightTangentPosition + groundPosition);


        var leftTangentHandle = Handles.FreeMoveHandle(
            curvePointEditObject.CurvePoint.LeftTangentPosition + groundPosition,
            HandleUtility.GetHandleSize(curvePointEditObject.CurvePoint.LeftTangentPosition) * .4f,
            Vector3.zero,
            Handles.ArrowHandleCap);

        var rightTangentHandle = Handles.FreeMoveHandle(
            curvePointEditObject.CurvePoint.RightTangentPosition + groundPosition,
            HandleUtility.GetHandleSize(curvePointEditObject.CurvePoint.RightTangentPosition) * .4f,
            Vector3.zero,
            Handles.ArrowHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curvePointEditObject, "Curve Point Edit");
            curvePointEditObject.TangentsChanged(leftTangentHandle, rightTangentHandle);
        }
    }
}

#endif
