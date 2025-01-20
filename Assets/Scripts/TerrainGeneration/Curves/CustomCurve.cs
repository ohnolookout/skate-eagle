using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCurve : Curve
{
    CurveDefinition _curveDef;

    public CustomCurve(CurveDefinition curveDef, CurvePoint startPoint)
    {
        _curveDef = curveDef;
        //Use inverted left tangent from last point as starting right tangent
        curvePoints = CurvePointsFromDefinition(curveDef, -startPoint.LeftTangent);
        curveType = CurveType.Custom;
        GenerateCurveStats();

    }

    private List<CurvePoint> CurvePointsFromDefinition(CurveDefinition curveDef, Vector2 prevTang)
    {
        List<CurvePoint> curvePoints = new();

        //Initialize startPoint with tangents of previous point
        CurvePoint startPoint = new(new(0, 0), -prevTang, prevTang);
        for (int i = 0; i < curveDef.Definitions.Length; i++)
        {
            CurveSectionParameters sectionParams = curveDef.Definitions[i].GetSectionParameters(startPoint.RightTangent);
            
            if (i == 0)
            {
                curvePoints = CalculateCurvePointPair(sectionParams, startPoint);
                _highpoint = curvePoints[0].ControlPoint;
                _lowpoint = _highpoint;
                EvaluateHighLow(BezierMath.GetMidpoint(curvePoints[0], curvePoints[1]));
            }
            else
            {
                List<CurvePoint> additionalCurvePoints = CalculateCurvePointPair(sectionParams, startPoint);

                curvePoints[^1] = additionalCurvePoints[0];
                curvePoints.Add(additionalCurvePoints[1]);
                EvaluateHighLow(BezierMath.GetMidpoint(additionalCurvePoints[0], additionalCurvePoints[1]));
            }
            startPoint = curvePoints[^1];
        }

        return curvePoints;
    }

    private List<CurvePoint> CalculateCurvePointPair(CurveSectionParameters sectionParams, CurvePoint startPoint)
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
    
    private void EvaluateHighLow(Vector3 newPoint)
    {
        if(newPoint.y >= _highpoint.y)
        {
            _highpoint = newPoint;
        } else if(newPoint.y <= _lowpoint.y)
        {
            _lowpoint = newPoint;
        }
    }    

}
