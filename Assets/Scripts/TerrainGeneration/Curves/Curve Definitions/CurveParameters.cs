using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CurveParameters
{
    public float Length;
    public float Shape;
    public float Slope;
    public float Climb;

    public CurveParameters(float length, float shape, float slope, float climb)
    {
        Length = length;
        Shape = shape;
        Slope = slope;
        Climb = climb;
    }
}
