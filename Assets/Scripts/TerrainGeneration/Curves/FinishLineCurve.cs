using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLineCurve : Curve
{
    public FinishLineCurve(CurvePoint startPoint)
    {
        curvePoints = GenerateFinishPoints(startPoint);
        curveType = CurveType.FinishLine;
        GenerateCurveStats();
    }

    private List<CurvePoint> GenerateFinishPoints(CurvePoint startPoint)
    {
        List<CurvePoint> curvePoints = new();
        curvePoints.Add(new(new(0,0), startPoint.LeftTangent, -startPoint.LeftTangent));
        curvePoints.Add(SecondPoint(curvePoints[0]));
        curvePoints.Add(ThirdPoint(curvePoints[1]));
        _highpoint = curvePoints[0].ControlPoint;
        _lowpoint = curvePoints[1].ControlPoint;
        return curvePoints;
    }

    private CurvePoint SecondPoint(CurvePoint startPoint)
    {
        float finishY = Mathf.Max(Mathf.Abs(startPoint.RightTangent.y), 12);
        Vector3 location = new Vector3(startPoint.ControlPoint.x + finishY * 3, startPoint.ControlPoint.y - finishY);
        CurvePoint point = new(location, new Vector3(-6, 0), new Vector3(6, 0));
        return point;
    }

    private CurvePoint ThirdPoint(CurvePoint startPoint)
    {
        Vector3 location = new Vector3(startPoint.ControlPoint.x + 500, startPoint.ControlPoint.y);
        CurvePoint point = new(location, new Vector3(-6, 0), new Vector3(6, 0));
        return point;
    }
}
