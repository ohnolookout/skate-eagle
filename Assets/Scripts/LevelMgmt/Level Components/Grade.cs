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


    public float MinClimb => _minClimb;
    public float MaxClimb => _maxClimb;

}
