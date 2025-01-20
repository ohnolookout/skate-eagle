using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLineCurve : Curve
{
    const float _thirdPointXVelocity = 10;
    const float _thirdPointXSlope = -1.1f;
    public StartLineCurve()
    {
        curvePoints = GenerateCurvePoints(new(0,0));
        curveType = CurveType.StartLine;
        _highpoint = curvePoints[1].ControlPoint;
        _lowpoint = curvePoints[2].ControlPoint;
        GenerateCurveStats();
    }

    private static List<CurvePoint> GenerateCurvePoints(Vector3 origin)
    {
        var firstPoint = FirstPoint();
        var secondPoint = SecondPoint();
        var thirdPoint = ThirdPoint(secondPoint.ControlPoint);
        return new() { firstPoint, secondPoint, thirdPoint };
    }

    private static CurvePoint FirstPoint()
    {
        return new(new(0, 0), new(0, -1), new(40, -130));
    }

    private static CurvePoint SecondPoint()
    {
        return new(new(400, -150), new(-45, 0.5f), new(10, -0.5f));
    }

    private static CurvePoint ThirdPoint(Vector3 secondPoint)
    {
        Vector3 leftTangent = new(-_thirdPointXVelocity, -_thirdPointXVelocity * _thirdPointXSlope);
        Vector3 rightTangent = new(_thirdPointXVelocity, _thirdPointXVelocity * _thirdPointXSlope);
        return new(secondPoint + new Vector3(30, -12), leftTangent, rightTangent);
    }
}
