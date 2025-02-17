using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[Serializable]
public class SerializedGroundSegment
{
    //Transform contents
    public string name;
    public Vector3 position;
    public Quaternion rotation;

    //Segment contents
    public bool isStart;
    public bool isFinish;

    //Curve contents
    public CurveDefinition curveDefinition;
    public List<CurvePoint> curvePoints;
    public List<float> curveSectionLengths;
    public Vector3 highPoint;
    public Vector3 lowPoint;

    //Spline contents
    public List<SplineControlPoint> fillSplinePoints;
    public bool fillSpineIsOpen;
    public List<SplineControlPoint> edgeSplinePoints;

    //Collider contents
    public List<Vector2> colliderPoints;

    public SerializedGroundSegment(GroundSegment segment)
    {
        //Transform
        name = segment.gameObject.name;
        position = segment.transform.position;
        rotation = segment.transform.rotation;

        //Segment
        isStart = segment.IsStart;
        isFinish = segment.IsFinish;

        //Curve
        curveDefinition = segment.Curve.curveDefinition;
        curveSectionLengths = segment.Curve.SectionLengths;
        highPoint = segment.Curve.Highpoint;
        lowPoint = segment.Curve.Lowpoint;


        //Spline
        fillSplinePoints = CopySplinePoints(segment.Spline);
        fillSpineIsOpen = segment.Spline.isOpenEnded;
        edgeSplinePoints = CopySplinePoints(segment.EdgeSpline);

        //Collider
        colliderPoints = CopyColliderPoints(segment.Collider);
    }

    private List<SplineControlPoint> CopySplinePoints(Spline splineToCopy)
    {
        var pointsList = new List<SplineControlPoint>();
        for (int i = 0; i < splineToCopy.GetPointCount(); i++)
        {
            SplineControlPoint newPoint = new();
            newPoint.position = splineToCopy.GetPosition(i);
            newPoint.leftTangent = splineToCopy.GetLeftTangent(i);
            newPoint.rightTangent = splineToCopy.GetRightTangent(i);
            newPoint.mode = splineToCopy.GetTangentMode(i);
            pointsList.Add(newPoint);
        }

        return pointsList;

    }

    private List<Vector2> CopyColliderPoints(EdgeCollider2D colliderToCopy)
    {
        var pointsList = new List<Vector2>();
        for (int i = 0; i < colliderToCopy.points.Length; i++)
        {
            pointsList.Add(colliderToCopy.points[i]);
        }
        return pointsList;
    }
}
