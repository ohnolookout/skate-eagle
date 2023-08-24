using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class Grade
{
    public float _minClimb, _maxClimb;

    public Grade(float minClimb, float maxClimb)
    {
        _minClimb = minClimb;
        _maxClimb = maxClimb;
    }

    public Grade()
    {
        _minClimb = 0;
        _maxClimb = 0;
    }


    public float MinClimb
    {
        get
        {
            return _minClimb;
        }
    }
    public float MaxClimb
    {
        get
        {
            return _maxClimb;
        }
    }

    public Grade DeepCopy()
    {
        return new Grade(_minClimb, _maxClimb);
    }
}
