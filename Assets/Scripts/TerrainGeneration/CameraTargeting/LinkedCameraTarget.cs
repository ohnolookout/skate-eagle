using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class LinkedCameraTarget
{
    public CameraTarget LowTarget { get => _lowTarget; set => _lowTarget = value; }
    public CameraTarget HighTarget { get => _highTarget; set => _highTarget = value; }
    [SerializeReference] public List<LinkedCameraTarget> LeftTargets = new();
    [SerializeReference] public List<LinkedCameraTarget> RightTargets = new();
    [SerializeReference] public LinkedCameraTarget LeftKDNode;
    [SerializeReference] public LinkedCameraTarget RightKDNode;
    public CameraTargetType TargetType;
    public int[] SerializedLocation;
    [SerializeField] private CameraTarget _lowTarget;
    [SerializeField] private CameraTarget _highTarget;
    public LinkedCameraTarget()
    {
        LeftTargets = new();
        RightTargets = new();
        SerializedLocation = new int[0];
    }

    public void DrawTargets()
    {
        if (LowTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(LowTarget.TargetPosition, 0.5f);
        }
        if (HighTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(HighTarget.TargetPosition, 0.5f);
        }

        foreach (var target in LeftTargets)
        {
            if (target != null)
            {
                DrawToTarget(target, Color.deepPink);
            }
        }

        foreach (var target in RightTargets)
        {
            if (target != null)
            {
                DrawToTarget(target, Color.cyan);
            }
        }

    }
    private void DrawToTarget(LinkedCameraTarget target, Color color)
    {
        Gizmos.color = color;
        var pos = target.LowTarget.TargetPosition;
        Gizmos.DrawSphere(pos, 0.5f);
        Gizmos.DrawLine(pos, LowTarget.TargetPosition);
    }

}
