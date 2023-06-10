using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CurveParameters
{
    public float xDeltaMin, xDeltaMax, lengthToVelocityRatioMin, lengthToVelocityRatioMax, slopeMin, slopeMax;


    public CurveParameters(float xDeltaMin, float xDeltaMax, float lengthToVelocityRatioMin, float lengthToVelocityRatioMax, float slopeMin, float slopeMax)
    {
        this.xDeltaMin = xDeltaMin;
        this.xDeltaMax = xDeltaMax;
        this.lengthToVelocityRatioMin = lengthToVelocityRatioMin;
        this.lengthToVelocityRatioMax = lengthToVelocityRatioMax;
        this.slopeMin = slopeMin;
        this.slopeMax = slopeMax;
    }
}
