using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class DoublePositionalListFactory<T> where T : IDoublePosition, IPosition
{
    public static DoublePositionalList<T> TransformTracker(List<T> allObjects, Transform transform, float trailingBuffer, float leadingBuffer)
    {
        Func<float> updateTrailing = () => transform.position.x - trailingBuffer;
        Func<float> updateLeading = () => transform.position.x + leadingBuffer;
        DoublePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading);
        return positionalList;
    }

    public static DoublePositionalList<T> CameraTracker(List<T> allObjects, Camera camera, float trailingBuffer, float leadingBuffer, Action<T, ListSection> onObjectAdded = null, Action<T, ListSection> onObjectRemoved = null)
    {
        Func<float> updateTrailing = () => camera.ViewportToWorldPoint(new Vector3(0, 1, 0)).x - trailingBuffer;
        Func<float> updateLeading = () => camera.ViewportToWorldPoint(new Vector3(1, 1, 0)).x + leadingBuffer;
        DoublePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading, onObjectAdded, onObjectRemoved);
        return positionalList;
    }
    public static DoublePositionalList<T> BodyTracker(List<T> allObjects, Rigidbody2D body, float trailingBuffer, float leadingBuffer)
    {
        Func<float> updateTrailing = () => body.position.x - trailingBuffer;
        Func<float> updateLeading = () => body.position.x + leadingBuffer;
        DoublePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading);
        return positionalList;
    }
    /*
    public static DoublePositionalList<T> CameraOperatorTracker(List<T> allObjects, ICameraOperator cameraOperator, float trailingBuffer, float leadingBuffer, Action<T, ListSection> onObjectAdded = null, Action<T, ListSection> onObjectRemoved = null)
    {
        Func<float> updateTrailing = () => cameraOperator.TrailingCorner.x - trailingBuffer;
        Func<float> updateLeading = () => cameraOperator.LeadingCorner.x + leadingBuffer;
        DoublePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading, onObjectAdded, onObjectRemoved);
        return positionalList;
    }
    */
}
