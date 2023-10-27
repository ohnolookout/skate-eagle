using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCurve : Curve
{
    int hillStatus = 1;
    float _climbMin, _climbMax;
    CurvePoint _startPoint;
    public CustomCurve(CurveParameters[] parameters, CurvePoint startPoint, float climbMin, float climbMax)
    {
        //Using array of CurveParameters2 because there will be a separate set of params for lower and upper parts of curve.
        curvePoints = CurvePointsFromParameters(parameters, startPoint, climbMin, climbMax);
        curveType = CurveType.Custom;
        GenerateCurveStats();
    }
    
    public CustomCurve(CurveDefinition curveDef, CurvePoint startPoint, float climbMin, float climbMax)
    {
        _startPoint = startPoint;
        _climbMin = climbMin;
        _climbMax = climbMax;
        curvePoints = CurvePointsFromDefinition(curveDef);
        curveType = CurveType.Custom;
        GenerateCurveStats();

    }

    private List<CurvePoint> CurvePointsFromDefinition(CurveDefinition curveDef)
    {
        List<CurvePoint> curvePoints = new();
        for (int i = 0; i < curveDef.Array.Length; i++)
        {
            CurveParameters curveParams = new(curveDef.Array[i]);
            if (i == 0)
            {
                curvePoints = SingleCurvePoints(curveParams, _startPoint, _climbMin, _climbMax);
            }
            else
            {
                List<CurvePoint> additionalCurvePoints = SingleCurvePoints(curveParams, startPoint, _climbMin, _climbMax);
                curvePoints[^1] = additionalCurvePoints[0];
                curvePoints.Add(additionalCurvePoints[1]);
            }
            hillStatus *= -1;
            startPoint = curvePoints[^1];
        }
        return curvePoints;
    }

    private List<CurvePoint> CurvePointsFromParameters(CurveParameters[] parameters, CurvePoint startPoint, float climbMin, float climbMax)
    {

        List<CurvePoint> curvePoints = new();
        for (int i = 0; i < parameters.Length; i++)
        {
            
            if(i == 0)
            {
                curvePoints = SingleCurvePoints(parameters[i], startPoint, climbMin, climbMax);
            }
            else
            {
                List<CurvePoint> additionalCurvePoints = SingleCurvePoints(parameters[i], startPoint, climbMin, climbMax);
                curvePoints[^1] = additionalCurvePoints[0];
                curvePoints.Add(additionalCurvePoints[1]);
            }
            hillStatus *= -1;
            startPoint = curvePoints[^1];
        }
        return curvePoints;
    }

    private List<CurvePoint> SingleCurvePoints(CurveParameters parameters, CurvePoint startPoint, float climbMin, float climbMax)
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

}
