using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedCurve : Curve
{
    public FixedCurve(FixedCurveDefinition curveDef, CurvePoint startPoint)
    {
        curveDefinition = curveDef;
        curvePoints = CurvePointsFromDefinition(curveDef, -startPoint.LeftTangent);
        curveType = CurveType.Fixed;
        GenerateCurveStats();
    }
}
