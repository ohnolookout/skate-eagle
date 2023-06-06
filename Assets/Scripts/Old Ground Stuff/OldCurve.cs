using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldCurve
{
    private float length = 0;
    private Dictionary<int, CurvePoint> curvePoints = new Dictionary<int, CurvePoint>();
    private int lastLengthCount = 0;
    private CurvePoint startPoint;

    public OldCurve(CurvePoint start)
    {
        startPoint = start;
    }
    // Calculates a point on a bezier curve using the given control points and tangents at time t (0 <= t <= 1)
    private static Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
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

    public void AddPoint(CurvePoint point)
    {
        int i = curvePoints.Count;
        curvePoints[i] = point;
    }

    public CurvePoint GetPoint(int i)
    {
        return curvePoints[i];
    }

    public int Count
    {
        get
        {
            return curvePoints.Count;
        }
    }
    public float CurveLength
    {
        get
        {
            if (lastLengthCount == curvePoints.Count)
            {
                return length;
            }
            else
            {
                length = 0;
                for (int i = 0; i < curvePoints.Count - 1; i++)
                {
                    length += GetCurveLength(curvePoints[i].ControlPoint, curvePoints[i].RightTangent, curvePoints[i + 1].LeftTangent, curvePoints[i + 1].ControlPoint);
                }
                lastLengthCount = curvePoints.Count;
                Debug.Log($"Length: {length}");
                return length;
            }
        }
    }
    public static float GetCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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
}
