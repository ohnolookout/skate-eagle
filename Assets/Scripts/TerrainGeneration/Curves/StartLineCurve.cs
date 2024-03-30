using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLineCurve : Curve
{
    public StartLineCurve(CurvePoint startPoint)
    {
        curvePoints = GenerateStartPoints(startPoint);
        curveType = CurveType.FinishLine;
        GenerateCurveStats();
    }
    private float eagleHeight = 8.4962f; //Calculated by eagle.gameObject.GetComponent<SpriteRenderer>().bounds.size.y
    private List<CurvePoint> GenerateStartPoints(CurvePoint startPoint)
    {
        List<CurvePoint> curvePoints = new();
        Vector3 startLocation = new(startPoint.ControlPoint.x - 400, startPoint.ControlPoint.y + 150);
        CurvePoint firstPoint = new(startLocation, new Vector2(0, -1), new Vector2(40, -130));
        curvePoints.Add(firstPoint);
        Vector3 secondLocation = new(startPoint.ControlPoint.x, startPoint.ControlPoint.y - (eagleHeight / 2) - 1.1f);
        CurvePoint secondPoint = new(secondLocation, new Vector2(-45, 0.5f), new Vector2(10, -0.5f));
        curvePoints.Add(secondPoint);
        CurvePoint thirdPoint = new();
        float xVelocity = 10;
        float slope = -1.1f;
        thirdPoint.ControlPoint = secondPoint.ControlPoint + new Vector3(30, -12);
        thirdPoint.LeftTangent = new(-xVelocity, -xVelocity * slope);
        thirdPoint.RightTangent = new(xVelocity, xVelocity * slope);
        curvePoints.Add(thirdPoint);
        _highpoint = secondPoint.ControlPoint;
        _lowpoint = thirdPoint.ControlPoint;

        return curvePoints;
    }
}
