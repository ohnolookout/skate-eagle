using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StandardCurveSection : ICurveSection
{
    private CurvePoint _startPoint;
    private CurvePoint _endPoint;
    private CurvePoint _centerPoint;

    private Vector2 _xyDelta;  //Distance between start and end points
    private float _height; //Height of middlepoint perpendicular to slope of start and end points
    private float _skew; //Position of centerpoint along the line between start and end points
    private float _shape; //Velocity of centerPoint tangents as percentage of distance between start and end point tangents 

    public Vector2 XYDelta { get => _xyDelta; set => _xyDelta = value; }
    public float Height { get => _height; set => _height = value; }
    public float Skew { get => _skew; set => _skew = value; }
    public float Shape { get => _shape; set => _shape = value; }
    public CurveSectionType CurveType => CurveSectionType.Standard;



    public StandardCurveSection()
    {
        _xyDelta = new Vector2(20, 0);
        _height = 20;
        _skew = 0.5f;
        _shape = 0.5f;

        _startPoint = new();
        _endPoint = new();
        _centerPoint = new();

        UpdateCurvePoints();
    }

    public StandardCurveSection(Vector2 xyDelta, float height, float skew, float shape)
    {
        _xyDelta = xyDelta;
        _height = height;
        _skew = skew;
        _shape = shape;

        _startPoint = new();
        _endPoint = new();
        _centerPoint = new();

        UpdateCurvePoints();
    }

    public List<CurvePoint> GetCurvePoints()
    {
        return new() { _startPoint, _centerPoint, _endPoint };
    }

    private void UpdateCurvePoints()
    {
        _startPoint.ControlPoint = new Vector3(0, 0);
        _endPoint.ControlPoint = new Vector3(_xyDelta.x, _xyDelta.y);
        _centerPoint.ControlPoint = new Vector3(_xyDelta.x * _skew, _xyDelta.y * _skew + _height);

        var tangXYDelta = _endPoint.RightTangent - _startPoint.RightTangent;
        var centerTang = (tangXYDelta * _shape)/2;

        _centerPoint.SetTangents(centerTang);
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

    public void SetEndSlope(float slope, float magnitude)
    {
        _endPoint.SetTangents(slope, magnitude);
        UpdateCurvePoints();
    }




}
