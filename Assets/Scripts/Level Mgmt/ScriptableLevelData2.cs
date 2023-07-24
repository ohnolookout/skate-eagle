using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Level", menuName = "ScriptableObjects/LevelData2")]
public class ScriptableLevelData2 : ScriptableObject
{
    public string _name;
    public float _length;
    public MedalTimes _medalTimes;
    public List<LevelSection> _levelSections;
    public LevelData2 _levelData;

    public ScriptableLevelData2(LevelData2 levelData)
    {
        ReassignValues(levelData);
    }

    public ScriptableLevelData2()
    {
        _levelData = new();
        ReassignValues(_levelData);
    }

    public void ReassignValues(LevelData2 levelData)
    {
        _name = levelData.Name;
        _length = levelData.Length;
        _medalTimes = levelData.MedalTimes;
        _levelSections = levelData.LevelSections;
        _levelData = levelData;

    }

    public LevelData2 LevelData
    {
        get
        {
            return _levelData;
        }
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
}
