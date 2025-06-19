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

    public float Bronze { get => _bronzeTime; set => _bronzeTime = value; }

    public float Silver { get => _silverTime; set => _silverTime = value; }

    public float Gold { get => _goldTime; set => _goldTime = value; }

    public float Blue { get => _blueTime; set => _blueTime = value; }

    public float Red { get => _redTime; set => _redTime = value; }

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

    public void SetTime(Medal medal, float time)
    {
        switch (medal)
        {
            case Medal.Red:
                _redTime = time;
                break;
            case Medal.Blue:
                _blueTime = time;
                break;
            case Medal.Gold:
                _goldTime = time;
                break;
            case Medal.Silver:
                _silverTime = time;
                break;
            case Medal.Bronze:
                _bronzeTime = time;
                Debug.Log("Medaltimes updated bronze to " + time);
                Debug.Log("Saved bronze value: " + _bronzeTime);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(medal), medal, null);
        }
    }

}
