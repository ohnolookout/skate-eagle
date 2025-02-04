using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralCurve : Curve
{
    public new ProceduralCurveDefinition curveDefinition;

    public ProceduralCurve(ProceduralCurveDefinition curveDef, CurvePoint startPoint)
    {
        curveDefinition = curveDef;
        //Use inverted left tangent from last point as starting right tangent
        curvePoints = curveDefinition.GenerateCurvePoints(-startPoint.LeftTangent);
        //curvePoints = CurvePointsFromDefinition(curveDefinition, -startPoint.LeftTangent);
        curveType = CurveType.Procedural;
        GenerateCurveStats();

    }



}
