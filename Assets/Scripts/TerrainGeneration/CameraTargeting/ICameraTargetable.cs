using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;

public interface ICameraTargetable
{
    List<CameraTarget> CameraTargets { get; }
    bool DoTarget { get; }
}
