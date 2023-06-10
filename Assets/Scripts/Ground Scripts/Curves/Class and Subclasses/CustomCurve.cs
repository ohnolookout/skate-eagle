using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCurve : Curve
{
    // Start is called before the first frame update
    public CustomCurve(CurveParameters parameters, CurvePoint startPoint, float yDeltaMin = 0, float yDeltaMax = 0)
    {
        curvePoints = CurvePointsFromParameters(parameters, startPoint, yDeltaMin, yDeltaMax);
        curveType = CurveType.Custom;
        GenerateCurveStats();
    }

    public static List<CurvePoint> CurvePointsFromParameters(CurveParameters parameters, CurvePoint startPoint, float yDeltaMin = 0, float yDeltaMax = 0, float lengthMult = 1, float amplitudeMult = 1, float declineMult = 1)
    {

        List<CurvePoint> curvePoints = new();
        int hillStatus = 1;
        curvePoints.Add(startPoint);
        for (int i = 0; i < 2; i++)
        {
            Vector3 prevTangent = startPoint.RightTangent;
            CurvePoint nextPoint = new CurvePoint();
            float prevTangSpacer = prevTangent.x + Mathf.Abs(prevTangent.y) / 3;
            float xDelta = Random.Range(parameters.xDeltaMin + prevTangSpacer, parameters.xDeltaMax + prevTangSpacer);
            float yDelta = Random.Range(yDeltaMin, yDeltaMax);
            float xVelocity = Random.Range(xDelta * parameters.lengthToVelocityRatioMin, xDelta * parameters.lengthToVelocityRatioMax);
            float randomSlope = Random.Range(parameters.slopeMin, parameters.slopeMax) * hillStatus;
            nextPoint.ControlPoint = startPoint.ControlPoint + new Vector3(xDelta, yDelta, 0);
            nextPoint.SetTangents(randomSlope, xVelocity);
            hillStatus *= -1;
            curvePoints.Add(nextPoint);
            startPoint = nextPoint;
        }
        return curvePoints;
    }
}
