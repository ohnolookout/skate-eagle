using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;
using System.Linq;
using System;

public static class CameraTargetBuilder
{
    #region Build Targets
    public static void BuildLinkedCameraTarget(ICameraTargetable targetable)
    {
        if (targetable.LinkedCameraTarget == null)
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
            if (targetObjects[i] == null)
            {
                targetObjects.RemoveAt(i);
                i--;
                continue;
            }
            var linkedTarget = targetObjects[i].GetComponentInChildren<ICameraTargetable>();
            if (linkedTarget != null && linkedTarget.DoTargetLow)
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

    #region Deserialize Target GameObjects
    public static void DeserializeCameraTargets(GroundManager groundManager)
    {
        if (groundManager == null || groundManager.Grounds == null)
        {
            Debug.Log("GroundManager is null or has no grounds.");
            return;
        }
        var targetables = GetAllTargetables(groundManager);
        foreach (var targetable in targetables)
        {
            ReassociateGameObjects(targetable, groundManager);
        }
    }


    private static List<ICameraTargetable> GetAllTargetables(GroundManager groundManager)
    {
        var targetables = new List<ICameraTargetable>();

        foreach (var ground in groundManager.Grounds)
        {
            foreach (var curvePointObj in ground.CurvePointObjects)
            {
                targetables.Add(curvePointObj);
            }
        }

        foreach(var targetable in targetables)
        {
            targetable.PopulateDefaultTargets();
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

        if (!targetable.LinkedCameraTarget.doTargetLow)
        {
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

    #region KD Tree
    public static LinkedCameraTarget BuildKdTree(ICameraTargetable[] targetables)
    {
        if (targetables == null || targetables.Length == 0) 
        { 
            Debug.Log("CameraTargetKDTreeBuilder: No targetables found to build KD-Tree.");
            return null;
        }

        List<LinkedCameraTarget> targets = new();

        foreach (var targetable in targetables)
        {
            if (targetable.DoTargetLow)
            {
                BuildLinkedCameraTarget(targetable);
                targets.Add(targetable.LinkedCameraTarget);
            }
        }

        Debug.Log($"CameraTargetKDTreeBuilder: Found {targets.Count} targets to build KD-Tree.");

        return BuildKdTreeRecursive(targets);
    }

    private static LinkedCameraTarget BuildKdTreeRecursive(List<LinkedCameraTarget> targets, int depth = 0)
    {
        if (targets == null || targets.Count == 0)
            return null;

        var axis = depth % 2;

        targets = targets.OrderBy(t => axis == 0 
        ? t.Target.TargetPosition.x 
        : t.Target.TargetPosition.y).ToList();

        int medianIndex = targets.Count / 2;
        LinkedCameraTarget medianTarget = targets[medianIndex];
        medianTarget.LeftKDNode = BuildKdTreeRecursive(targets.GetRange(0, medianIndex), depth + 1);
        medianTarget.RightKDNode = BuildKdTreeRecursive(targets.GetRange(medianIndex + 1, targets.Count - medianIndex - 1), depth + 1);
        return medianTarget;
    }

    public static LinkedCameraTarget FindNearest(LinkedCameraTarget root, Vector2 point)
    {
        return FindNearestRecursive(root, point, 0, root);
    }
    private static LinkedCameraTarget FindNearestRecursive(LinkedCameraTarget node, Vector2 point, int depth, LinkedCameraTarget best)
    {
        if (node == null) { 
            return best;
        }

        float nodeDist = DistanceSquared(point, node.Target.TargetPosition);
        float bestDist = DistanceSquared(point, best.Target.TargetPosition);

        if (nodeDist < bestDist)
            best = node;

        int axis = depth % 2;

        float pointVal = axis == 0 ? point.x : point.y;
        float nodeVal = axis == 0 ? node.Target.TargetPosition.x : node.Target.TargetPosition.y;

        LinkedCameraTarget first = pointVal < nodeVal ? node.LeftKDNode : node.RightKDNode;
        LinkedCameraTarget second = pointVal < nodeVal ? node.RightKDNode : node.LeftKDNode;

        best = FindNearestRecursive(first, point, depth + 1, best);

        float planeDist = (pointVal - nodeVal) * (pointVal - nodeVal);
        bestDist = DistanceSquared(point, best.Target.TargetPosition);

        if (planeDist < bestDist)
        {
            best = FindNearestRecursive(second, point, depth + 1, best);
        }

        return best;
    }

    private static float DistanceSquared(Vector2 a, Vector3 b)
    {
        return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
    }
    #endregion
}
