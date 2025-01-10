using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class FixedCurve : Curve
{
    public List<FixedHalfCurve> halfCurves;
    public FixedCurve(List<FixedHalfCurve> halfCurves, CurvePoint start)
    {
        curveType = CurveType.Fixed;
        startPoint = start;
        curvePoints = CurvePointsFromHalfCurves(halfCurves);

    }

    private List <CurvePoint> CurvePointsFromHalfCurves(List<FixedHalfCurve> halfCurves)
    {
        List<CurvePoint> curvePoints = new();
        for (int i = 0; i < halfCurves.Count; i++)
        {
            if (i == 0)
            {
                curvePoints = CalculateCurvePointPair(halfCurves[i], startPoint);
                _highpoint = curvePoints[0].ControlPoint;
                _lowpoint = _highpoint;
                EvaluateHighLow(BezierMath.GetMidpoint(curvePoints[0], curvePoints[1]));
            }
            else
            {
                List<CurvePoint> additionalCurvePoints = CalculateCurvePointPair(halfCurves[i], startPoint);
                curvePoints[^1] = additionalCurvePoints[0];
                curvePoints.Add(additionalCurvePoints[1]);
                EvaluateHighLow(BezierMath.GetMidpoint(additionalCurvePoints[0], additionalCurvePoints[1]));
            }
            startPoint = curvePoints[^1];
        }
        return curvePoints;
    }

    private List<CurvePoint> CalculateCurvePointPair(FixedHalfCurve halfCurve, CurvePoint startPoint)
    {
        Debug.Log("Calculating curve point pair, startPoint right tangent: " + startPoint.RightTangent);

        List<CurvePoint> curvePoints = new();

        float slope = halfCurve.Type == HalfCurveType.Peak ? halfCurve.Slope : halfCurve.Slope * -1;

        CurvePoint nextPoint = new();
        nextPoint.ControlPoint = startPoint.ControlPoint + new Vector3(halfCurve.Length, halfCurve.Climb);
        nextPoint.SetTangents(slope, 1);

        Vector3 middleVertex = BezierMath.CalculateThirdVertexFromCurvePoints(startPoint, nextPoint);
        float firstMaxMagnitude = (middleVertex - startPoint.ControlPoint).magnitude * 1.1f;
        float secondMaxMagnitude = (nextPoint.ControlPoint - middleVertex).magnitude * 1.1f;
        float firstMagnitude = Mathf.Min(Mathf.Max(halfCurve.Shape * firstMaxMagnitude, startPoint.LeftTangent.magnitude * 0.5f), startPoint.LeftTangent.magnitude * 3f);
        float secondMagnitude = halfCurve.Shape * secondMaxMagnitude;

        Vector3 prevTangent = -startPoint.LeftTangent.normalized;
        startPoint.RightTangent = prevTangent * firstMagnitude;
        nextPoint.LeftTangent = nextPoint.LeftTangent.normalized * secondMagnitude;
        curvePoints.Add(startPoint);
        curvePoints.Add(nextPoint);


        return curvePoints;
    }

    private void EvaluateHighLow(Vector3 newPoint)
    {
        if (newPoint.y >= _highpoint.y)
        {
            _highpoint = newPoint;
        }
        else if (newPoint.y <= _lowpoint.y)
        {
            _lowpoint = newPoint;
        }
    }
}
