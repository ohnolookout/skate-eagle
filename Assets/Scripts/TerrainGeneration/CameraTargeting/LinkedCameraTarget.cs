using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[Serializable]
public class LinkedCameraTarget
{
    [SerializeReference] public List<CurvePoint> forceZoomTargets = new();
    [SerializeReference] public LinkedCameraTarget prevTarget;
    [SerializeReference] public LinkedCameraTarget nextTarget;
    public bool doZoomTarget = false;
    public bool doLowTarget = false;
    public bool doUseManualOffsets = false;
    public float manualYOffset = 0f;
    public float YOffset = 0f;
    public float OrthoSize = 50f;
    public bool doUseManualZoomOrthoSize = false;
    public float manualZoomOrthoSize = 0f;
    public int[] SerializedObjectLocation;
    public Transform TargetTransform;
    public Vector3 Position
    {
        get
        {
            if (TargetTransform != null)
            {
                return SerializedPosition = TargetTransform.position;
            }
            else
            {
                return SerializedPosition;
            }
        }
    }

    public Vector3 CamBottomPosition => Position - new Vector3(0, YOffset * CameraTargetUtility.DefaultOrthoSize);

    public bool IsAdjusting = false;
    public Vector3 SerializedPosition;
    public LinkedCameraTarget()
    {
        SerializedObjectLocation = new int[0];
    }

    public LinkedCameraTarget DeepCopy()
    {
        LinkedCameraTarget copy = new LinkedCameraTarget
        {
            SerializedObjectLocation = (int[])SerializedObjectLocation.Clone(),
            SerializedPosition = SerializedPosition,
            TargetTransform = TargetTransform,
            doZoomTarget = doZoomTarget,
            doLowTarget = doLowTarget,
            doUseManualOffsets = doUseManualOffsets,
            manualYOffset = manualYOffset,
            YOffset = YOffset,
            OrthoSize = OrthoSize,
            manualZoomOrthoSize = manualZoomOrthoSize,
            doUseManualZoomOrthoSize = doUseManualZoomOrthoSize,
            forceZoomTargets = new List<CurvePoint>(forceZoomTargets),
            prevTarget = prevTarget,
            nextTarget = nextTarget
        };
        return copy;
    }

}
