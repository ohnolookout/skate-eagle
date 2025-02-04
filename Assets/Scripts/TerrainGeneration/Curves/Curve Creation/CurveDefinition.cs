using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveDefinition
{
    public List<ICurveSection> curveSections;
    public string name;

    public List<CurvePoint> GenerateCurvePoints(Vector2 startingTangent)
    {
        List<CurvePoint> curvePoints = new();

        //Initialize startPoint with tangents of previous point
        CurvePoint startPoint = new(new(0, 0), -startingTangent, startingTangent);
        foreach (var section in curveSections)
        {
            var sectionParams = section.GetSectionParameters(startPoint.RightTangent);

            if (curvePoints.Count == 0)
            {
                curvePoints = CalculateCurvePointPair(sectionParams, startPoint);
                /*
                _highpoint = curvePoints[0].ControlPoint;
                _lowpoint = _highpoint;
                EvaluateHighLow(BezierMath.GetMidpoint(curvePoints[0], curvePoints[1]));*/
            }
            else
            {
                List<CurvePoint> additionalCurvePoints = CalculateCurvePointPair(sectionParams, startPoint);

                curvePoints[^1] = additionalCurvePoints[0];
                curvePoints.Add(additionalCurvePoints[1]);

                //EvaluateHighLow(BezierMath.GetMidpoint(additionalCurvePoints[0], additionalCurvePoints[1]));
            }
            startPoint = curvePoints[^1];
        }

        return curvePoints;
    }

    public List<CurvePoint> CalculateCurvePointPair(CurveSectionParameters sectionParams, CurvePoint startPoint)
    {
        List<CurvePoint> curvePoints = new();

        Vector3 prevTangent = -startPoint.LeftTangent.normalized;
        CurvePoint nextPoint = new();
        nextPoint.ControlPoint = startPoint.ControlPoint + new Vector3(sectionParams.Length, sectionParams.Climb);
        nextPoint.SetTangents(sectionParams.Pitch, 1);

        Vector3 middleVertex = BezierMath.CalculateThirdVertexFromCurvePoints(startPoint, nextPoint);
        float firstMaxMagnitude = (middleVertex - startPoint.ControlPoint).magnitude * 1.1f;
        float secondMaxMagnitude = (nextPoint.ControlPoint - middleVertex).magnitude * 1.1f;
        float firstMagnitude = Mathf.Min(Mathf.Max(sectionParams.Shape * firstMaxMagnitude, startPoint.LeftTangent.magnitude * 0.5f), startPoint.LeftTangent.magnitude * 3f);
        float secondMagnitude = sectionParams.Shape * secondMaxMagnitude;

        startPoint.RightTangent = prevTangent * firstMagnitude;
        nextPoint.LeftTangent = nextPoint.LeftTangent.normalized * secondMagnitude;
        curvePoints.Add(startPoint);
        curvePoints.Add(nextPoint);

        return curvePoints;
    }
}
