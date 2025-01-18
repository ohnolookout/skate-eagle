using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CurveSectionParameters
{
    public float Length;
    public float Shape;
    public float Pitch;
    public float Climb;

    public CurveSectionParameters(float length, float shape, float pitch, float climb)
    {
        Length = length;
        Shape = shape;
        Pitch = pitch;
        Climb = climb;
    }

    public void Log()
    {
        Debug.Log("~~~Curve Section Parameters~~~");
        Debug.Log($"Length: {Length}");
        Debug.Log($"Shape: {Shape}");
        Debug.Log($"Pitch: {Pitch}");
        Debug.Log($"Climb: {Climb}");
    }
}
