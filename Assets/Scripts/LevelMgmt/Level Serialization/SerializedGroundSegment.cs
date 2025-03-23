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
    public bool isFloating;
    public bool hasShadow;

    //Curve contents
    public Curve curve;
    //Spline contents
    public List<SplineControlPoint> fillSplinePoints;
    public bool fillSpineIsOpen;
    public List<SplineControlPoint> edgeSplinePoints;

    //Collider contents
    public List<Vector2> colliderPoints;

    public SerializedGroundSegment()
    {
    }
}
