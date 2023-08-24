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

    public float Bronze
    {
        get
        {
            return _bronzeTime;
        }
    }

    public float Silver
    {
        get
        {
            return _silverTime;
        }
    }

    public float Gold
    {
        get
        {
            return _goldTime;
        }
    }

    public float Blue
    {
        get
        {
            return _blueTime;
        }
    }

    public float Red
    {
        get
        {
            return _redTime;
        }
    }

    public float[] TimesArray
    {
        get
        {
            return new float[5] { _redTime, _blueTime, _goldTime, _silverTime, _bronzeTime}; 
        }
    }

    public bool Validate()
    {
        float[] times = TimesArray;
        for (int i = 1; i < times.Length; i++)
        {
            if (times[i] <= times[i - 1])
            {
                return false;
            }
        }
        return true;
    }

    public MedalTimes DeepCopy()
    {
        return new MedalTimes(_bronzeTime, _silverTime, _goldTime, _blueTime, _redTime);
    }

}
