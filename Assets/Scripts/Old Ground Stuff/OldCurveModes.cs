using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public static class OldCurveModes
{
    //return Vector3[] containing relative positions for control points and tangents of a roller-style curve
    //index 0 is start point, beginning at a random x relative to the last point. index 1 and 2 are left and right vectors, index 3 is next point, etc.
    //parameters are used to define range of potential values.
    //Starts with downcurve then upcurve.

    public static OldCurve RollerCurve(CurvePoint lastPoint, bool includeStart = false, float lengthMult = 1, float amplitudeMult = 1, float declineMult = 1)
    {
        OldCurve curve = new OldCurve(lastPoint);
        int hillStatus = 1;
        Vector3 prevTangent = lastPoint.RightTangent;
        if (includeStart) curve.AddPoint(lastPoint);
        for (int i = 0; i < 2; i++)
        {
            CurvePoint nextPoint = new CurvePoint();
            float prevTangSpacer = prevTangent.x + Mathf.Abs(prevTangent.y);
            float xDelta = Random.Range(20 + prevTangSpacer, 50 + prevTangSpacer / 2);
            float yDelta = Random.Range(-5, 3);
            float xVelocity = Random.Range(5 * (xDelta / 30), (10 * (xDelta / (35 + prevTangSpacer / 4))));
            float randomSlope = Random.Range(0.4f * (xVelocity / 6), 1.6f * (xVelocity / 10)) * hillStatus;
            nextPoint.ControlPoint = lastPoint.ControlPoint + new Vector3(xDelta, yDelta, 0);
            nextPoint.SetTangents(randomSlope, xVelocity);
            hillStatus *= -1;
            curve.AddPoint(nextPoint);
            lastPoint = nextPoint;
        }
        return curve;
    }

    public static OldCurve SmallRollerCurve(CurvePoint lastPoint, bool includeStart = false, float lengthMult = 1, float amplitudeMult = 1, float declineMult = 1)
    {
        OldCurve curve = new OldCurve(lastPoint);
        int hillStatus = 1;
        Vector3 prevTangent = lastPoint.RightTangent;
        if (includeStart) curve.AddPoint(lastPoint);
        for (int i = 0; i < 2; i++)
        {
            CurvePoint nextPoint = new CurvePoint();
            float prevTangSpacer = prevTangent.x + Mathf.Abs(prevTangent.y);
            float xDelta = Random.Range(6 + prevTangSpacer, 24 + prevTangSpacer / 2);
            float yDelta = Random.Range(-4, 2);
            float xVelocity = Random.Range(3 * (xDelta / 10), (6 * (xDelta / (15 + prevTangSpacer / 4))));
            float randomSlope = Random.Range(0.2f * (xVelocity / 4), 1f * (xVelocity / 6)) * hillStatus;
            nextPoint.ControlPoint = lastPoint.ControlPoint + new Vector3(xDelta, yDelta, 0);
            nextPoint.SetTangents(randomSlope, xVelocity);
            hillStatus *= -1;
            curve.AddPoint(nextPoint);
            lastPoint = nextPoint;
        }
        return curve;
    }

    public static OldCurve InitialCurve(SpriteShapeController groundControl, Transform eagle)
    {
        GameObject[] playerObj = GameObject.FindGameObjectsWithTag("Player");
        if (playerObj.Length < 1) eagle = Camera.main.transform;
        else eagle = GameObject.FindGameObjectWithTag("Player").transform;
        groundControl.transform.position = eagle.position - new Vector3(400, -150);
        CurvePoint startPoint = new CurvePoint();
        startPoint.ControlPoint = eagle.position;
        OldCurve curve = new OldCurve(startPoint);
        float xVelocity = 4;
        float slope = 1.5f;
        CurvePoint firstPoint = new CurvePoint();
        firstPoint.ControlPoint = new Vector2(0, 0);
        firstPoint.LeftTangent = new Vector2(0, -1);
        firstPoint.RightTangent = new Vector2(40, -130);
        curve.AddPoint(firstPoint);
        CurvePoint secondPoint = new CurvePoint();
        if(playerObj.Length >= 1) secondPoint.ControlPoint = new Vector3(400, -eagle.gameObject.GetComponent<SpriteRenderer>().bounds.size.y/2 - 1.1f - 150);
        else secondPoint.ControlPoint = new Vector3(400, eagle.position.y - 150);
        secondPoint.LeftTangent = new Vector2(-45, 0.5f);
        secondPoint.RightTangent = new Vector2(10, -0.5f);
        curve.AddPoint(secondPoint);
        CurvePoint thirdPoint = new CurvePoint();
        xVelocity = 10;
        slope = -1.1f;
        thirdPoint.ControlPoint = secondPoint.ControlPoint + new Vector3(30, -12);
        thirdPoint.LeftTangent = new Vector3(-xVelocity, -xVelocity * slope);
        thirdPoint.RightTangent = new Vector3(xVelocity, xVelocity * slope);
        curve.AddPoint(thirdPoint);

        return curve;
    }

    public static OldCurve FinishLine(CurvePoint lastPoint, bool includeStart = false)
    {
        OldCurve curve = new OldCurve(lastPoint);
        Vector3 lastRightTangent = lastPoint.RightTangent;
        if (includeStart) curve.AddPoint(lastPoint);
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
        for(int i = 0; i < 2; i++)
        {
            CurvePoint newPoint = new CurvePoint();
            if(i == 0)
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
            curve.AddPoint(newPoint);
        }
        return curve;
    }


}
