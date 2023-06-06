using System;
using System.Collections.Generic;
using UnityEngine;

public struct CurvePoint
{
    private Vector3 controlPoint, leftTangent, rightTangent;

    public CurvePoint(Vector3 control)
    {
        controlPoint = control;
        leftTangent = new Vector3(0, 0);
        rightTangent = new Vector3(0, 0);
    }

    public CurvePoint(Vector3 control, Vector3 lTang, Vector3 rTang)
    {
        controlPoint = control;
        leftTangent = lTang;
        rightTangent = rTang;
    }


    public Vector3 ControlPoint
    {
        get
        {
            return controlPoint;
        }
        set
        {
            controlPoint = value;
        }
    }

    public Vector3 LeftTangent
    {
        get
        {
            return leftTangent;
        }
        set
        {
            leftTangent = value;
        }
    }

    public Vector3 RightTangent
    {
        get
        {
            return rightTangent;
        }
        set
        {
            rightTangent = value;
        }
    }

    public void SetTangents(float slope, float velocity)
    {
        leftTangent = new Vector3(-velocity, -velocity * slope);
        rightTangent = new Vector3(velocity, velocity * slope);
    }
}
