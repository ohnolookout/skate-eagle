using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;

public interface IGroundSegment
{
    Curve Curve { get; }
    Vector2 EndPoint { get; }
    Spline Spline { get; }
    Vector2 StartPoint { get; }
    CurveType Type { get; }
    GameObject gameObject { get; }

    void ApplyCurve(Curve curve);
    bool ContainsX(float targetX);
    bool EndsBeforeX(float endX);
    bool StartsAfterX(float startX);
}