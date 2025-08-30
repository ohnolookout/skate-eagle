using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Static utility class for generating collider points for ground segments.
/// Provides methods to generate edge and bottom collider points based on curve data.
/// </summary>
[ExecuteAlways]
public static class ColliderGenerator
{
    // The resolution for point generation and the offset for edge colliders.
    private static float resolution, edgeOffset = 1.2f; //1.2f

    #region Collider Point Generation
    /// <summary>
    /// Generates an array of 2D points representing the edge collider for a ground segment.
    /// </summary>
    /// <param name="curvePoints">List of CurvePoints that define the ground segment's curve.</param>
    /// <param name="firstEdgeColliderPoint">Optional: The first point to use for the edge collider (can be null).</param>
    /// <param name="isInverted">If true, inverts the segment (affects offset direction).</param>
    /// <param name="resolutionMult">Multiplier for the resolution of the generated points.</param>
    /// <returns>Array of Vector2 points for the edge collider.</returns>
    public static Vector2[] GetEdgeColliderPoints(List<CurvePoint> curvePoints, Vector3? firstEdgeColliderPoint, bool isInverted = false, float resolutionMult = 10)
    {
        var colliderPoints = new Vector2[0];
        // Iterate through points that make up GroundSegment's curve.
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            var p1 = curvePoints[i];
            var p2 = curvePoints[i + 1];
            // Calculate resolution based on segment length and multiplier, with a minimum of 15.
            resolution = Mathf.Max(resolutionMult * GetLength(p1, p2) / 20, 15);
            // Generate new points for this segment.
            Vector2[] newPoints = Calculate2DPoints(p1, p2, firstEdgeColliderPoint, !isInverted);

            if (i == 0)
            {
                // For the first segment, initialize the collider points array.
                colliderPoints = newPoints;
                firstEdgeColliderPoint = null;
            }
            else
            {
                // For subsequent segments, combine the new points with the existing array.
                colliderPoints = CombineArrays(colliderPoints, newPoints);
            }
        }
        // Return the complete array of collider points.
        return colliderPoints;
    }

    /// <summary>
    /// Generates the bottom collider points for a ground segment, used for filling or mesh generation.
    /// </summary>
    /// <param name="fillSplinePoints">All points in the fill spline.</param>
    /// <param name="edgeColliderPoints">Points from the edge collider.</param>
    /// <param name="curvePointCount">Number of curve points in the segment.</param>
    /// <param name="isFirstSegment">True if this is the first segment in the chain.</param>
    /// <param name="isLastSegment">True if this is the last segment in the chain.</param>
    /// <returns>Array of Vector2 points for the bottom collider.</returns>
    public static Vector2[] GetBottomColliderPoints(List<CurvePoint> fillSplinePoints, Vector2[] edgeColliderPoints, int curvePointCount, bool isFirstSegment, bool isLastSegment)
    {
        var colliderPointCount = fillSplinePoints.Count - curvePointCount;
        var bottomColliderPoints = new Vector2[colliderPointCount];

        // Fill the bottom collider points in reverse order.
        for (int i = curvePointCount, j = 1; i < fillSplinePoints.Count; i++, j++)
        {
            bottomColliderPoints[colliderPointCount - j] = fillSplinePoints[i].Position;
        }

        // Add extra points for the first segment to close the shape.
        if (isFirstSegment)
        {
            Vector2[] firstPointArray = new Vector2[3] { edgeColliderPoints[0], fillSplinePoints[0].Position, fillSplinePoints[^1].Position };
            bottomColliderPoints = CombineArrays(firstPointArray, bottomColliderPoints);
        }
        // Add extra points for the last segment to close the shape.
        if (isLastSegment)
        {
            Vector2[] lastPointArray = new Vector2[3] { fillSplinePoints[curvePointCount].Position, fillSplinePoints[curvePointCount - 1].Position, edgeColliderPoints[^1] };
            bottomColliderPoints = CombineArrays(bottomColliderPoints, lastPointArray);
        }

        return bottomColliderPoints;

    }

    /// <summary>
    /// Calculates 2D points between two curve points, optionally applying an offset.
    /// </summary>
    /// <param name="firstPoint">The starting CurvePoint.</param>
    /// <param name="secondPoint">The ending CurvePoint.</param>
    /// <param name="firstVectorPoint">Optional: The first point to use (can be null).</param>
    /// <param name="doOffset">Whether to apply an offset to the points.</param>
    /// <returns>Array of Vector2 points for the segment.</returns>
    private static Vector2[] Calculate2DPoints(CurvePoint firstPoint, CurvePoint secondPoint, Vector3? firstVectorPoint, bool doOffset)
    {
        List<Vector2> points = new();
        if (firstVectorPoint != null)
        {
            points.Add((Vector3)firstVectorPoint);

            if (firstPoint.TangentMode != ShapeTangentMode.Continuous)
            {
                points.Add(firstPoint.Position);
            }
        }
        else
        {
            points.Add(firstPoint.Position);
        }


        // Skip curve calculations if points form a straight line, just add points
        if ((firstPoint.TangentMode == ShapeTangentMode.Linear && secondPoint.TangentMode == ShapeTangentMode.Linear) || AreTangentsAligned(firstPoint, secondPoint))
        {
            points.Add(secondPoint.Position);
        }
        // Otherwise calculate points from curve
        else
        {
            for (int i = 1; i < resolution; i++)
            {
                Vector3 newPoint = BezierMath.CalculateBezierPoint(firstPoint.Position, firstPoint.RightTangent + firstPoint.Position, secondPoint.LeftTangent + secondPoint.Position, secondPoint.Position, (1 / resolution) * i);
                points.Add(newPoint);
            }
            points.Add(secondPoint.Position);
        }

        // Offset points to account for edge distance if segment not inverted
        Vector2[] returnArray;

        if (doOffset)
        {
            returnArray = OffsetPoints(points.ToArray(), secondPoint, firstVectorPoint);
        }
        else
        {
            returnArray = points.ToArray();
        }

        points.RemoveAt(0);

        return returnArray;
    }

    /// <summary>
    /// Gets the length of a Bezier curve segment between two CurvePoints.
    /// </summary>
    private static float GetLength(CurvePoint p1, CurvePoint p2)
    {
        return BezierMath.Length(p1.Position, p1.RightTangent, p2.LeftTangent, p2.Position);
    }

#endregion

    #region Offset Points
    /// <summary>
    /// Offsets an array of points perpendicular to the segment direction.
    /// </summary>
    /// <param name="array">Array of points to offset.</param>
    /// <param name="lastPoint">The last CurvePoint in the segment.</param>
    /// <param name="firstVectorPoint">Optional: The first point to use (can be null).</param>
    /// <returns>Offset array of points.</returns>
    private static Vector2[] OffsetPoints(Vector2[] array, CurvePoint lastPoint, Vector3? firstVectorPoint)
    {
        Vector2[] offsetArray = new Vector2[array.Length];

        var startIndex = 0;

        if (firstVectorPoint != null)
        {
            offsetArray[0] = (Vector2)firstVectorPoint;
            startIndex = 1;
        }

        for (int i = startIndex; i < array.Length - 1; i++)
        {
            offsetArray[i] = CalculateOffset(array[i], array[i + 1], -edgeOffset);
        }
        // Offset the last point in the opposite direction
        offsetArray[^1] = CalculateOffset(array[^1], array[^2], edgeOffset);
        return offsetArray;
    }

    /// <summary>
    /// Calculates a point offset perpendicular to the direction between two points.
    /// </summary>
    /// <param name="p0">The base point.</param>
    /// <param name="p1">The next point to determine direction.</param>
    /// <param name="length">The offset distance.</param>
    /// <returns>The offset point.</returns>
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
    /// <summary>
    /// Combines two arrays of Vector2 into one.
    /// </summary>
    private static Vector2[] CombineArrays(Vector2[] array1, Vector2[] array2)
    {
        Vector2[] combinedArray = new Vector2[array1.Length + array2.Length];
        array1.CopyTo(combinedArray, 0);
        array2.CopyTo(combinedArray, array1.Length);
        return combinedArray;
    }

    /// <summary>
    /// Checks if the tangents of two CurvePoints are aligned (i.e., the segment is straight).
    /// </summary>
    /// <param name="firstPoint">The first CurvePoint.</param>
    /// <param name="secondPoint">The second CurvePoint.</param>
    /// <param name="tolerance">Tolerance for alignment check.</param>
    /// <returns>True if tangents are aligned, false otherwise.</returns>
    public static bool AreTangentsAligned(CurvePoint firstPoint, CurvePoint secondPoint, float tolerance = .001f)
    {
        // Vector from first to second point
        Vector2 pointToPoint = secondPoint.Position - firstPoint.Position;

        // World space right tangent direction from first point
        Vector2 rightTangentDir = firstPoint.RightTangent;
        // World space left tangent direction from second point
        Vector2 leftTangentDir = secondPoint.LeftTangent;

        // If both tangents are zero, line will be straight
        if (rightTangentDir == Vector2.zero && leftTangentDir == Vector2.zero) {
            return true;
        }

        // If points are the same, we can't determine alignment
        if (pointToPoint == Vector2.zero)
        {
            return false;
        }

        // Normalize vectors
        Vector2 pointToPointDir = pointToPoint.normalized;
        Vector2 rightDir = rightTangentDir.normalized;
        Vector2 leftDir = leftTangentDir.normalized;

        // Check if right tangent points toward second point or right tangent is zero
        bool rightAligned = rightTangentDir == Vector2.zero || Vector2.Dot(rightDir, pointToPointDir) > 1f - tolerance;

        // Check if left tangent points toward first point (i.e., opposite direction) pr left tangent is zero
        bool leftAligned = leftTangentDir == Vector2.zero || Vector2.Dot(leftDir, -pointToPointDir) > 1f - tolerance;

        return rightAligned && leftAligned;
    }
    #endregion

}
