using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CurvePoint
{
    [SerializeField] private Vector3 controlPoint, leftTangent, rightTangent;

    public CurvePoint(Vector3 control)
    {
        controlPoint = control;
        leftTangent = new(0, 0);
        rightTangent = new(0, 0);
    }

    public CurvePoint(Vector3 control, Vector3 lTang, Vector3 rTang)
    {
        controlPoint = control;
        leftTangent = lTang;
        rightTangent = rTang;
    }


    public Vector3 ControlPoint { get => controlPoint; set => controlPoint = value; }  
    public Vector3 LeftTangent {  get => leftTangent; set => leftTangent = value; }
    public Vector3 RightTangent {  get => rightTangent; set => rightTangent = value; } 
    public void SetTangents(float slope, float velocity)
    {
        leftTangent = new Vector3(-velocity, -velocity * slope);
        rightTangent = new Vector3(velocity, velocity * slope);
    }

    public void Log()
    {
        Debug.Log("~~~Curve Point~~~");
        Debug.Log($"Control Point: {controlPoint}");
        Debug.Log($"Left Tangent: {leftTangent}");
        Debug.Log($"Right Tangent: {rightTangent}");

    }
}
