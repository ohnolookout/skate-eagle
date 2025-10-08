using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.Rendering.HableCurve;

[Serializable]
public class SerializedGroundSegment
{
    //Transform contents
    public string name;
    public Vector3 position;

    //Floor contents
    public Vector3 floorPositions;
    public int leftFloorHeight;
    public int leftFloorAngle;
    public int rightFloorHeight;
    public int rightFloorAngle;
    public Vector3 startFloor;
    public Vector3 endFloor;

    //Segment contents
    public bool isStart;
    public bool isFinish;

    //Curve contents
    public Curve curve;
    public int startPointIndex;
    public int endPointIndex;
    public int startLowPointIndex = -1;

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

    //Constructors
    public SerializedGroundSegment(string name, SerializedGround serializedGround, List<CurvePoint> curvePoints, 
        Vector3? firstColliderPoint, bool isFirst, bool isLast)
    {
        //Position
        this.name = name;
        position = serializedGround.position;

        //State
        isStart = false;
        isFinish = false;

        if (curvePoints == null || curvePoints.Count < 2)
        {
            return;
        }

        //Curve points
        edgeSplineCurvePoints = SerializeLevelUtility.DeepCopyCurvePoints(curvePoints);
        fillSplineCurvePoints = SerializeLevelUtility.DeepCopyCurvePoints(curvePoints);
        startPointIndex = serializedGround.curvePoints.IndexOf(curvePoints[0]);
        endPointIndex = serializedGround.curvePoints.IndexOf(curvePoints[^1]);

        var lowPoints = curvePoints.Where(cp => cp.LinkedCameraTarget.doLowTarget).ToList();
        if (lowPoints.Count > 0)
        {
            startLowPointIndex = serializedGround.lowPoints.IndexOf(lowPoints[0]);
        }
        else
        {
            startLowPointIndex = FindFirstLowPointIndex(serializedGround.curvePoints, serializedGround.lowPoints, startPointIndex);
            if(startLowPointIndex == -1)
            {
                Debug.LogWarning($"SerializedGroundSegment: No low point camera targets found for segment {name}. Add some.");
            }
        }

        if (!serializedGround.IsFloating)
        {
            GroundSplineUtility.AddFloorPoints(fillSplineCurvePoints, GetFloorPositions(curvePoints));
        }        

        //Collider
        colliderPoints = ColliderGenerator.GetEdgeColliderPoints(curvePoints, firstColliderPoint, serializedGround.isInverted);

        if (!serializedGround.IsFloating)
        {
            bottomColliderPoints = ColliderGenerator.GetBottomColliderPoints(fillSplineCurvePoints, colliderPoints, curvePoints.Count, isFirst, isLast);
        }
        
    }

    private int FindFirstLowPointIndex(List<CurvePoint> curvePoints, List<CurvePoint> lowPoints, int startIndex)
    {
        for(int i = startIndex; i > 0; i--)
        {
            if(curvePoints[i].LinkedCameraTarget.doLowTarget)
            {
                return lowPoints.IndexOf(curvePoints[i]);
            }
        }

        for(int i = startIndex; i < curvePoints.Count; i++)
        {
            if (curvePoints[i].LinkedCameraTarget.doLowTarget)
            {
                return lowPoints.IndexOf(curvePoints[i]);
            }
        }

        return -1;
    }

    private List<Vector3> GetFloorPositions(List<CurvePoint> curvePoints)
    {
        List<Vector3> floorPositions = new();
        foreach(var point in curvePoints)
        {
            if(point.FloorPointType != FloorPointType.None)
            {
                floorPositions.Add(point.FloorPosition);
            }
        }
        return floorPositions;
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
        segment.gameObject.name = name;
        segment.LeftFloorHeight = leftFloorHeight;
        segment.RightFloorHeight = rightFloorHeight;
        segment.LeftFloorAngle = leftFloorAngle;
        segment.RightFloorAngle = rightFloorAngle;
        segment.StartPointIndex = startPointIndex;
        segment.EndPointIndex = endPointIndex;
        segment.StartLowPointIndex = startLowPointIndex;

        segment.parentGround = ground;

        if (fillSplineCurvePoints == null || fillSplineCurvePoints.Count < 2)
        {
            return segment;
        }

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
