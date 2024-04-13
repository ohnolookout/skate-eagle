using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

[CreateAssetMenu(fileName = "New Level", menuName = "ScriptableObjects/LevelData")]
[Serializable]
public class Level : ScriptableObject
{
    public string _UID;
    public string _name;
    public MedalTimes _medalTimes;
    public List<LevelSection> _levelSections;

    public Level()
    {
        _name = "Default Level";
        _medalTimes = new(60, 45, 30, 20, 10);
        _levelSections = new();
        _levelSections.Add(new LevelSection());
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (string.IsNullOrWhiteSpace(_UID))
        {
            _UID = Guid.NewGuid().ToString();            
            EditorUtility.SetDirty(this);
        }
#endif
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
        _levelSections = DeepCopy.CopyLevelSections(levelSections);
    }

    public void ReassignValues(Level level)
    {
        _name = level.Name;
        _medalTimes = DeepCopy.CopyMedalTimes(level.MedalTimes);
        _levelSections = DeepCopy.CopyLevelSections(level.LevelSections);
    }

    public string Name
    {
        get
        {
            return _name;
        }
    }

    public string UID
    {
        get
        {
            return _UID;
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

    public Dictionary<Grade, Sequence> GenerateSequences()
    {
        Dictionary<Grade, Sequence> sequences = new();
        foreach (LevelSection section in _levelSections)
        {
            sequences[section.Grade] = section.GenerateSequence();
        }
        return sequences;
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

    public Medal MedalFromTime(float timeInSeconds)
    {
        return _medalTimes.MedalFromTime(timeInSeconds);
    }
}
