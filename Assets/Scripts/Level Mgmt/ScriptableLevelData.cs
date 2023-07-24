using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Level", menuName = "ScriptableObjects/LevelData")]
public class ScriptableLevelData : ScriptableObject
{
    public float[] levelTimes = new float[3];
    public float length;
    public Dictionary<int, Vector2> grade; //Use Vector2 to set min and max value. Int indicates distance when grade changes.
    public List<CurveDefinition[]> curveDefinitions; //Each array of curveDefinitions has at least two elements. The first defines the valley, the second defines the peak.
    public List<CombinedCurveDefinition> combinedCurveDefinitions;
    public List<GradeData> gradeDataList;

    public float[] LevelTimes
    {
        get
        {
            return levelTimes;
        }
        set
        {
            if (value.Length != levelTimes.Length)
            {
                throw new Exception("Incorrect number of times passed into LevelData argument.");
            }
            for (int i = 0; i < levelTimes.Length; i++)
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
