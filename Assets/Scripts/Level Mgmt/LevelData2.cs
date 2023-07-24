using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LevelData2
{
    private string _name;
    private float _length;
    private MedalTimes _medalTimes;
    private List<CombinedCurveDefinition> _weightedCurveList;
    private List<LevelSection> _levelSections;
    private int _currentSectionIndex = 0;

    public LevelData2()
    {
        _name = "Default Level";
        _length = 2000;
        _medalTimes = new();
        _levelSections = new();
        _levelSections.Add(new LevelSection());
        _weightedCurveList = WeightedCurveList(_levelSections[0]._curves);
        _currentSectionIndex = 0;

    }

    public LevelData2(string name, float length, MedalTimes medalTimes, List<LevelSection> levelSections)
    {
        _name = name;
        _length = length;
        _medalTimes = medalTimes;
        _levelSections = levelSections;
        _weightedCurveList = WeightedCurveList(_levelSections[0]._curves);
        _currentSectionIndex = 0;
    }

    public string Name
    {
        get
        {
            return _name;
        }
    }

    public float Length
    {
        get
        {
            return _length;
        }
    }

    public MedalTimes MedalTimes
    {
        get
        {
            return _medalTimes;
        }
    }

    public List<LevelSection> LevelSections
    {
        get
        {
            return _levelSections;
        }
    }

    public CombinedCurveDefinition NextCurve(float targetT, out GradeData grade)
    {
        targetT = Mathf.Clamp01(targetT);
        while(!CurrentSectionIsValid(targetT))
        {
            if (_currentSectionIndex > _levelSections.Count - 1)
            {
                _currentSectionIndex = _levelSections.Count - 1;
                break;
            }
            _currentSectionIndex++;
            _weightedCurveList = WeightedCurveList(_levelSections[_currentSectionIndex]._curves);
        }
        grade = _levelSections[_currentSectionIndex]._grade;
        int newCurveIndex = Random.Range(0, _weightedCurveList.Count);
        return _weightedCurveList[newCurveIndex];
    }

    private int SectionIndexByT(float targetT)
    {
        int sectionIndex = 0;
        while (targetT < _levelSections[sectionIndex]._startT)
        {
            sectionIndex++;
            if (sectionIndex >= _levelSections.Count - 1)
            {
                break;
            }
        }
        return sectionIndex;
    }

    private bool CurrentSectionIsValid(float targetT)
    {
        if(_currentSectionIndex >= _levelSections.Count - 1)
        {
            return true;
        }
        return _levelSections[_currentSectionIndex + 1]._startT >= targetT;
    }

    private static List<CombinedCurveDefinition> WeightedCurveList(List<CombinedCurveDefinition> unweightedCurveList) //Generates a list of curve definitions with multiple entries for curves with weight > 2
    {
        List<CombinedCurveDefinition> weightedList = new();
        foreach(CombinedCurveDefinition curveDefinition in unweightedCurveList)
        {
            for(int i = 0; i < curveDefinition.Weight; i++)
            {
                weightedList.Add(curveDefinition);
            }
        }
        return weightedList;
    }


}
