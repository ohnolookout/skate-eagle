using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LinkedCameraTarget
{
    [SerializeReference] public List<CurvePoint> forceZoomTargets = new();
    [SerializeReference] public LinkedCameraTarget prevTarget;
    [SerializeReference] public LinkedCameraTarget nextTarget;
    public bool doLowTarget = false;
    public bool doUseManualOffset = false;
    public float manualYOffset = 0f;
    public float yOffset = 0f;
    public float orthoSize = 50f;
    public bool doUseManualOrthoSize = false;
    public float manualOrthoSize = 0f;
    public int[] serializedObjectLocation;
    public Transform targetTransform;
    public ICameraTargetable parentObject;
    public Vector3 Position
    {
        get
        {
            if (targetTransform != null)
            {
                return SerializedPosition = targetTransform.position;
            }
            else
            {
                return SerializedPosition;
            }
        }
    }

    public Vector3 CamBottomPosition => Position - new Vector3(0, yOffset * CameraTargetUtility.DefaultOrthoSize);
    public Vector3 SerializedPosition;
    public LinkedCameraTarget()
    {
        serializedObjectLocation = new int[0];
    }

    public LinkedCameraTarget DeepCopy()
    {
        LinkedCameraTarget copy = new()
        {
            serializedObjectLocation = (int[])serializedObjectLocation.Clone(),
            SerializedPosition = SerializedPosition,
            targetTransform = targetTransform,
            doLowTarget = doLowTarget,
            doUseManualOffset = doUseManualOffset,
            manualYOffset = manualYOffset,
            yOffset = yOffset,
            orthoSize = orthoSize,
            manualOrthoSize = manualOrthoSize,
            doUseManualOrthoSize = doUseManualOrthoSize,
            forceZoomTargets = new List<CurvePoint>(forceZoomTargets),
            prevTarget = prevTarget,
            nextTarget = nextTarget
        };
        return copy;
    }

}
