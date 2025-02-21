using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public enum Medal { Red = 0, Blue = 1, Gold = 2, Silver = 3, Bronze = 4, Participant = 5 }
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

    public float Bronze => _bronzeTime;

    public float Silver => _silverTime;

    public float Gold => _goldTime;

    public float Blue => _blueTime;

    public float Red => _redTime;

    public float[] TimesArray => new float[5] { _redTime, _blueTime, _goldTime, _silverTime, _bronzeTime };

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

    public Medal MedalFromTime(float timeInSeconds)
    {
        float[] times = TimesArray;
        for (int i = 0; i < times.Length; i++)
        {
            if (timeInSeconds <= times[i])
            {
                return (Medal) i;
            }
        }
        return Medal.Participant;
    }

}
