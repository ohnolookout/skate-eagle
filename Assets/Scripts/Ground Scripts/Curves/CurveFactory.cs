using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveFactory
{

    public static Curve CurveFromType(CurveType type, CurvePoint startPoint, float climbMin = 0, float climbMax = 0)
    {
        return type switch
        {
            CurveType.StartLine => new StartLineCurve(startPoint),
            CurveType.FinishLine => new FinishLineCurve(startPoint),
            CurveType.Roller => new CustomCurve(CurveTypes.Roller(), startPoint, climbMin, climbMax),
            CurveType.SmallRoller => new CustomCurve(CurveTypes.SmallRoller(), startPoint, climbMin, climbMax),
            _ => new CustomCurve(CurveTypes.Roller(), startPoint, climbMin, climbMax)
        };
    }

    public static Curve FinishLine(CurvePoint startPoint)
    {
        return new FinishLineCurve(startPoint);
    }

    public static Curve StartLine(CurvePoint startPoint)
    {
        return new StartLineCurve(startPoint);
    }

    public static Curve CurveFromParameters(CurveParameters[] parameters, CurvePoint startPoint, float climbMin, float climbMax)
    {
        return new CustomCurve(parameters, startPoint, climbMin, climbMax);
    }

    public static Curve CurveFromDefinition(CurveDefinition[] definitions, CurvePoint startPoint, float climbMin, float climbMax)
    {
        CurveParameters valleyParameters = new(definitions[0]);
        CurveParameters peakParameters = new(definitions[1]);
        return CurveFromParameters(new CurveParameters[] { valleyParameters, peakParameters }, startPoint, climbMin, climbMax);
    }

    public static Curve CurveFromCombinedDefinition(CombinedCurveDefinition definition, CurvePoint startPoint, float climbMin, float climbMax)
    {
        CurveParameters valleyParameters = new(definition.Valley);
        CurveParameters peakParameters = new(definition.Peak);
        return new CustomCurve(new CurveParameters[] { valleyParameters, peakParameters }, startPoint, climbMin, climbMax);
    }
    

}
