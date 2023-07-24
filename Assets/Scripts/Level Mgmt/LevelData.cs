using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelData
{
    private static float[] levelTimes = new float[3];
    private static List<CurveType> curveTypes;
    private static float length;
    private static Dictionary<int, Vector2> grade; //Use Vector2 to set min and max value. Int indicates distance when grade changes.
    private static List<CurveDefinition[]> curveDefinitions; //Each array of curveDefinitions has at least two elements. The first defines the valley, the second defines the peak.

    public LevelData()
    {

    }

    public LevelData(float[] times, List<CurveType> curves, float targetLength)
    {
        levelTimes = times;
        curveTypes = curves;
        length = targetLength;
        grade = new() { { 0, new Vector2(2, -8)} };
        curveDefinitions = new();
        AddCurveDefinitions(DefaultCurveDefinitions());
    }

    public LevelData(float[] levelTimes, List<CurveType> curves, float length, Dictionary<int, Vector2> grade)
    {
        LevelTimes = levelTimes;
        CurveTypes = curves;
        Length = length;
        Grade = grade;

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

    public void AddCurveDefinitions(CurveDefinition[] definitions)
    {
        curveDefinitions.Add(definitions);
    }

    public void ResetCurveDefinitions()
    {
        curveDefinitions = new();
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

    public Dictionary<int, Vector2> Grade
    {
        get
        {
            return grade;
        }
        set
        {
            grade = value;
        }
    }

    public List<CurveDefinition[]> CurveDefinitions
    {
        get
        {
            return curveDefinitions;
        }
    }

    private CurveDefinition[] DefaultCurveDefinitions()
    {
        CurveDefinition peakDefinition = new(LengthType.Short, ShapeType.HardPeak, SlopeType.Steep, SkewType.Center);
        CurveDefinition valleyDefinition = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal, SkewType.Center);
        return new CurveDefinition[] { valleyDefinition, peakDefinition };
    }
}
