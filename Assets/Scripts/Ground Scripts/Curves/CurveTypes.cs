using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveTypes
{

    public static CurveParameters Roller()
    {
        CurveParameters parameters = new();
        parameters.xDeltaMin = 20;
        parameters.xDeltaMax = 50;
        parameters.lengthToVelocityRatioMin = 0.2f;
        parameters.lengthToVelocityRatioMax = 0.4f;
        parameters.slopeMin = 0.4f;
        parameters.slopeMax = 1.8f;
        return parameters;
    }

    public static CurveParameters SmallRoller()
    {
        CurveParameters parameters = new();
        parameters.xDeltaMin = 7;
        parameters.xDeltaMax = 22;
        parameters.lengthToVelocityRatioMin = 0.2f;
        parameters.lengthToVelocityRatioMax = 0.4f;
        parameters.slopeMin = 0.1f;
        parameters.slopeMax = 1.2f;
        return parameters;
    }
}
