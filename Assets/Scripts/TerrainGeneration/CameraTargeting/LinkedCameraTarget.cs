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
    public float zoomOrthoSize = 50f;
    public bool doUseManualZoomOrthoSize = false;
    public float manualZoomOrthoSize = 0f;
    public int[] SerializedObjectLocation;
    public Transform TargetTransform;
    public Vector3 TargetPosition
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

    public Vector3 CamBottomPosition => TargetPosition - new Vector3(0, YOffset * CameraTargetUtility.DefaultOrthoSize);

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
            zoomOrthoSize = zoomOrthoSize,
            manualZoomOrthoSize = manualZoomOrthoSize,
            doUseManualZoomOrthoSize = doUseManualZoomOrthoSize,
            forceZoomTargets = new List<CurvePoint>(forceZoomTargets),
            prevTarget = prevTarget,
            nextTarget = nextTarget
        };
        return copy;
    }

    public void DrawTargetInfo()
    {
        if (!doLowTarget)
        {
            return;
        }
        var camCenterX = TargetPosition.x - CameraTargetUtility.DefaultXBuffer;

        var camBottomY = CamBottomPosition.y;
        var camTopY = camBottomY + (2 * zoomOrthoSize);
        var camLeftX = camCenterX - zoomOrthoSize * CameraTargetUtility.DefaultAspectRatio;
        var camRightX = camCenterX + zoomOrthoSize * CameraTargetUtility.DefaultAspectRatio;

        var camTopLeft = new Vector3(camLeftX, camTopY);
        var camTopRight = new Vector3(camRightX, camTopY);
        var camBottomLeft = new Vector3(camLeftX, camBottomY);
        var camBottomRight = new Vector3(camRightX, camBottomY);

        //Draw camera box
        Handles.color = Color.white;
        Handles.DrawLine(camTopLeft, camTopRight);
        Handles.DrawLine(camTopRight, camBottomRight);
        Handles.DrawLine(camBottomRight, camBottomLeft);
        Handles.DrawLine(camBottomLeft, camTopLeft);

        //Draw to prev/next targets with offset size
        Handles.color = Color.magenta;
        if (prevTarget != null)
        {
            Handles.DrawLine(CamBottomPosition, prevTarget.CamBottomPosition);
        }

        Handles.color = Color.cyan;
        if (nextTarget != null)
        {
            Handles.DrawLine(CamBottomPosition, nextTarget.CamBottomPosition);
        }

        //Draw unoffset target positions

        Handles.color = Color.beige;

        if (nextTarget != null)
        {
            Handles.DrawLine(TargetPosition, nextTarget.TargetPosition);
        }


        //Draw offset target positions
        Handles.color = Color.yellow;
        Handles.SphereHandleCap(0, CamBottomPosition, Quaternion.identity, 1f, EventType.Repaint);
    }
}
