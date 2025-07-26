using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class LinkedCameraTarget
{
    [SerializeReference] public List<LinkedCameraTarget> LeftTargets = new();
    [SerializeReference] public List<LinkedCameraTarget> RightTargets = new();
    [SerializeReference] public LinkedCameraTarget LeftKDNode;
    [SerializeReference] public LinkedCameraTarget RightKDNode;
    public bool doTargetHigh = false;
    public bool doTargetLow = false;
    public CameraTargetType TargetType;
    public int[] SerializedLocation;
    [SerializeField] private CameraTarget _target;
    public CameraTarget Target { get => _target; set => _target = value; }
    public LinkedCameraTarget()
    {
        LeftTargets = new();
        RightTargets = new();
        SerializedLocation = new int[0];
    }

    public void DrawTargets()
    {
        if (Target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Target.TargetPosition, 0.5f);
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
        var pos = target.Target.TargetPosition;
        Gizmos.DrawSphere(pos, 0.5f);
        Gizmos.DrawLine(pos, Target.TargetPosition);
    }

    public LinkedCameraTarget DeepCopy()
    {
        LinkedCameraTarget copy = new LinkedCameraTarget
        {
            Target = Target,
            LeftKDNode = LeftKDNode,
            RightKDNode = RightKDNode,
            TargetType = TargetType,
            SerializedLocation = (int[])SerializedLocation.Clone(),
            LeftTargets = new List<LinkedCameraTarget>(LeftTargets),
            RightTargets = new List<LinkedCameraTarget>(RightTargets)
        };
        return copy;
    }

}
