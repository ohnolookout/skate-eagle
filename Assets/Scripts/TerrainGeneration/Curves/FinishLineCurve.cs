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
        Vector3 lastRightTangent = -startPoint.LeftTangent;
        startPoint.RightTangent = lastRightTangent;
        curvePoints.Add(startPoint);
        //Set right tangent for last point.
        float finishY = Mathf.Max(Mathf.Abs(lastRightTangent.y), 12);
        for (int i = 0; i < 2; i++)
        {
            CurvePoint newPoint = new CurvePoint();
            if (i == 0)
            {
                newPoint.ControlPoint = new Vector3(startPoint.ControlPoint.x + finishY * 3, startPoint.ControlPoint.y - finishY);
                startPoint = newPoint;
            }
            else
            {
                newPoint.ControlPoint = new Vector3(startPoint.ControlPoint.x + 500, startPoint.ControlPoint.y);
            }
            newPoint.LeftTangent = new Vector3(-6, 0);
            newPoint.RightTangent = new Vector3(6, 0);
            curvePoints.Add(newPoint);

        }
        return curvePoints;
    }
}
