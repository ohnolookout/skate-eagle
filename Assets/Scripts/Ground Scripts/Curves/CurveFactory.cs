using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveFactory
{
    public static Curve CurveFromParameters(CurveParameters parameters, CurvePoint startPoint)
    {
        return new CustomCurve(parameters, startPoint);
    }

    public static Curve CurveFromType(CurveType type, CurvePoint startPoint)
    {
        return type switch
        {
            CurveType.StartLine => new StartLineCurve(startPoint),
            CurveType.FinishLine => new FinishLineCurve(startPoint),
            CurveType.Roller => new CustomCurve(CurveTypes.Roller(), startPoint),
            CurveType.SmallRoller => new CustomCurve(CurveTypes.SmallRoller(), startPoint),
            _ => new CustomCurve(CurveTypes.Roller(), startPoint),
        };
    }

}
