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

    public static Curve CurveFromDefinition(ProceduralCurveDefinition definition, CurvePoint startPoint, float climbMin, float climbMax)
    {
        return new ProceduralCurve(definition, startPoint, climbMin, climbMax);
    }
    public static Curve FixedCurve(List<FixedHalfCurve> halfCurves, CurvePoint startPoint)
    {
        return new FixedCurve(halfCurves, startPoint);
    }
}
