using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

[CreateAssetMenu(fileName = "New Level", menuName = "ScriptableObjects/LevelData2")]
[Serializable]
public class Level : ScriptableObject
{
    public string _name;
    public float _length;
    public MedalTimes _medalTimes;
    public List<LevelSection> _levelSections;
    private List<CombinedCurveDefinition> _weightedCurveList;
    private int _currentSectionIndex = 0;


    public Level()
    {
        _name = "Default Level";
        _length = 2000;
        _medalTimes = new(60, 45, 30, 20, 10);
        _levelSections = new();
        _levelSections.Add(new LevelSection());
        _weightedCurveList = WeightedCurveList(_levelSections[0]._curves);
        _currentSectionIndex = 0;
    }

    public void ManualReset()
    {
        List<LevelSection> defaultSections = new();
        defaultSections.Add(new LevelSection());
        ReassignValues("Default Level", 2000, new MedalTimes(), defaultSections);
        _currentSectionIndex = 0;
    }

    public void ReassignValues(string name, float length, MedalTimes medalTimes, List<LevelSection> levelSections)
    {
        _name = name;
        _length = length;
        _medalTimes = medalTimes;
        _levelSections = levelSections;
    }

    public void ReassignValues(Level level)
    {
        Level reassignLevel = CreateInstance<Level>();
        reassignLevel = level.DeepCopy();
        _name = reassignLevel.Name;
        _length = reassignLevel.Length;
        _medalTimes = reassignLevel.MedalTimes;
        _levelSections = reassignLevel.LevelSections;
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

    private static List<CombinedCurveDefinition> WeightedCurveList(List<CombinedCurveDefinition> unweightedCurveList) //Generates a list of curve definitions with multiple entries for curves with weight > 2
    {
        List<CombinedCurveDefinition> weightedList = new();
        foreach (CombinedCurveDefinition curveDefinition in unweightedCurveList)
        {
            for (int i = 0; i < curveDefinition.Weight; i++)
            {
                weightedList.Add(curveDefinition);
            }
        }
        return weightedList;
    }
    public CombinedCurveDefinition NextCurve(float targetT, out GradeData grade)
    {
        targetT = Mathf.Clamp01(targetT);
        while (!CurrentSectionIsValid(targetT))
        {
            if (_currentSectionIndex > _levelSections.Count - 1)
            {
                _currentSectionIndex = _levelSections.Count - 1;
                break;
            }
            _currentSectionIndex++;
        }
        _weightedCurveList = WeightedCurveList(_levelSections[_currentSectionIndex]._curves);
        grade = _levelSections[_currentSectionIndex]._grade;
        int newCurveIndex = UnityEngine.Random.Range(0, _weightedCurveList.Count);
        return _weightedCurveList[newCurveIndex];
    }

    private bool CurrentSectionIsValid(float targetT)
    {
        if (_currentSectionIndex >= _levelSections.Count - 1)
        {
            return true;
        }
        return _levelSections[_currentSectionIndex + 1]._startT >= targetT;
    }

    public Level DeepCopy()
    {
        string name = _name;
        float length = _length;
        MedalTimes medalTimes = _medalTimes.DeepCopy();
        List<LevelSection> levelSections = new();
        foreach (LevelSection section in _levelSections)
        {
            levelSections.Add(section.DeepCopy());
        }
        Level newLevel = ScriptableObject.CreateInstance<Level>();
        newLevel.ReassignValues(name, length, medalTimes, levelSections);
        return newLevel;
    }

    public bool Validate()
    {
        if (_name == "" || _name is null)
        {
            EditorUtility.DisplayDialog("Unnamed Level", "You can't save a level without a name!", "OK", "OK");
            return false;
        }
        if (!ValidateSections())
        {
            EditorUtility.DisplayDialog("Invalid Level Sections", "Levels must have at least one section, the first section must begin at 0, all sections must have at least one curve, and StartT values must be between 0 and 1", "OK", "I'm sorry");
            return false;
        }
        if (!_medalTimes.Validate())
        {
            EditorUtility.DisplayDialog("Invalid Medal Times", "Times must be in descending order and greater than zero.", "OK", "Oops");
            return false;
        }
        return true;
    }

    public bool ValidateSections()
    {
        bool isValid = true;
        if (_levelSections.Count < 1)
        {
            isValid = false;
        }
        _levelSections = _levelSections.OrderBy(section => section._startT).ToList();
        if (_levelSections[0].StartT != 0)
        {
            isValid = false;
        }
        for (int i = 0; i < _levelSections.Count; i++)
        {
            if (!_levelSections[i].Validate())
            {
                isValid = false;
            }
        }
        return isValid;
    }
}
