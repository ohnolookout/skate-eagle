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

    public float TimeFromMedal(Medal medal)
    {
        if(medal == Medal.Participant)
        {
            medal = Medal.Bronze;
        }
        return TimesArray[(int) medal];
        /*
        switch (medal)
        {
            case Medal.Participant:
                return _bronzeTime;
            case Medal.Bronze:
                return _bronzeTime;
            case Medal.Silver:
                return _silverTime;
            case Medal.Gold:
                return _goldTime;
            case Medal.Blue:
                return _blueTime;
            case Medal.Red:
                return _redTime;
            default:
                return _bronzeTime;
        }*/
    }

    public MedalTimes DeepCopy()
    {
        return new MedalTimes(_bronzeTime, _silverTime, _goldTime, _blueTime, _redTime);
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
