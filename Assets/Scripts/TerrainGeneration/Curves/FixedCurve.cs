using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedCurve : Curve
{
    new FixedCurveDefinition curveDefinition;
    public FixedCurve(FixedCurveDefinition curveDef, CurvePoint startPoint)
    {
        curveDefinition = curveDef;
        curvePoints = CurvePointsFromDefinition(curveDefinition, -startPoint.LeftTangent);
        curveType = CurveType.Fixed;
        GenerateCurveStats();
    }
}
