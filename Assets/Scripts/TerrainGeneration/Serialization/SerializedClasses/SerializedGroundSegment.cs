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
    public bool isFloating;
    public bool isInverted;
    public bool hasShadow;
    public bool useDefaultHighLowPoints;

    //Curve contents
    public Curve curve;

    //Spline contents
    public List<SplineControlPoint> fillSplinePoints;
    public bool fillSpineIsOpen;
    public List<SplineControlPoint> edgeSplinePoints;

    //Collider contents
    public List<Vector2> colliderPoints;
    public List<Vector2> bottomColliderPoints;


    //CameraTargetable contents
    public LinkedCameraTarget linkedCameraTarget;

    public SerializedGroundSegment()
    {

    }
    public SerializedGroundSegment(List<CurvePoint> curvePoints, SerializedGround serializedGround, int index)
    {
        //Position
        name = serializedGround.name.Remove(1, serializedGround.name.Length - 2) + " Segment " + index;
        position = serializedGround.position;
        rotation = serializedGround.rotation;
        leftFloorHeight = curvePoints[0].FloorHeight;
        rightFloorHeight = curvePoints[^1].FloorHeight;
        leftFloorAngle = curvePoints[0].FloorAngle;
        rightFloorAngle = curvePoints[^1].FloorAngle;

        //State
        isStart = false;
        isFinish = false;
        isFloating = serializedGround.isFloating;
        isInverted = serializedGround.isInverted;
        hasShadow = serializedGround.hasShadow;
    }

    public SerializedGroundSegment(List<CurvePoint> curvePoints, Ground ground, int index)
    {
        //Position
        name = ground.gameObject.name.Remove(1, ground.gameObject.name.Length - 2) + " Segment " + index;
        position = ground.transform.position;
        rotation = ground.transform.rotation;
        leftFloorHeight = curvePoints[0].FloorHeight;
        rightFloorHeight = curvePoints[^1].FloorHeight;
        leftFloorAngle = curvePoints[0].FloorAngle;
        rightFloorAngle = curvePoints[^1].FloorAngle;

        //State
        isStart = false;
        isFinish = false;
        isFloating = ground.IsFloating;
        isInverted = ground.IsInverted;
        hasShadow = ground.HasShadow;

    }

    public GroundSegment Deserialize(GameObject targetObject, GameObject contextObject)
    {
        var segment = targetObject.GetComponent<GroundSegment>();
        var ground = contextObject.GetComponent<Ground>();

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
        segment.NextLeftSegment = ground.SegmentList.Count == 0 ? null : ground.SegmentList[^1];

        //segment.Curve = curve;
        segment.IsFinish = isFinish;
        segment.IsStart = isStart;
        segment.IsFloating = isFloating;
        segment.IsInverted = isInverted;
        segment.HasShadow = hasShadow;
        segment.UseDefaultHighLowPoints = useDefaultHighLowPoints;
        segment.UpdateShadow();
        //segment.UpdateHighLowTransforms();

        GroundSplineUtility.GenerateSpline(segment.Spline, fillSplinePoints, fillSpineIsOpen);
        GroundSplineUtility.GenerateSpline(segment.EdgeSpline, edgeSplinePoints, true);

        //Create collierPoints
        segment.Collider.points = colliderPoints.ToArray();
        segment.Collider.sharedMaterial = segment.parentGround.ColliderMaterial;

        if (!segment.IsFloating)
        {
            segment.BottomCollider.points = bottomColliderPoints.ToArray();
            segment.BottomCollider.sharedMaterial = segment.parentGround.ColliderMaterial;
        }
        else
        {
            segment.BottomCollider.gameObject.SetActive(false);
        }

        //Camera targets
        segment.LinkedCameraTarget = linkedCameraTarget;

        return segment;
    }
}
