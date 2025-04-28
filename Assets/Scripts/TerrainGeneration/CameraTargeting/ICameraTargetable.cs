using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;
using System;

public interface ICameraTargetable
{
    void PopulateDefaultTargets();
    //Use objects for assigning targets in the editor
    List<GameObject> LeftTargetObjects { get; set; }
    List<GameObject> RightTargetObjects { get; set; }
    
    //Use LinkedCameraTarget for actual targeting, but only build from GameObjects on level save
    LinkedCameraTarget LinkedCameraTarget { get; set; }
    bool DoTarget { get; set; }
}

[Serializable]
public class LinkedCameraTarget
{
    public CameraTarget LowTarget { get => _lowTarget; set 
        { 
            Debug.Log("Setting low target to: " + value);
            _lowTarget = value; 
        } 
    }
    public CameraTarget HighTarget { get; set; }
    public List<LinkedCameraTarget> LeftTargets { get; set; } = new();
    public List<LinkedCameraTarget> RightTargets { get; set; } = new();
    public CameraTargetType TargetType { get; set; }
    public int[] SerializedLocation { get; set; }
    [SerializeField] private CameraTarget _lowTarget;
    public LinkedCameraTarget()
    {
        LeftTargets = new();
        RightTargets = new();
        SerializedLocation = new int[0];
    }


}
