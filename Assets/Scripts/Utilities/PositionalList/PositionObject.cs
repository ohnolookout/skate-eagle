using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionObject<T> : IPosition
{
    private Vector3 _position;
    private T _value;
    public PositionObject(T value, Vector3 position)
    {
        _value = value;
        _position = position;
    }
    public Vector3 Position { get => _position; set => _position = value; }
    public T Value => _value;
}
