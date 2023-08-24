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
    public MedalTimes _medalTimes;
    public List<LevelSection> _levelSections;
    public int cachedSequencesCount;


    public Level()
    {
        _name = "Default Level";
        _medalTimes = new(60, 45, 30, 20, 10);
        _levelSections = new();
        _levelSections.Add(new LevelSection());
    }

    public void ManualReset()
    {
        List<LevelSection> defaultSections = new();
        defaultSections.Add(new LevelSection());
        ReassignValues("Default Level", new MedalTimes(60, 45, 30, 20, 10), defaultSections);
    }

    public void ReassignValues(string name, MedalTimes medalTimes, List<LevelSection> levelSections)
    {
        _name = name;
        _medalTimes = medalTimes;
        _levelSections = DeepCopySections(levelSections);
    }

    public void ReassignValues(Level level)
    {
        _name = level.Name;
        _medalTimes = level.MedalTimes.DeepCopy();
        _levelSections = level.DeepCopySections();
        cachedSequencesCount = (int)CachedSequencesCount().y;
    }

    public void CacheSections()
    {
        foreach(LevelSection section in _levelSections)
        {
            section.CacheValidSections();
        }
        cachedSequencesCount = (int)CachedSequencesCount().y;
    }

    public Vector2 CachedSequencesCount() //X value is num of sections, y is total possible sequences
    {
        Vector2 sequencesCount = new(0, 0);
        foreach(LevelSection section in _levelSections)
        {
            sequencesCount = new(sequencesCount.x + 1, sequencesCount.y);
            foreach(Sequence sequence in section._cachedSequences)
            {
                sequencesCount = new(sequencesCount.x, sequencesCount.y + 1);
            }
        }
        return sequencesCount;
    }

    public string Name
    {
        get
        {
            return _name;
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

    public Dictionary<Grade, Sequence> GenerateSequence()
    {
        cachedSequencesCount = (int)CachedSequencesCount().y;
        Dictionary<Grade, Sequence> sequences = new();
        foreach (LevelSection section in _levelSections)
        {
            sequences[section.Grade] = section.RandomCurveSequence;
        }
        return sequences;
    }

    public Level DeepCopy()
    {
        string name = _name;
        MedalTimes medalTimes = _medalTimes.DeepCopy();
        List<LevelSection> levelSections = DeepCopySections();
        Level newLevel = ScriptableObject.CreateInstance<Level>();
        newLevel.ReassignValues(name, medalTimes, levelSections);
        return newLevel;
    }

    public List<LevelSection> DeepCopySections()
    {
        List<LevelSection> levelSections = new();
        foreach (LevelSection section in _levelSections)
        {
            levelSections.Add(section.DeepCopy());
        }
        return levelSections;
    }

    public List<LevelSection> DeepCopySections(List<LevelSection> sectionsToCopy)
    {
        List<LevelSection> levelSections = new();
        foreach (LevelSection section in sectionsToCopy)
        {
            levelSections.Add(section.DeepCopy());
        }
        return levelSections;
    }

    public bool Validate()
    {
        if (_name == "" || _name is null)
        {
            //EditorUtility.DisplayDialog("Unnamed Level", "You can't save a level without a name!", "OK", "OK");
            return false;
        }
        if (!ValidateSections())
        {
            //EditorUtility.DisplayDialog("Invalid Level Sections", "Levels must have at least one section, the first section must begin at 0, all sections must have at least one curve, and StartT values must be between 0 and 1", "OK", "I'm sorry");
            return false;
        }
        if (!_medalTimes.Validate())
        {
            //EditorUtility.DisplayDialog("Invalid Medal Times", "Times must be in descending order and greater than zero.", "OK", "Oops");
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
        for (int i = 0; i < _levelSections.Count; i++)
        {
            if (!_levelSections[i].Validate())
            {
                isValid = false;
            }
        }
        return isValid;
    }

    public void LogSections()
    {
        foreach(LevelSection section in _levelSections)
        {
            section.Log();
        }
    }
}
