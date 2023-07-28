
using UnityEngine;
using System;

[Serializable]
public enum LengthType { Short = 0, Medium = 1, Long = 2, Jumbo = 3};
[Serializable]
public enum ShapeType { Roller, SoftPeak, HardPeak, SoftTable, HardTable }
[Serializable]
public enum SlopeType { Shallow, Normal, Steep };
[Serializable]
public enum SkewType { Left, Right, Center, Random };
[Serializable]
public class CurveDefinition
{
    public ShapeType _shape;
    public LengthType _length;
    public SlopeType _slope;
    public SkewType _skew;

    public CurveDefinition(LengthType length, ShapeType shape, SlopeType slope, SkewType skew)
    {
        _length = length;
        _shape = shape;
        _slope = slope;
        _skew = skew;
    }

    public static Vector2 Lengths(LengthType lengthType)
    {
        return lengthType switch
        {
            LengthType.Short => new Vector2(25, 40),
            LengthType.Medium => new Vector2(35, 60),
            LengthType.Long => new Vector2(50, 70),
            LengthType.Jumbo => new Vector2(65, 90),
            _ => new Vector2(35, 60)
        };
    }

    public static Vector2 Slopes(SlopeType slopeType)
    {
        return slopeType switch
        {
            SlopeType.Shallow => new Vector2(0.2f, 0.8f),
            SlopeType.Normal => new Vector2(0.8f, 1.6f),
            SlopeType.Steep => new Vector2(1.6f, 2.2f),
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

    public LengthType Length
    {
        get
        {
            return _length;
        }
    }

    public ShapeType Shape
    {
        get
        {
            return _shape;
        }
    }

    public SlopeType Slope
    {
        get
        {
            return _slope;
        }
    }

    public SkewType Skew
    {
        get
        {
            return _skew;
        }
    }

    public CurveDefinition DeepCopy()
    {
        return new CurveDefinition(_length, _shape, _slope, _skew);
    }
}
