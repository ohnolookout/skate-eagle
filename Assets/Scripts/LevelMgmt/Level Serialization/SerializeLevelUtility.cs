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

    #endregion
}
