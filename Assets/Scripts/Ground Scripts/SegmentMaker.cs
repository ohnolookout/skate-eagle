using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Takes a CurveType and a startpoint or a CurveParameters and a startpoint and makes a corresponding GroundSegment
//StartLines and FinishLines should be their own classes that inherit from curve and only take startPoints.
public static class SegmentMaker
{
    public static GroundSegment GroundSegmentFromCurveType(GameObject newSegment, CurveType type, CurvePoint startPoint, Vector3? overlapPoint = null)
    {
        Curve curve = new Curve(type, startPoint); //This needs to be changed to something that pulls parameters from CurveTypes
        //If curveType = finish or start, have to hard code in options for puling those from child classes;
        //Once we have parameters, we can just call GroundSegmentFromParameters.
        GroundSegment groundSegment = newSegment.GetComponent<GroundSegment>();
        groundSegment.SetCurve(curve, startPoint, overlapPoint);
        return groundSegment;
    }

    public static GroundSegment GroundSegmentFromParameters(GameObject newSegment, CurveParameters parameters, CurvePoint startPoint, Vector3? overlapPoint = null)
    {
        Curve curve = new Curve(parameters, startPoint);
        GroundSegment groundSegment = newSegment.GetComponent<GroundSegment>();
        groundSegment.SetCurve(curve, startPoint, overlapPoint);
        return groundSegment;

    }
}
