using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PositionalListFactory<T> where T : IPosition
{
    public static SinglePositionalList<T> TransformTracker(List<T> allObjects, Transform transform, float trailingBuffer, float leadingBuffer)
    {
        Func<float> updateTrailing = () => transform.position.x - trailingBuffer;
        Func<float> updateLeading = () => transform.position.x + leadingBuffer;
        SinglePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading);
        return positionalList;
    }

    public static void CameraHighLowTrackers(ICameraOperator cameraOperator, LevelTerrain terrain, out PositionalMinMax<PositionObject<Vector3>> lowPointCache, out PositionalMinMax<PositionObject<Vector3>> highPointCache)
    {
        List<PositionObject<Vector3>> lowPoints = new(), highPoints = new();
        Debug.Log("Last lowPoint in segmentList: " + terrain.SegmentList[^1].Curve.Lowpoint);
        foreach (var segment in terrain.SegmentList)
        {
            lowPoints.Add(new PositionObject<Vector3>(segment.Curve.Lowpoint, segment.Curve.Lowpoint));
            highPoints.Add(new PositionObject<Vector3>(segment.Curve.Highpoint, segment.Curve.Highpoint));
        }
        var lowPointList = PositionalListFactory<PositionObject<Vector3>>.CameraTransformHighLowTracker(lowPoints, cameraOperator, false);
        var highPointList = PositionalListFactory<PositionObject<Vector3>>.CameraTransformHighLowTracker(highPoints, cameraOperator, true);

        lowPointCache = new(lowPointList, ComparisonType.Least);
        highPointCache = new(highPointList, ComparisonType.Greatest);
    }

    public static SinglePositionalList<T> CameraTransformHighLowTracker(List<T> allObjects, ICameraOperator cameraOperator, bool isHigh)
    {
        Func<float> updateTrailing, updateLeading;
        if (isHigh)
        {
            updateTrailing = () => cameraOperator.gameObject.transform.position.x - cameraOperator.HighPointBuffer;
            updateLeading = () => cameraOperator.gameObject.transform.position.x + cameraOperator.HighPointBuffer;
        }
        else
        {
            updateTrailing = () => cameraOperator.gameObject.transform.position.x - cameraOperator.LowPointBuffer;
            updateLeading = () => cameraOperator.gameObject.transform.position.x + cameraOperator.LowPointBuffer;
        }
        return new(allObjects, updateTrailing, updateLeading);

    }

    public static SinglePositionalList<T> CameraTracker(List<T> allObjects, Camera camera, float trailingBuffer, float leadingBuffer)
    {
        Func<float> updateTrailing = () => camera.ViewportToWorldPoint(new Vector3(0, 1, 0)).x - trailingBuffer;
        Func<float> updateLeading = () => camera.ViewportToWorldPoint(new Vector3(1, 1, 0)).x + leadingBuffer;
        SinglePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading);
        return positionalList;
    }

    public static SinglePositionalList<T> BodyTracker(List<T> allObjects, Rigidbody2D body, float trailingBuffer, float leadingBuffer)
    {
        Func<float> updateTrailing = () => body.position.x - trailingBuffer;
        Func<float> updateLeading = () => body.position.x + leadingBuffer;
        SinglePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading);
        return positionalList;
    }

    public static SinglePositionalList<T> CameraOperatorTracker(List<T> allObjects, ICameraOperator cameraOperator, float trailingBuffer, float leadingBuffer)
    {
        Func<float> updateTrailing = () => cameraOperator.TrailingCorner.x - trailingBuffer;
        Func<float> updateLeading = () => cameraOperator.LeadingCorner.x + leadingBuffer;
        SinglePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading);
        return positionalList;
    }
}
