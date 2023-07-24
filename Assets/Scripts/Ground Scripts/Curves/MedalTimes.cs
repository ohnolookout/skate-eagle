using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public struct MedalTimes
{
    public float _bronzeTime, _silverTime, _goldTime, _blueTime, _redTime;
    // Start is called before the first frame update
    public MedalTimes(float bronzeTime, float silverTime, float goldTime, float blueTime, float redTime)
    {
        _bronzeTime = bronzeTime;
        _silverTime = silverTime;
        _goldTime = goldTime;
        _blueTime = blueTime;
        _redTime = redTime;
    }
}
