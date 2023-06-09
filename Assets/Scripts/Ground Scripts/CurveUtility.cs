using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public static class CurveUtility
{
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
