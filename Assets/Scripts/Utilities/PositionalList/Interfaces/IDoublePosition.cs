using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDoublePosition
{
    Vector3 Position { get; }
    Vector3 StartPosition { get; }
    Vector3 EndPosition { get; }
}