using System;
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

    public static Vector3 GetMidpoint(CurvePoint startPoint, CurvePoint endPoint)
    {
        return Lerp(startPoint, endPoint, 0.5f);
    }

    public static Vector3 Lerp(CurvePoint startPoint, CurvePoint endPoint, float t)
    {

        Vector3 p0 = startPoint.Position;
        Vector3 rt = p0 + startPoint.RightTangent;
        Vector3 p1 = endPoint.Position;
        Vector3 lt = p1 + endPoint.LeftTangent;
        return CalculateBezierPoint(p0, rt, lt, p1, t);
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
        
        return CalculateThirdVertex(firstPoint.Position, firstPoint.Position + firstPoint.RightTangent, 
            secondPoint.Position, secondPoint.Position + secondPoint.LeftTangent);
    }

    public static Vector2 GetPointAlongLine(Vector2 startPoint, Vector2 slope, float magnitude)
    {
        // Normalize the slope to get the direction
        Vector2 direction = slope.normalized;

        // Scale by magnitude and add to startPoint
        return startPoint + direction * magnitude;
    }
    public static Vector2 GetPerpendicularIntersection(Vector2 startPoint, Vector2 endPoint, Vector2 targetPoint)
    {
        // Compute the direction vector of the line
        Vector2 lineUnit = (endPoint - startPoint).normalized;

        // Compute the vector from the start point to the target point
        Vector2 startToTarget = targetPoint - startPoint;

        // Project startToTarget onto the line direction
        float projectionLength = Vector2.Dot(startToTarget, lineUnit);

        // Compute the intersection point
        Vector2 intersection = startPoint + lineUnit * projectionLength;

        return intersection;
    }
    public static Vector2 GetParallelProjectionPoint(Vector2 startPoint, Vector2 endPoint, Vector2 targetPoint, float t)
    {
        // Compute the direction vector of the original line
        Vector2 lineDirection = endPoint - startPoint;

        // Normalize the direction vector
        Vector2 lineUnit = lineDirection.normalized;

        // Compute the perpendicular intersection of targetPoint onto l1
        Vector2 intersection = GetPerpendicularIntersection(startPoint, endPoint, targetPoint);

        // Compute the t% point along l1
        Vector2 tPointOnL1 = startPoint + lineDirection * t;

        // Compute the offset from targetPoint to the intersection
        Vector2 offset = targetPoint - intersection;

        // Compute the final return point on l2
        Vector2 returnPoint = tPointOnL1 + offset;

        return returnPoint;
    }
    public static float GetPerpendicularDistance(Vector2 position1, Vector2 position2, Vector2 point)
    {
        Vector2 lineDirection = position2 - position1;
        Vector2 pointVector = point - position1;
        float area = Mathf.Abs(lineDirection.x * pointVector.y - lineDirection.y * pointVector.x);
        float lineLength = lineDirection.magnitude;
        return area / lineLength;
    }

    public static Vector2? GetIntersection(CurvePoint startPoint, CurvePoint endPoint)
    {
        //return GetIntersection(startPoint.Position, startPoint.Position + startPoint.RightTangent, endPoint.Position, endPoint.Position + endPoint.LeftTangent);
        return GetIntersection(startPoint.Position, startPoint.RightTangent, endPoint.Position, endPoint.LeftTangent);
    }

    public static Vector2? GetIntersection(Vector2 position1, Vector2 tangent1, Vector2 position2, Vector2 tangent2)
    {

        float denominator = tangent1.x * tangent2.y - tangent1.y * tangent2.x;

        if (Mathf.Abs(denominator) < float.Epsilon)
        {
            // Rays are parallel or coincident
            return null;
        }

        Vector2 difference = position2 - position1;
        float t = (difference.x * tangent2.y - difference.y * tangent2.x) / denominator;
        float s = (difference.x * tangent1.y - difference.y * tangent1.x) / denominator;

        if (t >= 0 && s >= 0)
        {
            // The intersection point lies along the rays
            return position1 + t * tangent1;
        }

        return null; // No intersection within the valid ray directions
    }

    public static Vector2 GetTangentFromAngle(Vector2 startPoint, Vector2 endPoint, float angle, float magnitude)
    {
        // Calculate the direction vector from startPoint to endPoint
        Vector2 direction = (endPoint - startPoint).normalized;

        // Convert the angle to radians
        float radians = MathF.PI * angle / 180f;

        // Rotate the direction vector by the given angle
        float cosAngle = Mathf.Cos(radians);
        float sinAngle = Mathf.Sin(radians);
        Vector2 tangent = new Vector2(
            direction.x * cosAngle - direction.y * sinAngle,
            direction.x * sinAngle + direction.y * cosAngle
        );

        // Scale the tangent by the given magnitude
        return tangent * magnitude;
    }
    public static float GetAngleFromTangent(Vector2 startPoint, Vector2 endPoint, Vector2 tangent)
    {
        // Compute the direction vector of the line from startPoint to endPoint
        Vector2 lineDirection = (endPoint - startPoint).normalized;
        // Normalize the tangent vector
        Vector2 tangentDirection = tangent.normalized;

        // Compute the angle between the two vectors using the dot product
        float dotProduct = Vector2.Dot(lineDirection, tangentDirection);
        float angle = MathF.Acos(Mathf.Clamp(dotProduct, -1.0f, 1.0f));

        // Determine the sign of the angle using the cross product (Z-component)
        float crossProduct = lineDirection.x * tangentDirection.y - lineDirection.y * tangentDirection.x;
        if (crossProduct < 0)
        {
            angle = -angle; // Negative angle if tangent is counterclockwise relative to the line
        }

        return angle * (180 / Mathf.PI); // Convert angle to degrees
    }
    public static Vector3 ConvertAngleToVector(float angle, float magnitude)
    {
        float radians = (angle % 360) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radians) * magnitude, Mathf.Sin(radians) * magnitude, 0);
    }

    public static void ConvertVectorToAngleAndMagnitude(Vector3 vector, out float angle, out float magnitude)
    {
        magnitude = vector.magnitude;
        if (magnitude == 0)
        {
            angle = 0;
            return;
        }
        angle = (Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg) % 360;
        if (angle < 0)
        {
            angle += 360; // Ensure angle is in the range [0, 360)
        }
    }

}
