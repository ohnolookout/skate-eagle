using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveTypes
{


    public static CurveParameters[] Roller()
    {
        CurveParameters valleyParameters = new();
        valleyParameters.lengthMin = 30;
        valleyParameters.lengthMax = 70;
        valleyParameters.roundMin = 0.45f;
        valleyParameters.roundMax = 0.50f;
        valleyParameters.steepMin = 0.4f;
        valleyParameters.steepMax = 2f;
        valleyParameters.skew = SkewType.Center;
        CurveParameters peakParameters = new();
        peakParameters.lengthMin = 40;
        peakParameters.lengthMax = 70;
        peakParameters.roundMin = 0.45f;
        peakParameters.roundMax = 0.55f;
        peakParameters.steepMin = 0.6f;
        peakParameters.steepMax = 2f;
        peakParameters.skew = SkewType.Center;
        return new CurveParameters[2] { valleyParameters, peakParameters};
    }

    public static CurveParameters[] SmallRoller()
    {
        CurveParameters valleyParameters = new();
        valleyParameters.lengthMin = 25;
        valleyParameters.lengthMax = 40;
        valleyParameters.roundMin = 0.45f;
        valleyParameters.roundMax = 0.50f;
        valleyParameters.steepMin = 0.4f;
        valleyParameters.steepMax = 1.4f;
        valleyParameters.skew = SkewType.Center;
        CurveParameters peakParameters = new();
        peakParameters.lengthMin = 20;
        peakParameters.lengthMax = 35;
        peakParameters.roundMin = 0.45f;
        peakParameters.roundMax = 0.55f;
        peakParameters.steepMin = 0.4f;
        peakParameters.steepMax = 1.4f;
        peakParameters.skew = SkewType.Center;
        return new CurveParameters[2] { valleyParameters, peakParameters };
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
            SlopeType.Normal => new Vector2(0.8f, 1.4f),
            SlopeType.Steep => new Vector2(1.4f, 2f),
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
            ShapeType.SoftPeak => new Vector2(0.55f, 0.7f),
            ShapeType.HardPeak => new Vector2(0.7f, 0.9f),
            _ => new Vector2(0.45f, 0.55f)
        };
    }

    

}
