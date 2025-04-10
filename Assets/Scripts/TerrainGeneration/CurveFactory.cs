using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveFactory
{
    public static Curve DefaultCurve(Vector2? prevTang)
    {
        var peak = new StandardCurveSection(CurveDirection.Peak, prevTang);
        var valley = new StandardCurveSection(CurveDirection.Valley, peak.EndPoint.RightTangent);

        List<StandardCurveSection> curveSections = new() { peak, valley };

        return new(curveSections);
    }

    public static Curve DefaultStartLine()
    {
        var flatSection = new StandardCurveSection(CurveDirection.Flat);
        flatSection.XYDelta = new(75, 0);
        flatSection.StartMagnitude = 7;
        flatSection.EndMagnitude = 20;
        flatSection.UpdateCurvePoints();

        var valleySection = new StandardCurveSection(CurveDirection.Valley, flatSection.EndPoint.RightTangent);
        valleySection.XYDelta = new(59, -12);
        valleySection.Height = 21;
        valleySection.Skew = 63;
        valleySection.Shape = 70;
        valleySection.EndAngle = 48;
        valleySection.StartMagnitude = 20;
        valleySection.EndMagnitude = 7;
        valleySection.UpdateCurvePoints(flatSection.EndPoint.RightTangent);

        return new Curve(new List<StandardCurveSection>() { flatSection, valleySection});
    }

    public static Curve DefaultFinishLine(CurvePoint startPoint)
    {
        var baseXDelta = Mathf.Max(startPoint.RightTangent.x, startPoint.RightTangent.y, 8);
        var xyDelta = new Vector2(baseXDelta * 10, startPoint.RightTangent.y * 7);
        var height = 18;
        var skew = 45;
        var shape = 45;
        var valleySection = new StandardCurveSection(CurveDirection.Valley, xyDelta, height, skew, shape, startPoint.RightTangent);

        var flatSection = new StandardCurveSection(CurveDirection.Flat, valleySection.EndPoint.RightTangent);
        flatSection.XYDelta = new(300, 0);
        flatSection.StartMagnitude = 7;
        flatSection.EndMagnitude = 20;
        flatSection.UpdateCurvePoints(valleySection.EndPoint.RightTangent);

        return new Curve(new List<StandardCurveSection>() { valleySection, flatSection});
    }

}
