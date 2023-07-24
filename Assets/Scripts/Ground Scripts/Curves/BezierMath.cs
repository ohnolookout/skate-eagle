using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public static class BezierMath
{

    public static float Length(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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

    public static Vector3 GetPointAtT(CurvePoint startPoint, CurvePoint endPoint, float t)
    {
        Vector3 p0 = startPoint.ControlPoint;
        Vector3 rt = p0 + startPoint.RightTangent;
        Vector3 p1 = endPoint.ControlPoint;
        Vector3 lt = p1 + endPoint.LeftTangent;
        return BezierUtility.BezierPoint(p0, rt, lt, p1, t);
    }

    public static float CalculateAngle(Vector3 vertex, Vector3 firstRay, Vector3 secondRay)
    {
        // Calculate the direction vectors of the rays
        Vector3 dir1 = (firstRay - vertex).normalized;
        Vector3 dir2 = (secondRay - vertex).normalized;

        // Calculate the dot product of the direction vectors
        float dotProduct = Vector3.Dot(dir1, dir2);

        // Calculate the angle in radians using the dot product
        float angleRad = Mathf.Acos(dotProduct);

        // Convert the angle from radians to degrees
        float angleDeg = angleRad * Mathf.Rad2Deg;

        return angleRad;
    }

    public static Vector3 CalculateThirdVertex(Vector3 firstVertex, Vector3 firstTangent, Vector3 secondVertex, Vector3 secondTangent)
    {
        float firstAngle = CalculateAngle(firstVertex, secondVertex, firstTangent);
        float secondAngle = CalculateAngle(secondVertex, firstVertex, secondTangent);
        float thirdAngle = 3.14159f - firstAngle - secondAngle;
        float thirdSideLength = (secondVertex - firstVertex).magnitude;
        float secondSideLength = (Mathf.Sin(secondAngle) * thirdSideLength) / Mathf.Sin(thirdAngle);
        Vector3 secondSideDirection = (firstTangent - firstVertex).normalized;
        return firstVertex + secondSideLength * secondSideDirection;
    }

    public static Vector3 CalculateThirdVertexFromCurvePoints(CurvePoint firstPoint, CurvePoint secondPoint)
    {
        
        return CalculateThirdVertex(firstPoint.ControlPoint, firstPoint.ControlPoint + firstPoint.RightTangent, 
            secondPoint.ControlPoint, secondPoint.ControlPoint + secondPoint.LeftTangent);
    }


}
