using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections;
using System.Collections.Generic;
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


}
