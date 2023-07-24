using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class GradeData
{
    public float _minClimb, _maxClimb;

    public GradeData(float minClimb, float maxClimb)
    {
        _minClimb = minClimb;
        _maxClimb = maxClimb;
    }

    public GradeData()
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
}
