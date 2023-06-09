using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CurveParameters
{
    public float xDeltaMin, xDeltaMax, yDeltaMin, yDeltaMax, xVelocityMin, xVelocityMax, slopeMin, slopeMax;

    public CurveParameters(Vector2 xDeltaRange, Vector2 yDeltaRange, Vector2 xVelocityRange, Vector2 slopeRange)
    {
        xDeltaMin = xDeltaRange.x;
        xDeltaMax = xDeltaRange.y;
        yDeltaMin = yDeltaRange.x;
        yDeltaMax = yDeltaRange.y;
        xVelocityMin = xVelocityRange.x;
        xVelocityMax = xVelocityRange.y;
        slopeMin = slopeRange.x;
        slopeMax = slopeRange.y;
    }

    public CurveParameters(float xMin, float xMax, float yMin, float yMax, float xVelMin, float xVelMax, float slopeInputMin, float slopeInputMax)
    {
        xDeltaMin = xMin;
        xDeltaMax = xMax;
        yDeltaMin = yMin;
        yDeltaMax = yMax;
        xVelocityMin = xVelMin;
        xVelocityMax = xVelMax;
        slopeMin = slopeInputMin;
        slopeMax = slopeInputMax;
    }
}
