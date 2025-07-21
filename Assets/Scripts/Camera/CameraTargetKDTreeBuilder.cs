using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;
using System.Linq;
using System;

public static class CameraTargetKDTreeBuilder
{
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
                SerializeLevelUtility.BuildLinkedCameraTarget(targetable);
                targets.Add(targetable.LinkedCameraTarget.DeepCopy());
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
}
