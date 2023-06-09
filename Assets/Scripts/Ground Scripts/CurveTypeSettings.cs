using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class CurveTypeSettings : ScriptableObject
{
    private float xDeltaMin, xDeltaMax, yDeltaMin, yDeltaMax, xVelocityMin, xVelocityMax, slopeMin, slopeMax;
    private CurveParameters parameters;

    //public string curveTypeName;
    public CurveTypeSettings()
    {
        xDeltaMin = 7;
        xDeltaMax = 24;
        yDeltaMin = -4;
        yDeltaMax = 2;
        xVelocityMin = 3;
        xVelocityMax = 6;
        slopeMin = 0.2f;
        slopeMax = 1.6f;
        UpdateParameters();

    }
    public CurveTypeSettings(Vector2 xDeltaRange, Vector2 yDeltaRange, Vector2 xVelocityRange, Vector2 slopeRange)
    {
        xDeltaMin = xDeltaRange.x;
        xDeltaMax = xDeltaRange.y;
        yDeltaMin = yDeltaRange.x;
        yDeltaMax = yDeltaRange.y;
        xVelocityMin = xVelocityRange.x;
        xVelocityMax = xVelocityRange.y;
        slopeMin = slopeRange.x;
        slopeMax = slopeRange.y;
        UpdateParameters();
    }
    
    public CurveTypeSettings(float xMin, float xMax, float yMin, float yMax, float xVelMin, float xVelMax, float slopeInputMin, float slopeInputMax)
    {
        xDeltaMin = xMin;
        xDeltaMax = xMax;
        yDeltaMin = yMin;
        yDeltaMax = yMax;
        xVelocityMin = xVelMin;
        xVelocityMax = xVelMax;
        slopeMin = slopeInputMin;
        slopeMax = slopeInputMax;
        UpdateParameters();
    }

    public CurveTypeSettings(CurveParameters parameters)
    {
        xDeltaMin = parameters.xDeltaMin;
        xDeltaMax = parameters.xDeltaMax;
        yDeltaMin = parameters.yDeltaMin;
        yDeltaMax = parameters.yDeltaMax;
        xVelocityMin = parameters.xVelocityMin;
        xVelocityMax = parameters.xVelocityMax;
        slopeMin = parameters.slopeMin;
        slopeMax = parameters.slopeMax;
        this.parameters = parameters;
    }

    public void UpdateParameters()
    {
        parameters = new(xDeltaMin, xDeltaMax, yDeltaMin, yDeltaMax, xVelocityMin, xVelocityMax, slopeMin, slopeMax);
    }

    public CurveParameters CurveParameters
    {
        get
        {
            return parameters;
        }
    }

}

