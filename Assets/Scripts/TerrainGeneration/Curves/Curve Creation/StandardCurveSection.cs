using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public enum CurveDirection
{
    Valley,
    Peak,
    Flat
}

[Serializable]
public class StandardCurveSection : ICurveSection
{
    [SerializeField] private CurvePoint _startPoint;
    [SerializeField] private CurvePoint _endPoint;
    [SerializeField] private CurvePoint _centerPoint;

    [SerializeField] private Vector3 _xyDelta;  //Distance between start and end points
    [SerializeField] private float _height; //Height of middlepoint perpendicular to slope of start and end points as precentage of max height
    [SerializeField] private float _maxHeight; //Max height of middlepoint perpendicular to slope of start and end points
    [SerializeField] private float _skew; //Position of centerpoint along the line between start and end points
    [SerializeField] private float _shape; //Velocity of centerPoint tangents as percentage of distance between start and end point tangents 
    [SerializeField] private float _leftAngle;
    [SerializeField] private float _rightAngle;
    [SerializeField] private CurveDirection _type;

    private const float _heightCeiling = 250;

    public Vector3 XYDelta { get => _xyDelta; set => _xyDelta = value; }
    public Vector3 PerpendicularVectorSlope => new(XYDelta.y, XYDelta.x);
    public float Height { get => _height; set => _height = Mathf.Clamp(value, -2, 2); }
    public float Skew { get => _skew; set => _skew = Mathf.Clamp01(value); }
    public float Shape { get => _shape; set => _shape = Mathf.Clamp01(value); }
    public float LeftAngle { get => _leftAngle; set => _leftAngle = Mathf.Clamp(value, -89, 89); }
    public float RightAngle { get => _rightAngle; set => _rightAngle = Mathf.Clamp(value, -89, 89); }
    public CurveSectionType CurveType => CurveSectionType.Standard;
    public CurveDirection Type => _type;



    public StandardCurveSection(CurveDirection type)
    {
        _xyDelta = new Vector3(20, 0);
        _height = 0.8f;
        _skew = 0.5f;
        _shape = 0.5f;
        _type = type;

        _startPoint = new(new Vector3(0, 0));
        _endPoint = new(new Vector3(_xyDelta.x, _xyDelta.y));
        _centerPoint = new();
        
        SetTangentAngles(45, 5);

        UpdateCurvePoints();
    }

    public StandardCurveSection(Vector3 xyDelta, float height, float skew, float shape, CurveDirection type)
    {
        XYDelta = xyDelta;
        Height = height;
        Skew = skew;
        Shape = shape;

        _startPoint = new();
        _endPoint = new();
        _centerPoint = new();

        _type = type;

        UpdateCurvePoints();
    }

    public List<CurvePoint> GetCurvePoints(CurvePoint startPoint)
    {
        var positionChange = startPoint.Position;
        return new() { _startPoint.Move(positionChange), _centerPoint.Move(positionChange), _endPoint.Move(positionChange) };
    }

    private void UpdateCurvePoints()
    {
        _startPoint.Position = new Vector3(0, 0);
        _endPoint.Position = new Vector3(_xyDelta.x, _xyDelta.y);
        _endPoint.SetTangents(new(_startPoint.RightTangent.x, -_startPoint.RightTangent.y));
        
        SetCenterPoint();
        
    }

    public void SetStartTangents(float slope, float magnitude)
    {
        _startPoint.SetTangents(slope, magnitude);
        UpdateCurvePoints();
    }

    public void SetStartTangents(Vector3 tangent)
    {
        _startPoint.SetTangents(tangent);
        UpdateCurvePoints();
    }

    public void SetTangentAngles(float angle, float? magnitude = null)
    {
        if(Type == CurveDirection.Valley)
        {
            angle = -angle;
        }

        _startPoint.SetTangentAngles(angle, _endPoint.Position, magnitude);
        _endPoint.SetTangentAngles(-angle, _startPoint.Position, magnitude);
        UpdateCurvePoints();
    }

    public void SetEndSlope(float slope, float magnitude)
    {
        _endPoint.SetTangents(slope, magnitude);
        UpdateCurvePoints();
    }

    private void SetCenterPoint()
    {
        var midPoint = Vector2.Lerp(_startPoint.Position, _endPoint.Position, _skew);
        var minHeight = MinHeight();
        var maxHeight = MaxHeight();
        var maxHeightDelta = maxHeight-minHeight;
        var centerPosition = BezierMath.GetPointAlongLine(midPoint, PerpendicularVectorSlope, minHeight + Height * maxHeightDelta);
        _centerPoint.Position = centerPosition;
        Debug.Log("Center point position: " + _centerPoint.Position);
        SetCenterPointTangents(_shape);
    }

    private void SetCenterPointTangents(float t)
    {
        GetMaxTangents(out var maxLeftTangent, out var maxRightTangent);
        var leftTangent = Vector3.Lerp(maxLeftTangent, _centerPoint.Position, t) - _centerPoint.Position;
        var rightTangent = Vector3.Lerp(_centerPoint.Position, maxRightTangent, t) - _centerPoint.Position;
        _centerPoint.SetTangents(leftTangent, rightTangent);
    }

    private void GetMaxTangents(out Vector2 maxLeftTangent, out Vector2 maxRightTangent)
    {
        var baselineStartPoint = BezierMath.GetPerpendicularIntersection(_startPoint.Position, _endPoint.Position, _startPoint.RightTangentPosition);
        var baseLineEndPoint = BezierMath.GetPerpendicularIntersection(_startPoint.Position, _endPoint.Position, _endPoint.LeftTangentPosition);

        maxLeftTangent = BezierMath.GetParallelProjectionPoint(baselineStartPoint, baseLineEndPoint, _centerPoint.Position, 0);
        maxRightTangent = BezierMath.GetParallelProjectionPoint(baselineStartPoint, baseLineEndPoint, _centerPoint.Position, 1);
    }

    private float MaxHeight()
    {
        var tangentIntersection = BezierMath.GetIntersection(_startPoint, _endPoint);
        if (tangentIntersection == null)
        {
            Debug.Log("Could not calculate max height of center point");
            return _heightCeiling/2;
        }
        var max = BezierMath.GetPerpendicularDistance(_startPoint.Position, _endPoint.Position, (Vector2)tangentIntersection);
        Debug.Log("Calculated max height of center point: " + max);
        return Mathf.Min(max, _heightCeiling/2);
    }

    private float MinHeight()
    {
        var leftTangHeight = BezierMath.GetPerpendicularDistance(_startPoint.Position, _endPoint.Position, _startPoint.RightTangentPosition);
        var rightTangHeight = BezierMath.GetPerpendicularDistance(_startPoint.Position, _endPoint.Position, _endPoint.LeftTangentPosition);
        return Mathf.Max(leftTangHeight, rightTangHeight);
    }




}
