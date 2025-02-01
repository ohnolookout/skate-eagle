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

    public static Curve CurveFromDefinition(CurveDefinition definition, CurvePoint startPoint)
    {
        if (definition.GetType() == typeof(ProceduralCurveDefinition))
        {
            return CurveFromProceduralDefinition((ProceduralCurveDefinition)definition, startPoint);
        }
        else
        {
            return CurveFromFixedDefinition((FixedCurveDefinition)definition, startPoint);
        }
    }

    public static Curve DefaultFixedCurve(CurvePoint startPoint)
    {
        return new FixedCurve(new(), startPoint);
    }

    public static Curve DefaultProceduralCurve(CurvePoint startPoint)
    {
        return new ProceduralCurve(new(), startPoint);
    }

    public static Curve CurveFromProceduralDefinition(ProceduralCurveDefinition definition, CurvePoint startPoint)
    {
        return new ProceduralCurve(definition, startPoint);
    }

    public static Curve CurveFromFixedDefinition(FixedCurveDefinition definition, CurvePoint startPoint)
    {
        return new FixedCurve(definition, startPoint);
    }

}
