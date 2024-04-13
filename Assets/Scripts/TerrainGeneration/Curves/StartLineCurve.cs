using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLineCurve : Curve
{
    const float _thirdPointXVelocity = 10;
    const float _thirdPointXSlope = -1.1f;
    public StartLineCurve(CurvePoint origin)
    {
        curvePoints = GenerateCurvePoints(origin.ControlPoint);
        curveType = CurveType.FinishLine;
        _highpoint = curvePoints[1].ControlPoint;
        _lowpoint = curvePoints[2].ControlPoint;
        GenerateCurveStats();
    }

    private static List<CurvePoint> GenerateCurvePoints(Vector3 origin)
    {
        return new() { FirstPoint(origin), SecondPoint(origin), ThirdPoint(origin) };
    }

    private static CurvePoint FirstPoint(Vector3 origin)
    {
        Vector3 firstLocation = new(origin.x - 400, origin.y + 150);
        return new(firstLocation, new Vector2(0, -1), new Vector2(40, -130));
    }

    private static CurvePoint SecondPoint(Vector3 origin)
    {
        return new(origin, new Vector2(-45, 0.5f), new Vector2(10, -0.5f));
    }

    private static CurvePoint ThirdPoint(Vector3 origin)
    {
        Vector3 leftTangent = new(-_thirdPointXVelocity, -_thirdPointXVelocity * _thirdPointXSlope);
        Vector3 rightTangent = new(_thirdPointXVelocity, _thirdPointXVelocity * _thirdPointXSlope);
        return new(origin + new Vector3(30, -12), leftTangent, rightTangent);
    }
}
