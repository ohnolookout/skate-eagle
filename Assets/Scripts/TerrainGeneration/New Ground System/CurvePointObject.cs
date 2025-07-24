using UnityEngine;
using System;
using UnityEngine.U2D;
using System.Collections.Generic;

public class CurvePointObject : MonoBehaviour, ICameraTargetable
{
    #region Declarations
    public CurvePoint curvePoint;
    private Ground _parentGround;
    private Action<CurvePointObject> _onCurvePointChange;
    private CurvePointObject _nextLeftCurvePointObject;
    private CurvePointObject _nextRightCurvePointObject;
    public Action<CurvePointObject> OnCurvePointChange;
    public List<GameObject> rightTargetObjects = new();
    public List<GameObject> leftTargetObjects = new();
    private LinkedCameraTarget _linkedCameraTarget = new();
    public CurvePoint CurvePoint => curvePoint;
    public Ground ParentGround { get => _parentGround; set => _parentGround = value; }
    public List<GameObject> RightTargetObjects { get => rightTargetObjects; set => rightTargetObjects = value; }
    public List<GameObject> LeftTargetObjects { get => leftTargetObjects; set => leftTargetObjects = value; }
    public LinkedCameraTarget LinkedCameraTarget { get => curvePoint.LinkedCameraTarget; set => curvePoint.LinkedCameraTarget = value; }
    public bool DoTargetLow { get => curvePoint.DoTargetLow; set => curvePoint.DoTargetLow = value; }
    public bool DoTargetHigh { get => curvePoint.DoTargetHigh; set => curvePoint.DoTargetHigh = value; }
    public CurvePointObject NextLeftCurvePointObject 
    { 
        get  
        {
            var index = ParentGround.CurvePointObjects.IndexOf(this);
            
            if (index != 0)
            {
                for(int i = index - 1; i >= 0; i--)
                {
                    if (ParentGround.CurvePointObjects[i].DoTargetLow)
                    {
                        return ParentGround.CurvePointObjects[i];
                    }
                }
            }

            return _nextLeftCurvePointObject;
        }

        set => _nextLeftCurvePointObject = value; 
    }
    public CurvePointObject NextRightCurvePointObject
    {
        get
        {
            var index = ParentGround.CurvePointObjects.IndexOf(this);

            for(int i = index + 1; i < ParentGround.CurvePointObjects.Count; i++)
            {
                if (ParentGround.CurvePointObjects[i].DoTargetLow)
                {
                    return ParentGround.CurvePointObjects[i];
                }
            }

            return _nextRightCurvePointObject;
        }
        set => _nextRightCurvePointObject = value;
    }
    #endregion

    #region Monobehaviors
    void OnDrawGizmosSelected()
    {
        LinkedCameraTarget.DrawTargets();
    }

    #endregion
    public void SetCurvePoint(CurvePoint curvePoint)
    {
        this.curvePoint = curvePoint;
        this.curvePoint.Object = this; // Set the object reference in the CurvePoint
        transform.position = this.curvePoint.Position + ParentGround.transform.position;
    }

    public void TangentsChanged(Vector3 updatedleftTang, Vector3 updatedRightTang)
    {
        curvePoint.LeftTangent = updatedleftTang - transform.position;
        curvePoint.RightTangent = updatedRightTang - transform.position;

        _onCurvePointChange?.Invoke(this);
    }

    public void LeftTangentChanged(Vector3 updatedTang)
    {
        curvePoint.LeftTangent = updatedTang - transform.position;

        if (curvePoint.IsSymmetrical)
        {
            curvePoint.RightTangent = -curvePoint.LeftTangent; // If it's symmetrical, update the right tangent accordingly
        }
        else if (curvePoint.Mode == ShapeTangentMode.Continuous)
        {
            var rightMagnitude = curvePoint.RightTangent.magnitude;
            curvePoint.RightTangent = -curvePoint.LeftTangent.normalized * rightMagnitude; // Maintain the same magnitude for the left tangent
        }

        _onCurvePointChange?.Invoke(this);
    }

    public void RightTangentChanged(Vector3 updatedTang)
    {

        curvePoint.RightTangent = updatedTang - transform.position;

        if(curvePoint.IsSymmetrical)
        {
            curvePoint.LeftTangent = -curvePoint.RightTangent; // If it's symmetrical, update the right tangent accordingly
        } else if (curvePoint.Mode == ShapeTangentMode.Continuous)
        {
            var leftMagnitude = curvePoint.LeftTangent.magnitude;
            curvePoint.LeftTangent = -curvePoint.RightTangent.normalized * leftMagnitude; // Maintain the same magnitude for the left tangent
        }
        
        _onCurvePointChange?.Invoke(this);
    }

    public void PositionChanged(Vector3 updatedPosition)
    {
        var localPosition = updatedPosition - ParentGround.transform.position;
        curvePoint.Position = localPosition;
        transform.position = localPosition;
    }

    public void SettingsChanged(ShapeTangentMode mode, bool isSymmetrical)
    {
        curvePoint.Mode = mode;
        curvePoint.IsSymmetrical = isSymmetrical;
        if (isSymmetrical)
        {
            curvePoint.RightTangent = -curvePoint.LeftTangent; // If it's symmetrical, update the right tangent accordingly
        }
        else if (curvePoint.Mode == ShapeTangentMode.Continuous)
        {
            var rightMagnitude = curvePoint.RightTangent.magnitude;
            curvePoint.RightTangent = -curvePoint.LeftTangent.normalized * rightMagnitude; // Maintain the same magnitude for the left tangent
        }
        _onCurvePointChange?.Invoke(this);
    }

    public void PopulateDefaultTargets()
    {
        _linkedCameraTarget.Target = CameraTargetUtility.GetTarget(CameraTargetType.CurvePointLow, transform);

        if (NextLeftCurvePointObject != null && !leftTargetObjects.Contains(NextLeftCurvePointObject.gameObject))
        {
            leftTargetObjects.Add(NextLeftCurvePointObject.gameObject);
        }

        if (NextRightCurvePointObject != null && !rightTargetObjects.Contains(NextRightCurvePointObject.gameObject))
        {
            rightTargetObjects.Add(NextRightCurvePointObject.gameObject);
        }
    }
}



