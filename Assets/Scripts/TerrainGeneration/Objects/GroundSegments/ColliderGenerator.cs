using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteAlways]
public static class ColliderGenerator
{
    private static float resolution, edgeOffset = 1.2f; //1.2f

    #region Generation

    public static Vector2[] GetEdgeColliderPoints(List<CurvePoint> curvePoints, Vector3? firstEdgeColliderPoint, bool isInverted = false, float resolutionMult = 10)
    {
        var colliderPoints = new Vector2[0];
        //Iterate through points that make up GroundSegment's curve.
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            var p1 = curvePoints[i];
            var p2 = curvePoints[i + 1];
            resolution = Mathf.Max(resolutionMult * GetLength(p1, p2) / 20, 15);
            Vector2[] newPoints = Calculate2DPoints(p1, p2, firstEdgeColliderPoint, !isInverted);

            if (i == 0)
            {
                colliderPoints = newPoints;
                firstEdgeColliderPoint = null;
            }
            else
            {
                colliderPoints = CombineArrays(colliderPoints, newPoints);
            }
        }


        return colliderPoints;
    }

    public static Vector2[] GetBottomColliderPoints(List<CurvePoint> fillSplinePoints, Vector2[] edgeColliderPoints, int curvePointCount, bool isFirstSegment, bool isLastSegment)
    {
        var colliderPointCount = fillSplinePoints.Count - curvePointCount;
        var bottomColliderPoints = new Vector2[colliderPointCount];

        for (int i = curvePointCount, j = 1; i < fillSplinePoints.Count; i++, j++)
        {
            bottomColliderPoints[colliderPointCount - j] = fillSplinePoints[i].Position;
        }

        if (isFirstSegment)
        {
            Vector2[] firstPointArray = new Vector2[3] { edgeColliderPoints[0], fillSplinePoints[0].Position, fillSplinePoints[^1].Position };
            bottomColliderPoints = CombineArrays(firstPointArray, bottomColliderPoints);
        }
        if (isLastSegment)
        {
            Vector2[] lastPointArray = new Vector2[3] { fillSplinePoints[curvePointCount].Position, fillSplinePoints[curvePointCount-1].Position, edgeColliderPoints[^1] };
            bottomColliderPoints = CombineArrays(bottomColliderPoints, lastPointArray);
        }

        return bottomColliderPoints;

    }

    private static Vector2[] Calculate2DPoints(CurvePoint firstPoint, CurvePoint secondPoint, Vector3? firstVectorPoint, bool doOffset)
    {
        List<Vector2> points = new();
        if (firstVectorPoint != null)
        {
            points.Add((Vector3)firstVectorPoint);
        } else
        {
            points.Add(firstPoint.Position);
        }

        //Skip curve calcs if points form a straight line, just add points
        if ((firstPoint.TangentMode == ShapeTangentMode.Linear && secondPoint.TangentMode == ShapeTangentMode.Linear) || AreTangentsAligned(firstPoint, secondPoint))
        {
            //points.Add(firstPoint.Position);
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

        //Offset points to account for edge distance if segment not inverted
        Vector2[] returnArray;

        if (doOffset)
        {
            returnArray = OffsetPoints(points.ToArray(), secondPoint, firstVectorPoint);
        } else
        {
            returnArray = points.ToArray();
        }

        points.RemoveAt(0);

        return returnArray;
    }

    private static float GetLength(CurvePoint p1, CurvePoint p2)
    {
        return BezierMath.Length(p1.Position, p1.RightTangent, p2.LeftTangent, p2.Position);        
    }

    #endregion

    #region Offset Points
    private static Vector2[] OffsetPoints(Vector2[] array, CurvePoint lastPoint, Vector3? firstVectorPoint)
    {
        Vector2[] offsetArray = new Vector2[array.Length];

        var startIndex = 0;

        if(firstVectorPoint != null)
        {
            offsetArray[0] = (Vector2)firstVectorPoint;
            startIndex = 1;
        }

        for (int i = startIndex; i < array.Length-1; i++)
        {
            offsetArray[i] = CalculateOffset(array[i], array[i + 1], -edgeOffset);
        }
        //Vector3 lastDir = (lastPoint.RightTangent - lastPoint.LeftTangent)/2;
        //lastDir.Normalize();
        offsetArray[^1] = CalculateOffset(array[^1], array[^2], edgeOffset);
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
