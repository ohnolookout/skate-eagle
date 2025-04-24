using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;
using System;

public interface ICameraTargetable
{
    void PopulateHighLowTargets();
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
    public CameraTarget LowTarget { get; set; }
    public CameraTarget HighTarget { get; set; }
    public List<LinkedCameraTarget> LeftTargets { get; set; } = new();
    public List<LinkedCameraTarget> RightTargets { get; set; } = new();
    public int[] SerializedLocation { get; set; }


}
