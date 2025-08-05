using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.Rendering.HableCurve;

[Serializable]
public class SerializedGroundSegment
{
    //Transform contents
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public int leftFloorHeight;
    public int leftFloorAngle;
    public int rightFloorHeight;
    public int rightFloorAngle;

    //Segment contents
    public bool isStart;
    public bool isFinish;

    //Curve contents
    public Curve curve;

    //Spline contents
    public List<CurvePoint> fillSplineCurvePoints;
    public List<CurvePoint> edgeSplineCurvePoints;
    public List<SplineControlPoint> fillSplinePoints;
    public bool fillSpineIsOpen;
    public List<SplineControlPoint> edgeSplinePoints;

    //Collider contents
    public Vector2[] colliderPoints;
    public Vector2[] bottomColliderPoints;


    //CameraTargetable contents
    public LinkedCameraTarget linkedCameraTarget;

    public SerializedGroundSegment()
    {

    }
    public SerializedGroundSegment(string name, Vector3 position, Quaternion rotation, 
        List<CurvePoint> curvePoints, Vector3? firstColliderPoint, 
        bool isFloating, bool isInverted, bool isFirst, bool isLast)
    {
        //Position
        this.name = name;
        this.position = position;
        this.rotation = rotation;
        leftFloorHeight = curvePoints[0].FloorHeight;
        rightFloorHeight = curvePoints[^1].FloorHeight;
        leftFloorAngle = curvePoints[0].FloorAngle;
        rightFloorAngle = curvePoints[^1].FloorAngle;

        //State
        isStart = false;
        isFinish = false;

        //Curve points
        edgeSplineCurvePoints = SerializeLevelUtility.DeepCopyCurvePoints(curvePoints);
        fillSplineCurvePoints = SerializeLevelUtility.DeepCopyCurvePoints(curvePoints);

        if (!isFloating)
        {
            GroundSplineUtility.AddCornerPoints(fillSplineCurvePoints);
        }        

        //Collider
        colliderPoints = ColliderGenerator.GetEdgeColliderPoints(curvePoints, firstColliderPoint, isInverted);

        if (!isFloating)
        {
            bottomColliderPoints = ColliderGenerator.GetBottomColliderPoints(fillSplineCurvePoints, colliderPoints, isFirst, isLast);
        }


    }

    public GroundSegment Deserialize(GroundSegment segment, Ground ground)
    {

        if (segment == null) {
            Debug.LogWarning("SerializedGroundSegment: Deserialize called on a GameObject that does not have a GroundSegment component.");
                return null;
        }

        if (ground == null) {
            Debug.LogWarning("SerializedGroundSegment: Deserialize called with a context GameObject that does not have a Ground component.");
        }

        segment.transform.position = position;
        segment.transform.rotation = rotation;
        segment.gameObject.name = name;
        segment.LeftFloorHeight = leftFloorHeight;
        segment.RightFloorHeight = rightFloorHeight;
        segment.LeftFloorAngle = leftFloorAngle;
        segment.RightFloorAngle = rightFloorAngle;

        segment.parentGround = ground;

        segment.ActivateShadow(ground.HasShadow);

        GroundSplineUtility.GenerateSpline(segment.Spline, fillSplineCurvePoints, ground.IsFloating);
        GroundSplineUtility.GenerateSpline(segment.EdgeSpline, edgeSplineCurvePoints, true);

        //Create collierPoints
        segment.Collider.points = colliderPoints;
        segment.Collider.sharedMaterial = segment.parentGround.ColliderMaterial;

        if (!ground.IsFloating)
        {
            segment.BottomCollider.points = bottomColliderPoints;
            segment.BottomCollider.sharedMaterial = segment.parentGround.ColliderMaterial;
        }
        else
        {
            segment.BottomCollider.gameObject.SetActive(false);
        }


        return segment;
    }
}
