using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

[CreateAssetMenu(fileName = "New Level", menuName = "ScriptableObjects/LevelDatabase")]
[Serializable]
public class LevelDatabase : ScriptableObject
{
    private Dictionary<string, Level> _levelDictionary;

    public LevelDatabase()
    {
        _levelDictionary = new();
    }

    public bool LevelNameExists(string name)
    {
        return _levelDictionary.ContainsKey(name);
    }

    public bool SaveLevel(Level level)
    {
        if (LevelNameExists(level.Name))
        {
            bool overwrite = EditorUtility.DisplayDialog("Overwrite Level", $"Are you sure you want to overwrite {level.Name}?", "Yes", "No");
            if (!overwrite)
            {
                return false;
            }

            level.UID = _levelDictionary[level.Name].UID;

        }
        _levelDictionary[level.Name] = level;
        return true;        
    }

    public Level LoadLevel(string name)
    {
        if (LevelNameExists(name))
        {
            return _levelDictionary[name];
        }
        return null;
    }

    public bool DeleteLevel(string name)
    {
        if (LevelNameExists(name))
        {
            bool doDelete = EditorUtility.DisplayDialog("Delete Level", $"Are you sure you want to delete {name}?", "Yes", "No");

            if (!doDelete)
            {
                return false;
            }

            _levelDictionary.Remove(name);
            return true;
        }
        return false;
    }

    public string[] LevelNames()
    {
        return _levelDictionary.Keys.ToArray();
    }
}

