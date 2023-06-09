using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveTypes
{
    public static Dictionary<string, CurveParameters> SavedCurveParameters = new();
    public static List<CurvePoint> Roller(CurvePoint startPoint, float lengthMult = 1, float amplitudeMult = 1, float declineMult = 1)
    {
        List<CurvePoint> curve = new();
        int hillStatus = 1;
        Vector3 prevTangent = startPoint.RightTangent;
        curve.Add(startPoint);
        for (int i = 0; i < 2; i++)
        {
            CurvePoint nextPoint = new CurvePoint();
            float prevTangSpacer = prevTangent.x + Mathf.Abs(prevTangent.y);
            float xDelta = Random.Range(20 + prevTangSpacer, 50 + prevTangSpacer / 2);
            float yDelta = Random.Range(-5, 3);
            float xVelocity = Random.Range(5 * (xDelta / 30), (10 * (xDelta / (35 + prevTangSpacer / 4))));
            float randomSlope = Random.Range(0.4f * (xVelocity / 6), 1.6f * (xVelocity / 10)) * hillStatus;
            nextPoint.ControlPoint = startPoint.ControlPoint + new Vector3(xDelta, yDelta, 0);
            nextPoint.SetTangents(randomSlope, xVelocity);
            hillStatus *= -1;
            curve.Add(nextPoint);
            startPoint = nextPoint;
        }
        return curve;
    }

    public static List<CurvePoint> SmallRoller(CurvePoint startPoint, float lengthMult = 1, float amplitudeMult = 1, float declineMult = 1)
    {
        List<CurvePoint> curve = new();
        int hillStatus = 1;
        Vector3 prevTangent = startPoint.RightTangent;
        curve.Add(startPoint);
        for (int i = 0; i < 2; i++)
        {
            CurvePoint nextPoint = new CurvePoint();
            float prevTangSpacer = prevTangent.x + Mathf.Abs(prevTangent.y);
            float xDelta = Random.Range(7 + prevTangSpacer, 24 + prevTangSpacer / 2);
            float yDelta = Random.Range(-4, 2);
            float xVelocity = Random.Range(3 * (xDelta / 10), (6 * (xDelta / (15 + prevTangSpacer / 4))));
            float randomSlope = Random.Range(0.2f * (xVelocity / 4), 1f * (xVelocity / 6)) * hillStatus;
            nextPoint.ControlPoint = startPoint.ControlPoint + new Vector3(xDelta, yDelta, 0);
            nextPoint.SetTangents(randomSlope, xVelocity);
            hillStatus *= -1;
            curve.Add(nextPoint);
            startPoint = nextPoint;
        }
        return curve;
    }

    public static List<CurvePoint> StartLine()
    {
        Transform eagle;
        List<CurvePoint> curve = new();
        GameObject[] playerObj = GameObject.FindGameObjectsWithTag("Player");
        if (playerObj.Length < 1) eagle = Camera.main.transform;
        else eagle = playerObj[0].transform;
        Vector3 startLocation = new Vector3(eagle.position.x - 400, eagle.position.y + 150);
        CurvePoint firstPoint = new CurvePoint(startLocation, new Vector2(0, -1), new Vector2(40, -130));
        curve.Add(firstPoint);
        Vector3 secondLocation = new Vector3(0, 0); ;
        if (playerObj.Length >= 1) secondLocation = new Vector3(eagle.position.x, eagle.position.y - eagle.gameObject.GetComponent<SpriteRenderer>().bounds.size.y / 2 - 1.1f);
        CurvePoint secondPoint = new CurvePoint(secondLocation, new Vector2(-45, 0.5f), new Vector2(10, -0.5f));
        curve.Add(secondPoint);
        CurvePoint thirdPoint = new CurvePoint();
        float xVelocity = 10;
        float slope = -1.1f;
        thirdPoint.ControlPoint = secondPoint.ControlPoint + new Vector3(30, -12);
        thirdPoint.LeftTangent = new Vector3(-xVelocity, -xVelocity * slope);
        thirdPoint.RightTangent = new Vector3(xVelocity, xVelocity * slope);
        curve.Add(thirdPoint);

        return curve;
    }

    public static List<CurvePoint> FinishLine(CurvePoint lastPoint)
    {
        List<CurvePoint> curve = new();
        Vector3 lastRightTangent = lastPoint.RightTangent;
        curve.Add(lastPoint);
        //Set right tangent for last point.
        float finishY;
        if (Mathf.Abs(lastRightTangent.y) > 2)
        {
            finishY = Mathf.Abs(lastRightTangent.y);
        }
        else
        {
            finishY = 3;
        }
        //Calculate y for first point based on last point + tangent
        //Calculate x as 2x y difference
        //y for second point is the same
        for (int i = 0; i < 2; i++)
        {
            CurvePoint newPoint = new CurvePoint();
            if (i == 0)
            {
                newPoint.ControlPoint = new Vector3(lastPoint.ControlPoint.x + finishY * 3, lastPoint.ControlPoint.y - finishY);
                lastPoint = newPoint;
            }
            else
            {
                newPoint.ControlPoint = new Vector3(lastPoint.ControlPoint.x + 500, lastPoint.ControlPoint.y);
            }
            newPoint.LeftTangent = new Vector3(-2, 0);
            newPoint.RightTangent = new Vector3(2, 0);
            curve.Add(newPoint);
        }
        return curve;
    }

    public static List<CurvePoint> CustomCurve(CurveParameters parameters, CurvePoint startPoint, float lengthMult = 1, float amplitudeMult = 1, float declineMult = 1)
    {

        List<CurvePoint> curve = new();
        int hillStatus = 1;
        curve.Add(startPoint);
        for (int i = 0; i < 2; i++)
        {
            Vector3 prevTangent = startPoint.RightTangent;
            CurvePoint nextPoint = new CurvePoint();
            float prevTangSpacer = prevTangent.x + Mathf.Abs(prevTangent.y)/3;
            float xDelta = Random.Range(parameters.xDeltaMin + prevTangSpacer, parameters.xDeltaMax + prevTangSpacer / 2);
            float yDelta = Random.Range(parameters.yDeltaMin, parameters.yDeltaMax);
            float xVelocity = Random.Range(parameters.xVelocityMin + (xDelta/3), parameters.xVelocityMax + (xDelta /3) + prevTangSpacer / 4);
            float randomSlope = Random.Range(parameters.slopeMin * (xVelocity / 20), parameters.slopeMax * (xVelocity / 6) * hillStatus);
            nextPoint.ControlPoint = startPoint.ControlPoint + new Vector3(xDelta, yDelta, 0);
            nextPoint.SetTangents(randomSlope, xVelocity);
            hillStatus *= -1;
            curve.Add(nextPoint);
            startPoint = nextPoint;
        }
        return curve;
    }

    public static List<CurvePoint> FixedCustomCurve(CurveParameters parameters, CurvePoint startPoint, bool useMinimumParameters, float lengthMult = 1, float amplitudeMult = 1, float declineMult = 1)
    {

        List<CurvePoint> curve = new();
        int hillStatus = 1;
        Vector3 prevTangent = startPoint.RightTangent;
        curve.Add(startPoint);

        for (int i = 0; i < 2; i++)
        {
            CurvePoint nextPoint = new CurvePoint();
            float prevTangSpacer = prevTangent.x + Mathf.Abs(prevTangent.y);
            float xDelta, yDelta, xVelocity, slope;
            if (useMinimumParameters)
            {
                xDelta = parameters.xDeltaMin + prevTangSpacer;
                yDelta = parameters.yDeltaMin;
                xVelocity = parameters.xVelocityMin * (xDelta / 10);
                slope = parameters.slopeMin * hillStatus;
            }
            else
            {
                xDelta = parameters.xDeltaMax + prevTangSpacer / 2;
                yDelta = parameters.yDeltaMax;
                xVelocity = parameters.xVelocityMax * (xDelta / (15 + prevTangSpacer / 4));
                slope = parameters.slopeMax * (xVelocity / 6) * hillStatus;
            } 
            nextPoint.ControlPoint = startPoint.ControlPoint + new Vector3(xDelta, yDelta, 0);
            nextPoint.SetTangents(slope, xVelocity);
            hillStatus *= -1;
            curve.Add(nextPoint);
            startPoint = nextPoint;
        }
        return curve;
    }

    public static List<CurvePoint> GenerateCurveList(CurveType type, CurvePoint startPoint, float lengthMult = 1, float amplitudeMult = 1, float declineMult = 1)
    {
        return type switch
        {
            CurveType.StartLine => CurveTypes.StartLine(),
            CurveType.FinishLine => CurveTypes.FinishLine(startPoint),
            CurveType.Roller => CurveTypes.Roller(startPoint, lengthMult, amplitudeMult, declineMult),
            CurveType.SmallRoller => CurveTypes.SmallRoller(startPoint, lengthMult, amplitudeMult, declineMult),
            _ => CurveTypes.Roller(startPoint, lengthMult, amplitudeMult, declineMult),
        };
    }

    public static List<CurvePoint> GenerateStartLine()
    {
        return CurveTypes.StartLine();
    }
}
