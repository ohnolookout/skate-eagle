using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using RotaryHeart.Lib.SerializableDictionary;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "New Level", menuName = "ScriptableObjects/LevelDatabase")]
[Serializable]
public class LevelDatabase : ScriptableObject
{
    [SerializeField] private SerializableDictionaryBase<string, Level> _levelDictionary;
    public string currentLevelName;
    public SerializableDictionaryBase<string, Level> LevelDictionary => _levelDictionary;

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
        if(level == null)
        {
            Debug.Log("Level is null");
            return false;
        }
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
        EditorUtility.SetDirty(this);
        return true;        
    }

    public Level LoadLevel(string name)
    {
        if (LevelNameExists(name))
        {
            currentLevelName = name;
            Debug.Log($"Level {name} found.");
            return _levelDictionary[name];
        }

        Debug.Log($"Level {name} does not exist");
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
            if(currentLevelName == name)
            {
                currentLevelName = null;
            }
            return true;
        }
        return false;
    }

    public string[] LevelNames()
    {
        return _levelDictionary.Keys.ToArray();
    }
}

