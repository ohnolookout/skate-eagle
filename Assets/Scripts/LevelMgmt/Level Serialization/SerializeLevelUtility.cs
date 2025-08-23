using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.U2D;

public static class SerializeLevelUtility
{
    #region Serialization
    public static List<IDeserializable> SerializeGroundManager(GroundManager groundManager, out SerializedStartLine startLine)
    {
        var grounds = groundManager.GetGrounds();
        GenerateGroundIndices(grounds);

        if (groundManager.StartLine.CurvePoint == null)
        {
            Debug.Log("StartPoint is null, setting to default.");
            if (grounds == null || grounds.Length == 0 || grounds[0].CurvePoints.Count == 0)
            {
                groundManager.StartLine.SetStartLine(new CurvePoint());
            } else if (grounds[0].CurvePoints.Count > 1)
            {
                groundManager.StartLine.SetStartLine(grounds[0].CurvePoints[1]);
            }
            else
            {
                groundManager.StartLine.SetStartLine(grounds[0].CurvePoints[0]);
            }
        }

        startLine = (SerializedStartLine) groundManager.StartLine.Serialize();

        var serializables = groundManager.GetComponentsInChildren<ISerializable>();

        List<IDeserializable> serializedObjects = new();

        foreach(var serializable in serializables)
        {
            serializedObjects.Add(serializable.Serialize());
        }

        return serializedObjects;
    }
    private static void GenerateGroundIndices(Ground[] grounds)
    {
        for (int i = 0; i < grounds.Length; i++)
        {
            var ground = grounds[i];
            ground.gameObject.name = "Ground " + i;
            for (int j = 0; j < ground.CurvePointObjects.Length; j++)
            {
                var cpObj = ground.CurvePointObjects[j];
                cpObj.name = "CP " + i + "_" + j;

                if (cpObj.LinkedCameraTarget.doTargetLow)
                {
                    cpObj.name += "_LT";
                }

                cpObj.LinkedCameraTarget.SerializedLocation = new int[2] { i, j };
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

        serializedGround.segmentList = new();
        var groundNamePrefix = serializedGround.name.Remove(1, serializedGround.name.Length - 2);

        serializedGround.editorSegment = new(groundNamePrefix + " Editor Segment", serializedGround, serializedGround.curvePoints, null, true, true);

        //Create serialized runtime segments
        var runtimeSegmentsCurvePoints = BreakDownSegments(serializedGround);

        Debug.Log("SerializeGroundSegments: Broken down into " + runtimeSegmentsCurvePoints.Count + " segments.");

        for (int i = 0; i < runtimeSegmentsCurvePoints.Count; i++)
        {
            var name = groundNamePrefix + " Segment " + i;
            Vector3? lastColliderPoint = serializedGround.segmentList.Count > 0 ? serializedGround.segmentList[^1].colliderPoints[^1] : null;

            var isFirst = i == 0;
            var isLast = i == runtimeSegmentsCurvePoints.Count - 1;

            var serializedSegment = new SerializedGroundSegment(name, serializedGround, runtimeSegmentsCurvePoints[i], lastColliderPoint,
                isFirst, isLast);

            serializedGround.segmentList.Add(serializedSegment);
        }

    }

    private const float MaxSectionDistance = 80; // Maximum distance between points in a section
    private static List<List<CurvePoint>> BreakDownSegments(SerializedGround serializedGround)
    {
        var allCurvePoints = DeepCopyCurvePoints(serializedGround.curvePoints);
        List<List<CurvePoint>> sections = new();
        if (allCurvePoints.Count < 3)
        {
            sections.Add(allCurvePoints);

            return sections;
        }

        int prevFloorPositionIndex = 0;
        int nextFloorPositionIndex = allCurvePoints.Count - 1;


        if (serializedGround.floorType == FloorType.Segmented)
        {
            nextFloorPositionIndex = FindNextFloorPointIndex(allCurvePoints, 0);
        }

        List<CurvePoint> currentSection = new();
        List<Vector3> currentFloorSection = new();

        for (int i = 0; i < allCurvePoints.Count; i++)
        {

            var curvePoint = allCurvePoints[i];
            //Check if we need to update the last and next floor position indices
            if(i == nextFloorPositionIndex)
            {
                prevFloorPositionIndex = nextFloorPositionIndex;
                nextFloorPositionIndex = FindNextFloorPointIndex(allCurvePoints, prevFloorPositionIndex);
            }

            // Check if the distance from the start point exceeds the maximum section distance
            // Also, check to make sure we are not at the last point
            if (i < allCurvePoints.Count - 1 &&
                (currentSection.Count > 4 ||
                (currentSection.Count > 0 && Vector2.Distance(currentSection[0].Position, curvePoint.Position) > MaxSectionDistance)))
            {
                //End of section/start of next section identified
                //Calculate floor position and set to has floor position if cp doesn't currently have one.

                if (!curvePoint.HasFloorPosition)
                {
                    curvePoint.HasFloorPosition = true;
                    curvePoint.FloorPosition = LerpFloorPoint(curvePoint, allCurvePoints[prevFloorPositionIndex], allCurvePoints[nextFloorPositionIndex]);
                }

                currentSection.Add(curvePoint);
                sections.Add(currentSection);
                currentSection = new List<CurvePoint>();
            }
            currentSection.Add(curvePoint);
        }
        sections.Add(currentSection);

        return sections;
    }

    private static int FindNextFloorPointIndex(List<CurvePoint> curvePoints, int startIndex)
    {
        for (int i = startIndex + 1; i < curvePoints.Count; i++)
        {
            if (curvePoints[i].HasFloorPosition)
            {
                return i;
            }
        }
        return curvePoints.Count - 1;
    }

    private static Vector3 LerpFloorPoint(CurvePoint currentCP, CurvePoint prevFloorCP, CurvePoint nextFloorCP)
    {
        var point = BezierMath.GetPerpendicularIntersection(prevFloorCP.FloorPosition, nextFloorCP.FloorPosition, currentCP.Position);
        return point;
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

#if UNITY_EDITOR

        if (Application.isPlaying)
        {
            return;
        }

        //CameraTargetBuilder.DeserializeCameraTargets(groundManager);
        ResyncObjects(groundManager);

#endif
    }

    private static void ProcessSerializedObject(IDeserializable deserializable, GroundManager groundManager)
    {
        switch (deserializable)
        {
            case SerializedGround:
                var ground = groundManager.groundSpawner.AddGround();
                deserializable.Deserialize(ground.gameObject, groundManager.gameObject);
                break;
            case SerializedFinishLine:
                deserializable.Deserialize(groundManager.FinishLine.gameObject, groundManager.gameObject);
                break;
            case SerializedStartLine:
                deserializable.Deserialize(groundManager.StartLine.gameObject, groundManager.gameObject);
                break;
            case SerializedTutorialSign:
                var isSquare = ((SerializedTutorialSign)deserializable).Type;
                var tutorialSign = groundManager.groundSpawner.AddTutorialSign(isSquare);
                deserializable.Deserialize(tutorialSign.gameObject, groundManager.gameObject);
                break;
            default:
                Debug.Log($"DeserializeLevel: Unhandled type {deserializable} during deserialization.");
                break;
        }
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

    private static void ResyncObjects(GroundManager groundManager)
    {
        var iResyncObjects = groundManager.GetComponentsInChildren<IObjectResync>();

        List<ObjectResync> objectResyncs = new();
        foreach(var resyncObject in iResyncObjects) { 

            var resyncs = resyncObject.GetObjectResyncs();
            if (resyncs != null && resyncs.Count > 0)
            {
                objectResyncs.AddRange(resyncs);
            }
        }

        foreach(var resync in objectResyncs)
        {
            var gameObject = groundManager.GetGameObjectByIndices(resync.serializedLocation);

            if (gameObject != null)
            {
                resync.resyncFunc(gameObject);
            }
            else
            {
                Debug.Log("Gameobject is null. Skipping resync...");
            }
        }
    }

    #endregion
}

public class ObjectResync
{
    public int[] serializedLocation;
    public Action<GameObject> resyncFunc;

    public ObjectResync(int[] serializedLocation)
    {
        this.serializedLocation = serializedLocation;
    }
}
