using UnityEngine;
using System;

public interface ICameraOperator
{
    Camera Camera { get; }
    Vector3 Center { get; }
    float DefaultSize { get; }
    Vector3 LeadingCorner { get; }
    Vector3 TrailingCorner { get; }
    float ZoomYDelta { get; }
    Action<ICameraOperator> OnZoomOut { get; set; }
    Action OnFinishZoomIn { get; set; }

    bool CameraZoomOut { get; }
}