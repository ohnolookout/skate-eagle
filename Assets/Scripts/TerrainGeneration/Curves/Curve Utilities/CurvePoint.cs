using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CurvePoint
{
    [SerializeField] private Vector3 position, leftTangent, rightTangent;

    public CurvePoint(Vector3 control)
    {
        position = control;
        leftTangent = new(0, 0);
        rightTangent = new(0, 0);
    }

    public CurvePoint(Vector3 control, Vector3 lTang, Vector3 rTang)
    {
        position = control;
        leftTangent = lTang;
        rightTangent = rTang;
    }


    public Vector3 Position { get => position; set => position = value; }  
    public Vector3 LeftTangent {  get => leftTangent; set => leftTangent = value; }
    public Vector3 RightTangent {  get => rightTangent; set => rightTangent = value; } 
    public Vector3 LeftTangentPosition => position + leftTangent;
    public Vector3 RightTangentPosition => position + rightTangent;
    public void SetTangents(float slope, float velocity)
    {
        leftTangent = new Vector3(-velocity, -velocity * slope);
        rightTangent = new Vector3(velocity, velocity * slope);
    }

    public void SetTangents(Vector3 tangent)
    {
        leftTangent = -tangent;
        rightTangent = tangent;
    }

    public void SetTangents(Vector3 leftTangent, Vector3 rightTangent)
    {
        this.leftTangent = leftTangent;
        this.rightTangent = rightTangent;
    }

    public void SetLeftTangentAngle(float angle, Vector2 endPoint, float? magnitude = null)
    {
        var mag = magnitude ?? leftTangent.magnitude;
        leftTangent = BezierMath.GetTangentFromAngle(Position, endPoint, angle, mag);
        Debug.Log("Left Tangent: " + leftTangent);
    }

    public void SetRightTangentAngle(float angle, Vector2 endPoint, float? magnitude = null)
    {
        var mag = magnitude ?? rightTangent.magnitude;
        rightTangent = BezierMath.GetTangentFromAngle(Position, endPoint, angle, mag);
        Debug.Log("Right Tangent: " + rightTangent);
    }

    public void SetTangentAngles(float angle, Vector2 endPoint, float? magnitude = null)
    {
        SetLeftTangentAngle(-angle, endPoint, magnitude);
        SetRightTangentAngle(angle, endPoint, magnitude);
    }

    public CurvePoint Move(Vector3 move)
    {
        return new(position + move, leftTangent, rightTangent);
    }

    public void Log()
    {
        Debug.Log("~~~Curve Point~~~");
        Debug.Log($"Control Point: {position}");
        Debug.Log($"Left Tangent: {leftTangent}");
        Debug.Log($"Right Tangent: {rightTangent}");

    }

    //Returns the tangent that is adjusted relative to the given tangent
    public static Vector2 AdjustedTangent(Vector2 tangent, Vector2 baseline)
    {
        float m1 = tangent.y / tangent.x;
        float m2 = baseline.y / baseline.x;

        float magnitude = tangent.magnitude;

        // Handle perpendicular case
        if (1 + m1 * m2 == 0)
        {
            return new Vector2(-tangent.y, tangent.x); // Rotate 90 degrees
        }

        float angleSlope = (m2 - m1) / (1 + m1 * m2);
        Vector2 direction = new Vector2(1, angleSlope).normalized;

        return direction * magnitude;
    }
}
