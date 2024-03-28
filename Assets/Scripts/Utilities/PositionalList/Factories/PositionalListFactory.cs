using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PositionalListFactory<T> where T: IPosition
{
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
        Func<float> updateLeading = () => body.position.x - leadingBuffer;
        SinglePositionalList<T> positionalList = new(allObjects, updateTrailing, updateLeading);
        return positionalList;
    }
}
