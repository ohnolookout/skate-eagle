using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveFactory
{
    public static Curve FinishLine(CurvePoint startPoint)
    {
        return new FinishLineCurve(startPoint);
    }

    public static Curve StartLine()
    {
        return new StartLineCurve();
    }

    public static Curve CurveFromDefinition(CurveDefinition definition, CurvePoint startPoint, float climbMin, float climbMax)
    {
        return new CustomCurve(definition, startPoint, climbMin, climbMax);
    }
}
