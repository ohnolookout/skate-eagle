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

    [SerializeField] private Vector3 _xyDelta = new (40, 0);  //Distance between start and end points
    [SerializeField] private float _height = 10; //Height of middlepoint perpendicular to slope of start and end points as precentage of max height
    [SerializeField] private float _skew = 0.5f; //Position of centerpoint along the line between start and end points
    [SerializeField] private float _shape = 0.5f; //Velocity of centerPoint tangents as percentage of distance between start and end point tangents 
    [SerializeField] private float _startAngle = 45;
    [SerializeField] private float _endAngle = 45;
    [SerializeField] private float _startMagnitude = 7;
    [SerializeField] private float _endMagnitude = 7;
    private float _maxStartMagnitude;
    private float _maxEndMagnitude;
    [SerializeField] private CurveDirection _type;

    private const float _heightCeiling = 250;

    public Vector3 XYDelta { get => _xyDelta; set => _xyDelta = value; }
    public Vector3 PerpendicularVectorSlope => new(XYDelta.y, XYDelta.x);
    public float Height { get => _height; set => _height = Mathf.Clamp(value, -2, 2); }
    public float Skew { get => _skew; set => _skew = Mathf.Clamp01(value); }
    public float Shape { get => _shape; set => _shape = Mathf.Clamp01(value); }
    public float StartAngle { get => _startAngle; set => _startAngle = value; }
    public float EndAngle { get => _endAngle; set => _endAngle = value; }
    public float StartMagnitude { get => _startMagnitude; set => _startMagnitude = value; }
    public float EndMagnitude { get => _endMagnitude; set => _endMagnitude = value; }
    public CurvePoint StartPoint => _startPoint;
    public CurvePoint EndPoint => _endPoint;
    public CurveSectionType CurveType => CurveSectionType.Standard;
    public CurveDirection Type => _type;

    #region Construction
    public StandardCurveSection(CurveDirection type, Vector2? startTang = null)
    {
        _type = type;
        _startPoint = new(new Vector3(0, 0));
        _endPoint = new(new Vector3(_xyDelta.x, _xyDelta.y));
        _centerPoint = new();


        if (startTang != null)
        {
            var tangAngle = BezierMath.GetAngleFromTangent(_startPoint.Position, _endPoint.Position, (Vector2) startTang);
            _startAngle = tangAngle;
            _endAngle = tangAngle;            
            
            var tangMagnitude = ((Vector2) startTang).magnitude;
            _startMagnitude = tangMagnitude;
            _endMagnitude = tangMagnitude;
        }

        UpdateTangents();
        UpdateCenterPoint();
        UpdateCurvePoints();
    }

    public StandardCurveSection(Vector3 xyDelta, float height, float skew, float shape, CurveDirection type)
    {
        _type = type;
        XYDelta = xyDelta;
        Height = height;
        Skew = skew;
        Shape = shape;

        _startPoint = new(new Vector3(0, 0));
        _endPoint = new(new Vector3(_xyDelta.x, _xyDelta.y));
        _centerPoint = new();


        UpdateTangents();
        UpdateCenterPoint();
        UpdateCurvePoints();
    }

    public List<CurvePoint> GetCurvePoints(CurvePoint startPoint)
    {
        var positionChange = startPoint.Position;
        return new() { _startPoint.Move(positionChange), _centerPoint.Move(positionChange), _endPoint.Move(positionChange) };
    }
    #endregion

    #region Update Points
    public void UpdateCurvePoints()
    {
        ClampParameters();

        _startPoint.Position = new Vector3(0, 0);
        _endPoint.Position = new Vector3(_xyDelta.x, _xyDelta.y);

        UpdateTangents();
        UpdateCenterPoint();
        
    }
    private void ClampParameters()
    {
        if (Type == CurveDirection.Flat)
        {
            _height = 0;
            _startAngle = 0;
            _endAngle = 0;
        }
        else
        {
            _height = Mathf.Clamp(_height, MinHeight(), MaxHeight());
            _startAngle = Mathf.Clamp(_startAngle, 0, 89);
            _endAngle = Mathf.Clamp(_endAngle, 0, 89);
            _startMagnitude = Mathf.Clamp(_startMagnitude, 0, _maxStartMagnitude);
            _endMagnitude = Mathf.Clamp(_endMagnitude, 0, _maxEndMagnitude);
        }

        _skew = Mathf.Clamp01(_skew);
        _shape = Mathf.Clamp(_shape, 0.05f, 1);
    }

    private void UpdateTangents()
    {
        var startAngle = Type == CurveDirection.Valley ? -_startAngle : _startAngle;
        var endAngle = Type == CurveDirection.Valley ? -_endAngle : _endAngle;

        _startPoint.SetTangentAngles(startAngle, _endPoint.Position, false, _startMagnitude);
        _endPoint.SetTangentAngles(endAngle, _startPoint.Position, true, _endMagnitude);

    }
    #endregion

    #region Center Point
    private void UpdateCenterPoint()
    {
        var midPoint = Vector2.Lerp(_startPoint.Position, _endPoint.Position, _skew);
        var minHeight = MinHeight();
        var maxHeight = MaxHeight();
        var maxHeightDelta = maxHeight-minHeight;

        var adjustedHeight = _height;
        if(Type == CurveDirection.Valley)
        {
            adjustedHeight = -adjustedHeight;
        }

        var centerPosition = BezierMath.GetPointAlongLine(midPoint, PerpendicularVectorSlope, adjustedHeight);
        _centerPoint.Position = centerPosition;
        SetCenterPointTangents(_shape);
    }

    private void SetCenterPointTangents(float t)
    {
        GetMaxCenterTangents(out var maxLeftTangent, out var maxRightTangent);
        var leftTangent = Vector3.Lerp(_centerPoint.Position, maxLeftTangent, t) - _centerPoint.Position;
        var rightTangent = Vector3.Lerp(_centerPoint.Position, maxRightTangent, t) - _centerPoint.Position;
        _centerPoint.SetTangents(leftTangent, rightTangent);
    }

    private void GetMaxCenterTangents(out Vector2 maxLeftTangent, out Vector2 maxRightTangent)
    {
        var baselineStartPoint = BezierMath.GetPerpendicularIntersection(_startPoint.Position, _endPoint.Position, _startPoint.RightTangentPosition);
        var baseLineEndPoint = BezierMath.GetPerpendicularIntersection(_startPoint.Position, _endPoint.Position, _endPoint.LeftTangentPosition);

        maxLeftTangent = BezierMath.GetParallelProjectionPoint(baselineStartPoint, baseLineEndPoint, _centerPoint.Position, 0);
        maxRightTangent = BezierMath.GetParallelProjectionPoint(baselineStartPoint, baseLineEndPoint, _centerPoint.Position, 1);

        _maxStartMagnitude = (maxLeftTangent - (Vector2)_startPoint.Position).magnitude;
        _maxEndMagnitude = ((Vector2)_endPoint.Position - maxRightTangent).magnitude;
    }

    private float MaxHeight()
    {
        var tangentIntersection = BezierMath.GetIntersection(_startPoint, _endPoint);
        if (tangentIntersection == null)
        {
            return _heightCeiling/2;
        }

        var max = BezierMath.GetPerpendicularDistance(_startPoint.Position, _endPoint.Position, (Vector2)tangentIntersection);


        return Mathf.Min(max * 1.5f, _heightCeiling/2);
    }

    private float MinHeight()
    {
        var leftTangHeight = BezierMath.GetPerpendicularDistance(_startPoint.Position, _endPoint.Position, _startPoint.RightTangentPosition);
        var rightTangHeight = BezierMath.GetPerpendicularDistance(_startPoint.Position, _endPoint.Position, _endPoint.LeftTangentPosition);
        var min = Mathf.Max(leftTangHeight, rightTangHeight);

        return min;
    }
    #endregion
    




}
