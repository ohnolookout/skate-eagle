using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
[ExecuteAlways]
public static class CurveCollider
{
    private static float resolution, edgeOffset = 1.2f; //1.2f

    #region Generation
    public static void BuildSegmentCollider(GroundSegment segment, PhysicsMaterial2D material, float resolutionMult = 10)
    {
        segment.Collider.sharedMaterial = material;
        var collider = segment.Collider;
        Vector3? firstPoint = segment.FirstColliderPoint();
        var curve = segment.Curve;

        //Iterate through points that make up GroundSegment's curve.
        for (int i = 0; i < curve.Count - 1; i++)
        {
            resolution = Mathf.Max(resolutionMult * curve.SectionLengths[i] / 20, 15);
            Vector2[] newPoints = Calculate2DPoints(curve.GetPoint(i), curve.GetPoint(i + 1), firstPoint);
            if (i == 0)
            {
                collider.points = newPoints;
                firstPoint = null;
            }
            else
            {
                collider.points = CombineArrays(collider.points, newPoints);
            }
        }

        var bottomCollider = segment.BottomCollider;

        //Don't do edge collision if segment is floating;
        if (segment.IsFloating)
        {
            bottomCollider.gameObject.SetActive(false);
            return;
        }

        bottomCollider.gameObject.SetActive(true);
        bottomCollider.points = new Vector2[2] { segment.Spline.GetPosition(0), segment.Spline.GetPosition(segment.Spline.GetPointCount() - 1) };
        if (segment.IsFirstSegment)
        {
            Vector2[] firstPointArray = new Vector2[2] { segment.Spline.GetPosition(1), segment.Spline.GetPosition(0) };
            bottomCollider.points = CombineArrays(firstPointArray, bottomCollider.points);
        }
        if (segment.IsLastSegment)
        {
            Vector2[] lastPointArray = new Vector2[3] { segment.Spline.GetPosition(segment.Spline.GetPointCount() - 1), segment.Curve.EndPoint.Position, collider.points[^1] };
            bottomCollider.points = CombineArrays(bottomCollider.points, lastPointArray);
        }
    }

    private static Vector2[] Calculate2DPoints(CurvePoint firstPoint, CurvePoint secondPoint, Vector3? firstVectorPoint)
    {
        //Debug.Log("Calculating collider points...");
        List<Vector2> points = new();
        if (firstVectorPoint != null)
        {
            points.Add((Vector3)firstVectorPoint);
        }

        //Skip curve calcs if points form a straight line, just add points
        if (AreTangentsAligned(firstPoint, secondPoint))
        {
            points.Add(firstPoint.Position);
            points.Add(secondPoint.Position);
        }
        //Otherwise calculate points from curve
        else
        {
            for (int i = 1; i < resolution; i++)
            {
                Vector3 newPoint = BezierMath.CalculateBezierPoint(firstPoint.Position, firstPoint.RightTangent + firstPoint.Position, secondPoint.LeftTangent + secondPoint.Position, secondPoint.Position, (1 / resolution) * i);
                points.Add(newPoint);
            }
            points.Add(secondPoint.Position);
        }

        //Offset points to account for edge distance
        Vector2[] returnArray = OffsetPoints(points.ToArray(), secondPoint, !(firstVectorPoint == null));
        points.RemoveAt(0);

        return returnArray;
    }
    #endregion

    #region Offset Points
    private static Vector2[] OffsetPoints(Vector2[] array, CurvePoint lastPoint, bool isFirstSegment)
    {
        Vector2[] offsetArray = new Vector2[array.Length];
        //Set first point directly because the overlap point has already been offset.
        int startIndex = 0;
        if (isFirstSegment)
        {
            offsetArray[0] = array[0];
            startIndex = 1;
        }
        for (int i = startIndex; i < array.Length-1; i++)
        {
            offsetArray[i] = CalculateOffset(array[i], array[i + 1], -edgeOffset);
        }
        Vector3 lastDir = (lastPoint.RightTangent - lastPoint.LeftTangent)/2;
        lastDir.Normalize();
        offsetArray[^1] = lastPoint.Position + new Vector3(lastDir.y, -lastDir.x) * -edgeOffset;
        return offsetArray;
    }

    private static Vector3 CalculateOffset(Vector3 p0, Vector3 p1, float length)
    {

        // Calculate the direction vector of the line between p0 and p1
        Vector3 dir = p1 - p0;
        dir.Normalize();

        // Calculate the perpendicular vector to the line
        Vector3 perp = new Vector3(dir.y, -dir.x);
        perp.Normalize();

        // Calculate the new point at distance "length" from p1 along the perpendicular slope
        Vector3 newPoint = p0 + perp * length;

        return newPoint;
    }
    #endregion

    #region Utilities
    private static Vector2[] CombineArrays(Vector2[] array1, Vector2[] array2)
    {
        Vector2[] combinedArray = new Vector2[array1.Length + array2.Length];
        array1.CopyTo(combinedArray, 0);
        array2.CopyTo(combinedArray, array1.Length);
        return combinedArray;
    }

    public static bool AreTangentsAligned(CurvePoint firstPoint, CurvePoint secondPoint, float tolerance = 1e-5f)
    {
        // Vector from first to second point
        Vector2 pointToPoint = secondPoint.Position - firstPoint.Position;

        // World space right tangent direction from first point
        Vector2 rightTangentDir = firstPoint.RightTangent;
        // World space left tangent direction from second point
        Vector2 leftTangentDir = secondPoint.LeftTangent;

        // If any direction is zero, we can't determine alignment
        if (rightTangentDir == Vector2.zero || leftTangentDir == Vector2.zero || pointToPoint == Vector2.zero)
            return false;

        // Normalize vectors
        Vector2 pointToPointDir = pointToPoint.normalized;
        Vector2 rightDir = rightTangentDir.normalized;
        Vector2 leftDir = leftTangentDir.normalized;

        // Check if right tangent points toward second point
        bool rightAligned = Vector2.Dot(rightDir, pointToPointDir) > 1f - tolerance;

        // Check if left tangent points toward first point (i.e., opposite direction)
        bool leftAligned = Vector2.Dot(leftDir, -pointToPointDir) > 1f - tolerance;

        return rightAligned && leftAligned;
    }
    #endregion

}
