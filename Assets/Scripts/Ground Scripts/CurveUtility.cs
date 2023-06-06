using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public static class CurveUtility
{
    public static List<CurvePoint> GenerateCurveList(CurveType type, CurvePoint startPoint, float lengthMult = 1, float amplitudeMult = 1, float declineMult = 1)
    {
        return type switch
        {
            CurveType.StartLine => StartLine(),
            CurveType.FinishLine => FinishLine(startPoint),
            CurveType.Roller => Roller(startPoint, lengthMult, amplitudeMult, declineMult),
            CurveType.SmallRoller => SmallRoller(startPoint, lengthMult, amplitudeMult, declineMult),
            _ => Roller(startPoint, lengthMult, amplitudeMult, declineMult),
        };
    }

    public static List<CurvePoint> GenerateStartLine()
    {
        return StartLine();
    }
    //return Vector3[] containing relative positions for control points and tangents of a roller-style curve
    //index 0 is start point, beginning at a random x relative to the last point. index 1 and 2 are left and right vectors, index 3 is next point, etc.
    //parameters are used to define range of potential values.
    //Starts with downcurve then upcurve.

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
            float xDelta = Random.Range(6 + prevTangSpacer, 24 + prevTangSpacer / 2);
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

    public static float GetCurveLength(List<CurvePoint> curvePoints, out List<float> segmentLengths)
    {
        segmentLengths = new();
        float length = 0;
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            segmentLengths.Add(SingleCurveLength(curvePoints[i].ControlPoint, curvePoints[i].RightTangent, curvePoints[i + 1].LeftTangent, curvePoints[i + 1].ControlPoint));
            length += segmentLengths[^1];
        }
        return length;
    }

    public static float SingleCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        p1 += p0;
        p2 += p3;
        float step = 0.05f; // smaller value will give more precise results but slower performance
        float length = 0f;
        Vector3 prevPoint = p0;
        for (float t = step; t <= 1f; t += step)
        {
            Vector3 point = CalculateBezierPoint(p0, p1, p2, p3, t);
            length += Vector3.Distance(prevPoint, point);
            prevPoint = point;
        }
        return length;
    }
    public static Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0;
        point += 3f * uu * t * p1;
        point += 3f * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    public static Vector3 FindLowPoint(List<CurvePoint> curvePoints)
    {
        for(int i = 0; i < curvePoints.Count - 1; i++)
        {
            if(curvePoints[i].RightTangent.y < 0)
            {
                return GetPointAtT(curvePoints[i], curvePoints[i + 1], 0.5f);
            }
        }
        CurvePoint lowPoint = curvePoints[0];
        for(int i = 1; i < curvePoints.Count; i++)
        {
            if (curvePoints[i].ControlPoint.y < lowPoint.ControlPoint.y) lowPoint = curvePoints[i];
        }
        return lowPoint.ControlPoint;
    }

    public static Vector3 GetPointAtT(CurvePoint startPoint, CurvePoint endPoint, float t)
    {
        Vector3 p0 = startPoint.ControlPoint;
        Vector3 rt = p0 + startPoint.RightTangent;
        Vector3 p1 = endPoint.ControlPoint;
        Vector3 lt = p1 + endPoint.LeftTangent;
        return BezierUtility.BezierPoint(p0, rt, lt, p1, t);
    }

    public static void InsertCurve(SpriteShapeController controller, Curve curve, int index) //Inserts curve into the spline beginning at the given index
    {
        for (int i = 0; i < curve.Count; i++)
        {
            InsertCurvePoint(controller, curve.GetPoint(i), index);
            index++;
        }
        controller.spline.SetTangentMode(index, ShapeTangentMode.Broken);
        controller.spline.SetRightTangent(index, new Vector3(0, -1));

    }

    public static void InsertCurvePoint(SpriteShapeController controller, CurvePoint curvePoint, int index) //Inserts curvePoint at a given index
    {
        Spline spline = controller.spline;
        spline.InsertPointAt(index, curvePoint.ControlPoint);
        spline.SetTangentMode(index, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(index, curvePoint.LeftTangent);
        spline.SetRightTangent(index, curvePoint.RightTangent);
    }

}
