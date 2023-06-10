using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class CurveTypeSettings : ScriptableObject
{
    private float xDeltaMin, xDeltaMax, lengthToVelocityRatioMin, lengthToVelocityRatioMax, slopeMin, slopeMax;
    string curveName;
    private CurveParameters parameters;

    //public string curveTypeName;
    public CurveTypeSettings()
    {
        xDeltaMin = 7;
        xDeltaMax = 24;
        lengthToVelocityRatioMin = 1;
        lengthToVelocityRatioMax = 4;
        slopeMin = 0.2f;
        slopeMax = 1.6f;
        UpdateParameters();

    }
    
    public CurveTypeSettings(float xDeltaMin, float xDeltaMax, float lengthToVelocityRatioMin, float lengthToVelocityRatioMax, float slopeMin, float slopeMax)
    {
        this.xDeltaMin = xDeltaMin;
        this.xDeltaMax = xDeltaMax;
        this.lengthToVelocityRatioMin = lengthToVelocityRatioMin;
        this.lengthToVelocityRatioMax = lengthToVelocityRatioMax;
        this.slopeMin = slopeMin;
        this.slopeMax = slopeMax;
        UpdateParameters();
    }

    public CurveTypeSettings(CurveParameters parameters)
    {
        xDeltaMin = parameters.xDeltaMin;
        xDeltaMax = parameters.xDeltaMax;
        lengthToVelocityRatioMin = parameters.lengthToVelocityRatioMin;
        lengthToVelocityRatioMax = parameters.lengthToVelocityRatioMax;
        slopeMin = parameters.slopeMin;
        slopeMax = parameters.slopeMax;
        this.parameters = parameters;
    }

    public void UpdateParameters()
    {
        parameters = new(xDeltaMin, xDeltaMax, lengthToVelocityRatioMin, lengthToVelocityRatioMax, slopeMin, slopeMax);
    }

    public CurveParameters CurveParameters
    {
        get
        {
            return parameters;
        }
    }

}

