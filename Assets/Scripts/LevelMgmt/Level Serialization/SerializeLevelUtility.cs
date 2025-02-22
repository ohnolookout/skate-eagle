using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using UnityEngine.UIElements;

public static class SerializeLevelUtility
{
    public static List<SerializedGround> SerializeGroundList(Ground[] grounds)
    {
        var serializedGrounds = new List<SerializedGround>();

        foreach (Ground ground in grounds)
        {
            serializedGrounds.Add(SerializeGround(ground));
        }

        return serializedGrounds;
    }

    public static SerializedGround SerializeGround(Ground ground)
    {
        var name = ground.gameObject.name;
        var position = ground.transform.position;
        var segmentList = new List<SerializedGroundSegment>();
        foreach (GroundSegment segment in ground.SegmentList)
        {
            segmentList.Add(SerializeGroundSegment(segment));
        }
        return new SerializedGround(name, position, segmentList);
    }

    public static SerializedGroundSegment SerializeGroundSegment(GroundSegment segment)
    {
        SerializedGroundSegment serializedSegment = new();

        //Transform
        serializedSegment.name = segment.gameObject.name;
        serializedSegment.position = segment.transform.position;
        serializedSegment.rotation = segment.transform.rotation;

        //Segment
        serializedSegment.isStart = segment.IsStart;
        serializedSegment.isFinish = segment.IsFinish;

        //Curve
        serializedSegment.curve = segment.Curve;


        //Spline
        serializedSegment.fillSplinePoints = CopySplinePoints(segment.Spline);
        serializedSegment.fillSpineIsOpen = segment.Spline.isOpenEnded;
        serializedSegment.edgeSplinePoints = CopySplinePoints(segment.EdgeSpline);

        //Collider
        serializedSegment.colliderPoints = CopyColliderPoints(segment.Collider);

        return serializedSegment;
    }

    private static List<SplineControlPoint> CopySplinePoints(Spline splineToCopy)
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

    private static List<Vector2> CopyColliderPoints(EdgeCollider2D colliderToCopy)
    {
        var pointsList = new List<Vector2>();
        for (int i = 0; i < colliderToCopy.points.Length; i++)
        {
            pointsList.Add(colliderToCopy.points[i]);
        }
        return pointsList;
    }

    #region Deserialization
    public static void DeserializeLevel(Level level, GroundManager groundManager)
    {
        groundManager.ClearGround();

        foreach (var serializedGround in level.SerializedGrounds)
        {
            DeserializeGround(serializedGround, groundManager.groundSpawner);
        }

    }
    public static void DeserializeGround(SerializedGround serializedGround, GroundSpawner groundSpawner)
    {
        var ground = groundSpawner.AddGround();
        ground.name = serializedGround.name;
        ground.SegmentList = new();
        foreach (var serializedSegment in serializedGround.segmentList)
        {
            var segment = groundSpawner.AddEmptySegment(ground);
            DeserializeSegment(serializedSegment, segment, ground, ground.SegmentList.Count == 0 ? null : ground.SegmentList[^1]);
            ground.SegmentList.Add(segment);
            if (segment.IsStart)
            {
                groundSpawner.SetStartPoint(segment, 1);
            }
            if (segment.IsFinish)
            {
                groundSpawner.SetFinishPoint(segment, 1);
            }
        }
    }

    public static void DeserializeSegment(SerializedGroundSegment serializedSegment, GroundSegment segment, Ground parent, GroundSegment? previousSegment)
    {
        segment.transform.position = serializedSegment.position;
        segment.transform.rotation = serializedSegment.rotation;
        segment.gameObject.name = serializedSegment.name;

        segment.parentGround = parent;
        segment.PreviousSegment = previousSegment;
        segment.Curve = serializedSegment.curve;
        segment.isFinish = serializedSegment.isFinish;
        segment.isFinish = serializedSegment.isStart;

        GroundSplineUtility.GenerateSpline(segment.Spline, serializedSegment.fillSplinePoints, serializedSegment.fillSpineIsOpen);
        GroundSplineUtility.GenerateSpline(segment.EdgeSpline, serializedSegment.edgeSplinePoints, true);

        //Create collierPoints
        segment.Collider.points = serializedSegment.colliderPoints.ToArray();
        segment.Collider.sharedMaterial = parent.ColliderMaterial;

    }

    #endregion
}
