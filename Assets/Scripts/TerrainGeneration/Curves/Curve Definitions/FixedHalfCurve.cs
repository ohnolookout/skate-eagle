using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HalfCurveType { Peak, Valley, Straight};
public class FixedHalfCurve
{
    public float Length = length_medium;
    public float Slope = slope_normal;
    public float Shape = shape_roller;
    public float Climb = -1;
    public HalfCurveType Type = HalfCurveType.Peak;
    public float Grade => Climb/Length;

    #region Consts
    //Length
    public const float length_short = 30;
    public const float length_medium = 45;
    public const float length_long = 65;
    public const float length_jumbo = 85;

    //Slope
    public const float slope_flat = 0.6f;
    public const float slope_gentle = 0.85f;
    public const float slope_normal = 1.3f;
    public const float slope_steep = 2f;

    //Shape
    public const float shape_table = 0.2f;
    public const float shape_roller = 0.5f;
    public const float shape_soft_peak = 0.65f;
    public const float shape_hard_peak = 0.85f;
    #endregion

    public FixedHalfCurve()
    {

    }

    public FixedHalfCurve(float length, float slope, float shape, float climb, HalfCurveType type)
    {
        Length = length;
        Slope = slope;
        Shape = shape;
        Climb = climb;
        Type = type;
    }
}
