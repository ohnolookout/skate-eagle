
using UnityEngine;
using System;

#region Enums
[Serializable]
public enum LengthType { Short = 0, Medium = 1, Long = 2, Jumbo = 3};
[Serializable]
public enum ShapeType { Roller, SoftPeak, HardPeak, SoftTable, HardTable }
[Serializable]
public enum SlopeType { Flat, Gentle, Normal, Steep };
#endregion

[Serializable]
public class HalfCurveDefinition
{
    #region Declarations
    public ShapeType _shape;
    public LengthType _length;
    public SlopeType _slope;
    public LengthType Length => _length;
    public ShapeType Shape => _shape;
    public SlopeType Slope => _slope;
#endregion

    public HalfCurveDefinition(LengthType length, ShapeType shape, SlopeType slope)
    {
        _length = length;
        _shape = shape;
        _slope = slope;
    }

    public static Vector2 Lengths(LengthType lengthType)
    {
        return lengthType switch
        {
            LengthType.Short => new Vector2(25, 40),
            LengthType.Medium => new Vector2(35, 55),
            LengthType.Long => new Vector2(55, 75),
            LengthType.Jumbo => new Vector2(75, 90),
            _ => new Vector2(35, 60)
        };
    }

    public static Vector2 Slopes(SlopeType slopeType)
    {
        return slopeType switch
        {
            SlopeType.Flat => new Vector2(0.4f, 0.7f),
            SlopeType.Gentle => new Vector2(0.6f, 1.1f),
            SlopeType.Normal => new Vector2(1.0f, 1.5f),
            SlopeType.Steep => new Vector2(1.5f, 2.2f),
            _ => new Vector2(0.8f, 1.4f)
        };
    }

    public static Vector2 Shapes(ShapeType shapeType)
    {
        return shapeType switch
        {
            ShapeType.HardTable => new Vector2(0.1f, 0.3f),
            ShapeType.SoftTable => new Vector2(0.3f, 0.45f),
            ShapeType.Roller => new Vector2(0.45f, 0.55f),
            ShapeType.SoftPeak => new Vector2(0.55f, 0.75f),
            ShapeType.HardPeak => new Vector2(0.75f, 0.9f),
            _ => new Vector2(0.45f, 0.55f)
        };
    }

   

}
