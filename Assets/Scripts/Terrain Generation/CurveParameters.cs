using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CurveParameters
{
    public float lengthMin, lengthMax, roundMin, roundMax, steepMin, steepMax;
    public SkewType skew;


    public CurveParameters(float lengthMin, float lengthMax, float roundMin, float roundMax, float steepMin, float steepMax, SkewType skew = SkewType.Center)
    {
        this.lengthMin = lengthMin;
        this.lengthMax = lengthMax;
        this.roundMin = roundMin;
        this.roundMax = roundMax;
        this.steepMin = steepMin;
        this.steepMax = steepMax;
        this.skew = skew;
    }

    public CurveParameters(HalfCurveDefinition definition)
    {
        Vector2 lengthMinMax = HalfCurveDefinition.Lengths(definition._length);
        this.lengthMin = lengthMinMax.x;
        this.lengthMax = lengthMinMax.y;
        Vector2 roundMinMax = HalfCurveDefinition.Shapes(definition._shape);
        this.roundMin = roundMinMax.x;
        this.roundMax = roundMinMax.y;
        Vector2 slopeMinMax = HalfCurveDefinition.Slopes(definition._slope);
        this.steepMin = slopeMinMax.x;
        this.steepMax = slopeMinMax.y;
        this.skew = definition._skew;
    }
}
