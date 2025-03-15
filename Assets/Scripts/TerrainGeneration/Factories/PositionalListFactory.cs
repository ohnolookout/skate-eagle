using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PositionalListFactory<T> where T : IPosition
{
    #region Generic Positional Lists
    public static SinglePositionalList<T> TransformTracker(List<T> allObjects, Transform transform, float trailingBuffer, float leadingBuffer)
    {
        Func<float> updateTrailing = () => transform.position.x - trailingBuffer;
        Func<float> updateLeading = () => transform.position.x + leadingBuffer;
        SinglePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading);
        return positionalList;
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
    /*
    public static SinglePositionalList<T> CameraOperatorTracker(List<T> allObjects, ICameraOperator cameraOperator, float trailingBuffer, float leadingBuffer, Action<T, ListSection> onObjectAdded = null, Action<T, ListSection> onObjectRemoved = null)
    {
        Func<float> updateTrailing = () => cameraOperator.TrailingCorner.x - trailingBuffer;
        Func<float> updateLeading = () => cameraOperator.LeadingCorner.x + leadingBuffer;
        SinglePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading, onObjectAdded, onObjectRemoved);
        return positionalList;
    }
    */
    #endregion
    /*
    #region HighLowManager Positional List
    public static void HighLowPositional(CameraHighLowManager highLowManager, ICameraOperator cameraOperator, Ground terrain,
        out PositionalMinMax<SortablePositionObject<Vector3>> lowPointCache, out PositionalMinMax<SortablePositionObject<HighPoint>> highPointCache)
    {
        BuildSortableLists(terrain, out var sortableLowPoints, out var sortableHighPoints);
        var lowPointList = PositionalListFactory<SortablePositionObject<Vector3>>.CameraTransformHighLowTracker(sortableLowPoints, highLowManager, cameraOperator, false);
        var highPointList = PositionalListFactory<SortablePositionObject<HighPoint>>.CameraTransformHighLowTracker(sortableHighPoints, highLowManager, cameraOperator, true);

        lowPointCache = new(lowPointList, ComparisonType.Least);
        highPointCache = new(highPointList, ComparisonType.Greatest);
    }

    private static void BuildSortableLists(Ground terrain, out List<SortablePositionObject<Vector3>> sortableLowPoints, out List<SortablePositionObject<HighPoint>> sortableHighPoints)
    {
        sortableLowPoints = new();
        sortableHighPoints = new();

        AddSegmentsToSortableLists(terrain.SegmentList, sortableLowPoints, sortableHighPoints);
    }

    private static void AddSegmentsToSortableLists(List<GroundSegment> segments,  List<SortablePositionObject<Vector3>> sortableLowPoints, List<SortablePositionObject<HighPoint>> sortableHighPoints)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            var currentCurve = segments[i].Curve;

            var currentLowpoint = segments[i].gameObject.transform.TransformPoint(currentCurve.LowPoint);

            sortableLowPoints.Add(new SortablePositionObject<Vector3>(currentLowpoint, currentLowpoint, currentLowpoint.y));

            Vector3 leadingLowPoint;
            if (i < segments.Count - 1)
            {
                leadingLowPoint = segments[i + 1].gameObject.transform.TransformPoint(segments[i + 1].Curve.LowPoint);
            }
            else
            {
                leadingLowPoint = currentLowpoint;
            }

            HighPoint newHighPoint = new(segments[i].gameObject.transform.TransformPoint(currentCurve.HighPoint), currentLowpoint, leadingLowPoint);
            sortableHighPoints.Add(new SortablePositionObject<HighPoint>(newHighPoint, newHighPoint.High, newHighPoint.Distance));
        }
    }

    public static SinglePositionalList<T> CameraTransformHighLowTracker(List<T> allObjects, CameraHighLowManager highLowManager, ICameraOperator cameraOperator, bool isHigh)
    {
        Func<float> updateTrailing, updateLeading;

        if (isHigh)
        {
            updateTrailing = highLowManager.TrailingCamHigh;
            updateLeading = highLowManager.LeadingCamHigh;
        }
        else
        {
            updateTrailing = highLowManager.TrailingCamLow;
            updateLeading = highLowManager.LeadingCamLow;
        }

        return new(allObjects, updateTrailing, updateLeading);
    }
    #endregion
    */
}