using UnityEngine;
using System.Collections.Generic;
public class OldBezierCollider2D
{
    public Vector3 firstPoint;
    public Vector3 secondPoint;

    public Vector3 handlerFirstPoint;
    public Vector3 handlerSecondPoint;
    public float edgeOffset = 0.55f;

    public int resolution = 0;
    private OldBezierColliderCreator colliderCreator;
    private bool movingRight;

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 handlerP0, Vector3 handlerP1, Vector3 p1)
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

    //Figure out way to use slope of tangents to create offset.

    public Vector2[] calculate2DPoints(OldBezierColliderCreator creator, int startSplineIndex)
    {
        List<Vector2> points = new List<Vector2>();
        resolution = Resolution;
        colliderCreator = creator;
        points.Add(firstPoint);
        //How to handle first and last points? Might need to access edgecollider...
        for (int i = 1; i < resolution; i++)
        {
            Vector2 newPoint = CalculateBezierPoint((1f / resolution) * i, firstPoint, handlerFirstPoint, handlerSecondPoint, secondPoint);
            points.Add(newPoint);
        }
        points.Add(secondPoint);
        //return points.ToArray();
        return OffsetPoints(points.ToArray(), startSplineIndex);
    }

    public Vector2[] OffsetPoints(Vector2[] array, int startSplineIndex)
    {
        Vector2[] offsetArray = new Vector2[array.Length - 1];
        //Don't index first point -- it will come from preceding curve's last point.
        for(int i = 0; i < array.Length - 1; i++)
        {
            offsetArray[i] = CalculateOffset(array[i], array[i + 1], -edgeOffset);
        }

        return offsetArray;
    }

    public static Vector3 CalculateOffset(Vector3 p0, Vector3 p1, float length)
    {

        // Calculate the direction vector of the line between p0 and p1
        Vector3 dir = p1 - p0;
        dir.Normalize(); // Make sure it has unit length
        
        // Calculate the perpendicular vector to the line
        Vector3 perp = new Vector3(dir.y, -dir.x, 0); // Assumes 2D space
        perp.Normalize(); // Make sure it has unit length

        // Calculate the new point at distance "length" from p1 along the perpendicular slope
        Vector3 newPoint = p1 + perp * length;

        return newPoint;
    }

    public float Height
    {
        get
        {
            return Mathf.Max(Mathf.Abs(handlerFirstPoint.y - firstPoint.y) , 2);
        }
    }

    public float Length
    {
        get
        {
            return Vector3.Distance(firstPoint, handlerFirstPoint) +
                Vector3.Distance(handlerFirstPoint, handlerSecondPoint) +
                Vector3.Distance(handlerSecondPoint, secondPoint);
        }
    }

    public int Resolution
    {
        get
        {
            int res = (int)((Height / 3) * (Length));
            if(res <= 0)
            {
                //Debug.LogError($"DANGER!! Resolution <= 0!! Resolution: {res} Height: {Height} Length: {Length}");
            }
            return res;
        }
        set
        {
            resolution = value;
        }
    }
}