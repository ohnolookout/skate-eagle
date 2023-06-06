using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelData
{
    private static float[] levelTimes = new float[3];
    private static List<CurveType> curveTypes;
    private static float length;

    public LevelData()
    {

    }

    public LevelData(float[] times, List<CurveType> curves, float targetLength) 
    {
        levelTimes = times;
        curveTypes = curves;
        length = targetLength;
    }

    public float[] LevelTimes
    {
        get
        {
            return levelTimes;
        }
        set
        {
            if(value.Length != levelTimes.Length)
            {
                throw new Exception("Incorrect number of times passed into LevelData argument.") ;
            }
            for(int i = 0; i < levelTimes.Length; i++)
            {
                levelTimes[i] = value[i];
            }
        }
    }

    public float Length
    {
        get
        {
            return length;
        }
        set
        {
            length = value;
        }
    }

    public List<CurveType> CurveTypes
    {
        get
        {
            return curveTypes;
        }
        set
        {
            curveTypes = value;
        }
    }
}
