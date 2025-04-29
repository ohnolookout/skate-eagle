using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using UnityEngine.UIElements;
using Com.LuisPedroFonseca.ProCamera2D;
using System.Linq;
using System;

public static class SerializeLevelUtility
{
    #region Serialization
    public static List<SerializedGround> SerializeGroundList(Ground[] grounds)
    {
        var serializedGrounds = new List<SerializedGround>();

        GenerateGroundIndices(grounds);

        foreach (Ground ground in grounds)
        {
            serializedGrounds.Add(SerializeGround(ground));
        }

        return serializedGrounds;
    }
    private static void GenerateGroundIndices(Ground[] grounds)
    {
        for (int i = 0; i < grounds.Length; i++)
        {
            var ground = grounds[i];
            for (int j = 0; j < ground.SegmentList.Count; j++)
            {
                ground.SegmentList[j].LinkedCameraTarget.SerializedLocation = new int[2] { i, j };
            }
        }
    }

    private static SerializedGround SerializeGround(Ground ground)
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

    private static SerializedGroundSegment SerializeGroundSegment(GroundSegment segment)
    {
        BuildLinkedCameraTarget(segment);

        SerializedGroundSegment serializedSegment = new();

        //Transform
        serializedSegment.name = segment.gameObject.name;
        serializedSegment.position = segment.transform.position;
        serializedSegment.rotation = segment.transform.rotation;

        //Segment
        serializedSegment.isStart = segment.IsStart;
        serializedSegment.isFinish = segment.IsFinish;
        serializedSegment.isFloating = segment.IsFloating;
        serializedSegment.hasShadow = segment.HasShadow;

        //Curve
        segment.Curve.LowPoint = segment.LowPoint.position - segment.transform.position;
        segment.Curve.HighPoint = segment.HighPoint.position - segment.transform.position;
        serializedSegment.curve = segment.Curve;


        //Spline
        serializedSegment.fillSplinePoints = CopySplinePoints(segment.Spline);
        serializedSegment.fillSpineIsOpen = segment.Spline.isOpenEnded;
        serializedSegment.edgeSplinePoints = CopySplinePoints(segment.EdgeSpline);

        //Collider
        serializedSegment.colliderPoints = CopyColliderPoints(segment.Collider);
        serializedSegment.bottomColliderPoints = CopyColliderPoints(segment.BottomCollider);

        //CameraTargetable
        serializedSegment.linkedCameraTarget = segment.LinkedCameraTarget;
        //Debug.Log("Serializing segment with " + segment.LinkedCameraTarget.LeftTargets.Count + " left targets and " + segment.LinkedCameraTarget.RightTargets.Count + " right targets");

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

    public static Vector2[] SerializeFinishLine(FinishLine finishLine)
    {
        if(finishLine == null)
        {
            Debug.Log("Finish line is null! Idiot!");
            return null;
        }

        return new Vector2[2]
        {
            finishLine.FlagPosition,
            finishLine.BackstopPosition
        };
    }

    private static void BuildLinkedCameraTarget(ICameraTargetable targetable)
    {
        if(targetable.LinkedCameraTarget == null)
        {
            targetable.LinkedCameraTarget = new();
        }

        targetable.PopulateDefaultTargets();
        targetable.LinkedCameraTarget.LeftTargets = GetTargetableList(targetable.LeftTargetObjects);
        targetable.LinkedCameraTarget.RightTargets = GetTargetableList(targetable.RightTargetObjects);
    }

    private static List<LinkedCameraTarget> GetTargetableList(List<GameObject> targetObjects)
    {
        targetObjects = targetObjects.Distinct().ToList(); //Remove duplicates
        var targetables = new List<LinkedCameraTarget>();
        for (int i = 0; i < targetObjects.Count; i++)
        {
            if(targetObjects[i] == null)
            {
                targetObjects.RemoveAt(i);
                i--;
                continue;
            }
            var linkedTarget = targetObjects[i].GetComponent<ICameraTargetable>();
            if (linkedTarget != null)
            {
                targetables.Add(linkedTarget.LinkedCameraTarget);
            }
            else
            {
                targetObjects.RemoveAt(i);
                i--;
            }
        }

        return targetables;
    }

    #endregion

    #region Deserialization
    public static void DeserializeLevel(Level level, GroundManager groundManager)
    {
        groundManager.ClearGround();

        foreach (var serializedGround in level.SerializedGrounds)
        {
            var ground = DeserializeGround(serializedGround, groundManager);
            groundManager.Grounds.Add(ground);
        }
        if (level.FinishLineParameters != null && level.FinishLineParameters.Length == 2) { 
            groundManager.groundSpawner.SetFinishLine(level.FinishLineParameters[0], level.FinishLineParameters[1], level.BackstopIsActive);
        }


        if (Application.isPlaying)
        {
            return;
        }

        //Retrieve targetable objects from groundManager and reassociate targeted game objects for editing in inspector
        var targetables = GetAllTargetables(groundManager);

        foreach (var targetable in targetables)
        {
            ReassociateGameObjects(targetable, groundManager);
        }

    }
    private static Ground DeserializeGround(SerializedGround serializedGround, GroundManager groundManager)
    {
        var groundSpawner = groundManager.groundSpawner;

        var ground = groundSpawner.AddGround();
        ground.name = serializedGround.name;
        ground.SegmentList = new();
        foreach (var serializedSegment in serializedGround.segmentList)
        {
            var segment = groundSpawner.AddEmptySegment(ground);
            DeserializeSegment(serializedSegment, segment, ground, ground.SegmentList.Count == 0 ? null : ground.SegmentList[^1]);
            if(segment.PreviousSegment != null)
            {
                segment.PreviousSegment.NextSegment = segment;
            }
            ground.SegmentList.Add(segment);

            if(segment.IsStart)
            {
                groundManager.StartSegment = segment;
            }

            if (segment.IsFinish)
            {
                groundManager.FinishSegment = segment;
            }
        }

        return ground;
    }

    private static void DeserializeSegment(SerializedGroundSegment serializedSegment, GroundSegment segment, Ground parent, GroundSegment? previousSegment)
    {
        segment.transform.position = serializedSegment.position;
        segment.transform.rotation = serializedSegment.rotation;
        segment.gameObject.name = serializedSegment.name;

        segment.parentGround = parent;
        segment.PreviousSegment = previousSegment;
        segment.Curve = serializedSegment.curve;
        segment.IsFinish = serializedSegment.isFinish;
        segment.IsStart = serializedSegment.isStart;
        segment.IsFloating = serializedSegment.isFloating;
        segment.HasShadow = serializedSegment.hasShadow;
        segment.UpdateShadow();

        segment.UpdateHighLowTransforms();

        GroundSplineUtility.GenerateSpline(segment.Spline, serializedSegment.fillSplinePoints, serializedSegment.fillSpineIsOpen);
        GroundSplineUtility.GenerateSpline(segment.EdgeSpline, serializedSegment.edgeSplinePoints, true);

        //Create collierPoints
        segment.Collider.points = serializedSegment.colliderPoints.ToArray();
        segment.Collider.sharedMaterial = parent.ColliderMaterial;
        segment.BottomCollider.points = serializedSegment.bottomColliderPoints.ToArray();
        segment.BottomCollider.sharedMaterial = parent.ColliderMaterial;

        //Camera targets
        segment.LinkedCameraTarget = serializedSegment.linkedCameraTarget;

    }

    private static List<ICameraTargetable> GetAllTargetables(GroundManager groundManager)
    {
        var targetables = new List<ICameraTargetable>();

        foreach (var ground in groundManager.Grounds)
        {
            foreach (var segment in ground.SegmentList)
            {
                targetables.Add(segment);
            }
        }

        //Expand with additional types as added
        //Debug.Log(targetables.Count + " targetables found");
        return targetables;
    }

    private static void ReassociateGameObjects(ICameraTargetable targetable, GroundManager groundManager)
    {
        //Iterate through all objects of groundManager and relink gameObjects in left and right target objects
        //by using indices in serialized left and right targets

        if (targetable == null)
        {
            Debug.Log("Targetable is null");
            return;
        }

        targetable.LeftTargetObjects = BuildTargetObjectList(targetable.LinkedCameraTarget.LeftTargets, groundManager);
        targetable.RightTargetObjects = BuildTargetObjectList(targetable.LinkedCameraTarget.RightTargets, groundManager);    

    }

    private static List<GameObject> BuildTargetObjectList(List<LinkedCameraTarget> linkedTargets, GroundManager groundManager)
    {
        List<GameObject> gameObjects = new();

        foreach (var target in linkedTargets)
        {
            var obj = groundManager.GetGameObjectByIndices(target.SerializedLocation);

            if (obj != null)
            {
                gameObjects.Add(obj);
            }
        }
        return gameObjects;
    }

    #endregion
}
