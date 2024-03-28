using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[ExecuteAlways]
public static class CurveCollider
{
    private static float resolution, edgeOffset = 1.2f; //1.2f
    //private static Curve curve;
    //private static EdgeCollider2D collider;

    //Includes firstPoint to ensure exact transitions
    public static EdgeCollider2D CreateCollider(List<EdgeCollider2D> colliderList, IGroundSegment segment, Transform parent, PhysicsMaterial2D material, Vector3? firstPoint = null, float resolutionMult = 10)
    {
        GameObject colliderObject = new("Collider");
        colliderObject.transform.parent = parent;
        EdgeCollider2D collider = colliderObject.AddComponent<EdgeCollider2D>();
        collider.sharedMaterial = material;
        if(segment.Curve.Type == CurveType.StartLine)
        {
            firstPoint = segment.Curve.GetPoint(0).ControlPoint;
        }
        //Iterate through points that make up GroundSegment's curve.
        for (int i = 0; i < segment.Curve.Count - 1; i++)
        {
            resolution = Mathf.Max(resolutionMult * segment.Curve.SegmentLengths[i]/20, 15);
            List<Vector2> newUnoffsetPoints;
            Vector2[] newPoints = Calculate2DPoints(segment.Curve.GetPoint(i), segment.Curve.GetPoint(i + 1), out newUnoffsetPoints, firstPoint);
            if (i == 0)
            {
                collider.points = newPoints;
                firstPoint = null;
            } else
            {
                collider.points = CombineArrays(collider.points, newPoints);
            }
        }
        colliderList.Add(collider);
        colliderObject.SetActive(false);
        return collider;
    }
    public static EdgeCollider2D GenerateCollider(Curve curve, GameObject host, PhysicsMaterial2D material, Vector3? firstPoint = null, float resolutionMult = 10)
    {
        var collider = host.AddComponent<EdgeCollider2D>();
        collider.sharedMaterial = material;
        if (curve.Type == CurveType.StartLine)
        {
            firstPoint = curve.GetPoint(0).ControlPoint;
        }
        //Iterate through points that make up GroundSegment's curve.
        for (int i = 0; i < curve.Count - 1; i++)
        {
            resolution = Mathf.Max(resolutionMult * curve.SegmentLengths[i] / 20, 15);
            List<Vector2> newUnoffsetPoints;
            Vector2[] newPoints = Calculate2DPoints(curve.GetPoint(i), curve.GetPoint(i + 1), out newUnoffsetPoints, firstPoint);
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
        return collider;
    }

    private static Vector2[] Calculate2DPoints(CurvePoint firstPoint, CurvePoint secondPoint, out List<Vector2> unoffset, Vector3? firstVectorPoint = null)
    {
        List<Vector2> points = new();
        unoffset = new();
        if (!(firstVectorPoint is null))
        {
            points.Add((Vector3)firstVectorPoint);
            unoffset.Add(firstPoint.ControlPoint);
        } 
        for (int i = 1; i < resolution; i++)
        {
            Vector2 newPoint = CalculateBezierPoint((1 / resolution) * i, firstPoint.ControlPoint, firstPoint.RightTangent + firstPoint.ControlPoint, secondPoint.LeftTangent + secondPoint.ControlPoint, secondPoint.ControlPoint);
            points.Add(newPoint);
        }
        points.Add(secondPoint.ControlPoint);
        Vector2[] returnArray = OffsetPoints(points.ToArray(), secondPoint, !(firstVectorPoint is null));
        points.RemoveAt(0);
        unoffset.AddRange(points);
        return returnArray;
    }

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
        offsetArray[^1] = lastPoint.ControlPoint + new Vector3(lastDir.y, -lastDir.x) * -edgeOffset;
        return offsetArray;
    }

    private static Vector3 CalculateOffset(Vector3 p0, Vector3 p1, float length)
    {

        // Calculate the direction vector of the line between p0 and p1
        Vector3 dir = p1 - p0;
        dir.Normalize(); // Make sure it has unit length

        // Calculate the perpendicular vector to the line
        Vector3 perp = new Vector3(dir.y, -dir.x); // Assumes 2D space
        perp.Normalize(); // Make sure it has unit length

        // Calculate the new point at distance "length" from p1 along the perpendicular slope
        Vector3 newPoint = p0 + perp * length;

        return newPoint;
    }

    private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 handlerP0, Vector3 handlerP1, Vector3 p1)
    {
        float u = 1.0f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; //first term
        p += 3f * uu * t * handlerP0; //second term
        p += 3f * u * tt * handlerP1; //third term
        p += ttt * p1; //fourth term

        return p;
    }

    private static Vector2[] CombineArrays(Vector2[] array1, Vector2[] array2)
    {
        Vector2[] combinedArray = new Vector2[array1.Length + array2.Length];
        array1.CopyTo(combinedArray, 0);
        array2.CopyTo(combinedArray, array1.Length);
        return combinedArray;
    }


}
