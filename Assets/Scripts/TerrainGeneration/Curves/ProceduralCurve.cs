using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralCurve : Curve
{

    public ProceduralCurve(ProceduralCurveDefinition curveDef, CurvePoint startPoint)
    {
        curveDefinition = curveDef;
        //Use inverted left tangent from last point as starting right tangent
        curvePoints = CurvePointsFromDefinition(curveDef, -startPoint.LeftTangent);
        curveType = CurveType.Procedural;
        GenerateCurveStats();

    }



}
