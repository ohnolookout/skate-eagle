using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using System;

#nullable enable
public interface IGroundSegment : IDoublePosition, IPosition, ISerializable
{
    Curve Curve { get; }
    Spline Spline { get; }
    GameObject gameObject { get; }
    bool IsFinish { get; set; }
    EdgeCollider2D Collider { get; set; }
    
    bool ContainsX(float targetX);
    bool EndsBeforeX(float endX);
    bool StartsAfterX(float startX);
}