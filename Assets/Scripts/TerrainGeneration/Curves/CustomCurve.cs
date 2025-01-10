using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCurve : Curve
{
    int hillStatus = 1;
    float _climbMin, _climbMax;
    CurvePoint _startPoint;
    CurveDefinition _curveDef;

    public CustomCurve(CurveDefinition curveDef, CurvePoint startPoint, float climbMin, float climbMax)
    {
        _startPoint = startPoint;
        _climbMin = climbMin;
        _climbMax = climbMax;
        _curveDef = curveDef;
        curvePoints = CurvePointsFromDefinition(curveDef);
        curveType = CurveType.Custom;
        GenerateCurveStats();

    }

    private List<CurvePoint> CurvePointsFromDefinition(CurveDefinition curveDef)
    {
        List<CurvePoint> curvePoints = new();
        for (int i = 0; i < curveDef.Definitions.Length; i++)
        {
            ProceduralCurveSectionParams curveParams = new(curveDef.Definitions[i]);
            if (i == 0)
            {
                curvePoints = CalculateCurvePointPair(curveParams, _startPoint, _climbMin, _climbMax);
                _highpoint = curvePoints[0].ControlPoint;
                _lowpoint = _highpoint;
                EvaluateHighLow(BezierMath.GetMidpoint(curvePoints[0], curvePoints[1]));
            }
            else
            {
                List<CurvePoint> additionalCurvePoints = CalculateCurvePointPair(curveParams, startPoint, _climbMin, _climbMax);
                curvePoints[^1] = additionalCurvePoints[0];
                curvePoints.Add(additionalCurvePoints[1]);
                EvaluateHighLow(BezierMath.GetMidpoint(additionalCurvePoints[0], additionalCurvePoints[1]));
            }
            hillStatus *= -1;
            startPoint = curvePoints[^1];
        }
        return curvePoints;
    }

    private List<CurvePoint> CalculateCurvePointPair(ProceduralCurveSectionParams parameters, CurvePoint startPoint, float climbMin, float climbMax)
    {
        List<CurvePoint> curvePoints = new();
        Vector3 prevTangent = -startPoint.LeftTangent.normalized;
        float prevSlope = Mathf.Abs(prevTangent.y / prevTangent.x);
        float prevTangSpacer = prevTangent.x + Mathf.Abs(prevTangent.y) / 3;
        float length = Random.Range(parameters.lengthMin + prevTangSpacer, parameters.lengthMax + prevTangSpacer);
        float climb = Random.Range(climbMin, climbMax);
        float grade = climb / length;
        float adjustedSlopeMin = Mathf.Max(parameters.steepMin, Mathf.Abs(prevSlope * 0.4f)) + (grade * hillStatus);
        float adjustedSlopeMax = Mathf.Min(parameters.steepMax, Mathf.Abs(prevSlope * 2f)) + (grade * hillStatus);
        float slope = Random.Range(adjustedSlopeMin, adjustedSlopeMax) * hillStatus;

        CurvePoint nextPoint = new();
        nextPoint.ControlPoint = startPoint.ControlPoint + new Vector3(length, climb);
        nextPoint.SetTangents(slope, 1);

        Vector3 middleVertex = BezierMath.CalculateThirdVertexFromCurvePoints(startPoint, nextPoint);
        float firstMaxMagnitude = (middleVertex - startPoint.ControlPoint).magnitude * 1.1f;
        float secondMaxMagnitude = (nextPoint.ControlPoint - middleVertex).magnitude * 1.1f;
        float round = Random.Range(parameters.roundMin, parameters.roundMax);
        float firstMagnitude = Mathf.Min(Mathf.Max(round * firstMaxMagnitude, startPoint.LeftTangent.magnitude * 0.5f), startPoint.LeftTangent.magnitude * 3f);
        float secondMagnitude = round * secondMaxMagnitude;

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
