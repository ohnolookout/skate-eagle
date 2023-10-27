using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveFactory
{
    public static Curve FinishLine(CurvePoint startPoint)
    {
        return new FinishLineCurve(startPoint);
    }

    public static Curve StartLine(CurvePoint startPoint)
    {
        return new StartLineCurve(startPoint);
    }

    public static Curve CurveFromCombinedDefinition(CurveDefinition definition, CurvePoint startPoint, float climbMin, float climbMax)
    {
        CurveParameters valley = new(definition.Valley);
        CurveParameters peak = new(definition.Peak);
        return new CustomCurve(new CurveParameters[] { valley, peak }, startPoint, climbMin, climbMax);
    }

    public static Curve CompoundCurve(CurveDefinition definition, CurvePoint startPoint, float climbMin, float climbMax)
    {
        return new CustomCurve(definition, startPoint, climbMin, climbMax);
    }
}
