using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveFactory
{
    public static Curve DefaultCurve(Vector2 prevTang)
    {
        var peakSection = new StandardCurveSection(CurveDirection.Peak);
        var valleySection = new StandardCurveSection(CurveDirection.Valley);
        valleySection.Height = -valleySection.Height;
        valleySection.SetStartTangents(prevTang);

        List<ICurveSection> curveSections = new() { valleySection, peakSection };

        return new(curveSections);
    }

    public static Curve DefaultStartLine()
    {
        var firstPoint = new CurvePoint(new (0, 0), new (0, -1), new (40, -130));
        var secondPoint = new CurvePoint(new(400, -150), new(-45, 0.5f), new(10, -0.5f));

        const float thirdPointXVelocity = 10;
        const float thirdPointXSlope = -1.1f;
        Vector3 leftTangent = new(-thirdPointXVelocity, -thirdPointXVelocity * thirdPointXSlope);
        Vector3 rightTangent = new(thirdPointXVelocity, thirdPointXVelocity * thirdPointXSlope);
        var thirdPoint = new CurvePoint(secondPoint.Position + new Vector3(30, -12), leftTangent, rightTangent);

        var curvePoints = new List<CurvePoint> { firstPoint, secondPoint, thirdPoint };
        return new Curve(curvePoints);
    }

    public static Curve DefaultFinishLine(CurvePoint startPoint)
    {
        //Generate first point at controlPoint
        var firstPoint = new CurvePoint(new(0, 0), startPoint.LeftTangent, -startPoint.LeftTangent);

        //Generate second point
        var finishY = Mathf.Max(Mathf.Abs(firstPoint.RightTangent.y), 12);
        var secondLocation = new Vector3(finishY * 3, -finishY);
        var secondPoint = new CurvePoint(secondLocation, new Vector3(-6, 0), new Vector3(6, 0));

        //Generate third point
        var thirdLocation = new Vector3(secondPoint.Position.x + 500, secondPoint.Position.y);
        var thirdPoint = new CurvePoint(thirdLocation, new Vector3(-6, 0), new Vector3(6, 0));

        var curvePoints = new List<CurvePoint> { firstPoint, secondPoint, thirdPoint };
        return new Curve(curvePoints);
    }

}
