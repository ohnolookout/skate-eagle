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

    public static List<IDeserializable> SerializeGroundManager(GroundManager groundManager)
    {
        GenerateGroundIndices(groundManager.Grounds);
        var serializables = new List<ISerializable>();
        for (int i = 0; i < groundManager.transform.childCount; i++)
        {
            var child = groundManager.transform.GetChild(i);
            if (child.TryGetComponent(out ISerializable serializable))
            {
                serializables.Add(serializable);
            }
        }

        for (int i = 0; i < groundManager.groundContainer.transform.childCount; i++)
        {
            var child = groundManager.groundContainer.transform.GetChild(i);
            if (child.TryGetComponent(out ISerializable serializable))
            {
                serializables.Add(serializable);
            }
        }

        List<IDeserializable> serializedObjects = new();

        foreach(var serializable in serializables)
        {
            serializedObjects.Add(serializable.Serialize());
        }

        return serializedObjects;
    }
    private static void GenerateGroundIndices(List<Ground> grounds)
    {
        for (int i = 0; i < grounds.Count; i++)
        {
            var ground = grounds[i];
            for (int j = 0; j < ground.SegmentList.Count; j++)
            {
                ground.SegmentList[j].LinkedCameraTarget.SerializedLocation = new int[2] { i, j };
            }
        }
    }

    public static List<SplineControlPoint> CopySplinePoints(Spline splineToCopy)
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

    public static void SerializeGroundSegments(SerializedGround serializedGround)
    {
        Debug.Log($"Populating segments from {serializedGround.curvePoints.Count} curve points for serialized ground: {serializedGround.name}");

        serializedGround.segmentList = new();
        var groundNamePrefix = serializedGround.name.Remove(1, serializedGround.name.Length - 2);

        serializedGround.editorSegment = new(groundNamePrefix + " Editor Segment", serializedGround.position, 
            serializedGround.rotation, serializedGround.curvePoints, null, 
            serializedGround.isFloating, serializedGround.isInverted, true, true);

        var segmentedCurvePoints = BreakDownSegments(serializedGround.curvePoints);

        for (int i = 0; i < segmentedCurvePoints.Count; i++)
        {
            var name = groundNamePrefix + " Segment " + i;
            Vector3? lastColliderPoint = serializedGround.segmentList.Count > 0 ? serializedGround.segmentList[^1].colliderPoints[^1] : null;

            var isFirst = i == 0;
            var isLast = i == segmentedCurvePoints.Count - 1;

            var serializedSegment = new SerializedGroundSegment(name, serializedGround.position, 
                serializedGround.rotation, segmentedCurvePoints[i], lastColliderPoint,
                serializedGround.isFloating, serializedGround.isInverted, isFirst, isLast);

            serializedGround.segmentList.Add(serializedSegment);
        }

    }
    
    private const float MaxSectionDistance = 80; // Maximum distance between points in a section
    private static List<List<CurvePoint>> BreakDownSegments(List<CurvePoint> allCurvePoints)
    {
        allCurvePoints = DeepCopyCurvePoints(allCurvePoints);

        List<List<CurvePoint>> sections = new();
        if (allCurvePoints.Count < 3)
        {
            sections.Add(allCurvePoints);
            return sections;
        }

        List<CurvePoint> currentSection = new();
        var startPoint = allCurvePoints[0].Position;

        for (int i = 0; i < allCurvePoints.Count; i++)
        {
            var curvePoint = allCurvePoints[i];
            // Check if the distance from the start point exceeds the maximum section distance
            // Also, check to make sure we are not at the last point
            if (i < allCurvePoints.Count - 1 &&
                (currentSection.Count > 4 ||
                (currentSection.Count > 0 && Vector2.Distance(startPoint, curvePoint.Position) > MaxSectionDistance)))
            {
                currentSection.Add(curvePoint);
                sections.Add(currentSection);
                currentSection = new List<CurvePoint>();
                startPoint = curvePoint.Position;
            }
            currentSection.Add(curvePoint);
        }

        sections.Add(currentSection);

        return sections;
    }

    public static List<Vector2> CopyColliderPoints(EdgeCollider2D colliderToCopy)
    {
        var pointsList = new List<Vector2>();
        for (int i = 0; i < colliderToCopy.points.Length; i++)
        {
            pointsList.Add(colliderToCopy.points[i]);
        }
        return pointsList;
    }

    public static void BuildLinkedCameraTarget(ICameraTargetable targetable)
    {
        if(targetable.LinkedCameraTarget == null)
        {
            Debug.LogWarning("LinkedCameraTarget is null, creating new one");
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

    public static List<CurvePoint> DeepCopyCurvePoints(List<CurvePoint> curvePoints)
    {
        List<CurvePoint> copiedCurvePoints = new();
        foreach (var curvePoint in curvePoints)
        {
            copiedCurvePoints.Add(curvePoint.DeepCopy());
        }
        return copiedCurvePoints;
    }

    #endregion

    #region Deserialization
    public static void DeserializeLevel(Level level, GroundManager groundManager, LevelManager levelManager = null)
    {
        groundManager.ClearGround();
        var groundSpawner = groundManager.groundSpawner;

        foreach (var serializedObject in level.SerializedObjects)
        {
            ProcessSerializedObject(serializedObject, groundManager);
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

    private static void ProcessSerializedObject(IDeserializable deserializable, GroundManager groundManager)
    {
        switch (deserializable)
        {
            case SerializedGround:
                var ground = groundManager.groundSpawner.AddGround();
                deserializable.Deserialize(ground.gameObject, groundManager.gameObject);
                groundManager.Grounds.Add(ground);
                break;
            case SerializedFinishLine:
                deserializable.Deserialize(groundManager.FinishLine.gameObject, groundManager.gameObject);
                break;
            case SerializedTutorialSign:
                var isSquare = ((SerializedTutorialSign)deserializable).IsSquare;
                var tutorialSign = groundManager.groundSpawner.AddTutorialSign(isSquare);
                deserializable.Deserialize(tutorialSign.gameObject, groundManager.gameObject);
                break;
            default:
                Debug.Log($"DeserializeLevel: Unhandled type {deserializable} during deserialization.");
                break;
        }
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

    public static List<CurvePoint> GenerateCurvePointListFromGround(SerializedGround serializedGround)
    {
        List<CurvePoint> curvePoints = new List<CurvePoint>();
        bool isFirstSegment = true;
        foreach (var serializedSegment in serializedGround.segmentList)
        {

            if (isFirstSegment)
            {
                CurvePoint curvePoint = LocalizedCurvePointFromSegment(serializedGround, serializedSegment, serializedSegment.curve.CurvePoints[0]);
                curvePoints.Add(curvePoint);
                isFirstSegment = false;
            }

            for (int i = 1; i < serializedSegment.curve.CurvePoints.Count; i++)
            {
                CurvePoint curvePoint = LocalizedCurvePointFromSegment(serializedGround, serializedSegment, serializedSegment.curve.CurvePoints[i]);
                curvePoints.Add(curvePoint);
            }

        }

        return curvePoints;
    }

    private static CurvePoint LocalizedCurvePointFromSegment(SerializedGround ground, SerializedGroundSegment segment, CurvePoint curvePoint)
    {
        var groundLocalzedPosition = (segment.position + curvePoint.Position) - (Vector3)ground.position;
        return new CurvePoint(groundLocalzedPosition, curvePoint.LeftTangent, curvePoint.RightTangent);
    }

    #endregion
}
