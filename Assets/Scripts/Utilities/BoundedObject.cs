using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundedObject<T>
{
    private T _object;
    private Vector3 _position;
    public BoundedObject(T obj, Vector3 position){
        _object = obj;
        _position = position;
    }

    public T Object { get => _object; }
    public Vector3 Position { get => _position; }
}
