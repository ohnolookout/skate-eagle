using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ProceduralCurveSectionParams
{
    public float lengthMin, lengthMax, roundMin, roundMax, steepMin, steepMax;


    public ProceduralCurveSectionParams(float lengthMin, float lengthMax, float roundMin, float roundMax, float steepMin, float steepMax)
    {
        this.lengthMin = lengthMin;
        this.lengthMax = lengthMax;
        this.roundMin = roundMin;
        this.roundMax = roundMax;
        this.steepMin = steepMin;
        this.steepMax = steepMax;
    }

    public ProceduralCurveSectionParams(ProceduralCurveSection definition)
    {
        Vector2 lengthMinMax = ProceduralCurveSection.Lengths(definition._length);
        this.lengthMin = lengthMinMax.x;
        this.lengthMax = lengthMinMax.y;
        Vector2 roundMinMax = ProceduralCurveSection.Shapes(definition._shape);
        this.roundMin = roundMinMax.x;
        this.roundMax = roundMinMax.y;
        Vector2 slopeMinMax = ProceduralCurveSection.Slopes(definition._slope);
        this.steepMin = slopeMinMax.x;
        this.steepMax = slopeMinMax.y;
    }
}
