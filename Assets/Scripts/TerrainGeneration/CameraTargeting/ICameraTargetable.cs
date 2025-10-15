using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;
using System;

public interface ICameraTargetable
{
    void AddObjectToTarget();
    
    //Use LinkedCameraTarget for actual targeting, but only build from GameObjects on level save
    LinkedCameraTarget LinkedCameraTarget { get; set; }
    GameObject Object { get; }
    bool DoTargetLow { get; set; }
}


