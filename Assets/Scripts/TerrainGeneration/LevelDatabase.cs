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
    public string lastLevelLoaded;
    public SerializableDictionaryBase<string, Level> LevelDictionary => _levelDictionary;

    public LevelDatabase()
    {
        _levelDictionary = new();
    }

    public bool LevelNameExists(string name)
    {
        if(name == null)
        {
            return false;
        }
        return _levelDictionary.ContainsKey(name);
    }

    public bool SaveLevel(Level level)
    {
        if(level == null || level.Name == null)
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
        lastLevelLoaded = level.Name;
        EditorUtility.SetDirty(this);
        return true;        
    }

    public Level LoadLevel(string name)
    {
        if(name == null)
        {
            Debug.Log("Name is null");
            return null;
        }
        if (LevelNameExists(name))
        {
            lastLevelLoaded = name;
            Debug.Log($"Level {name} found.");
            EditorUtility.SetDirty(this);
            return _levelDictionary[name];
        }

        Debug.Log($"Level {name} does not exist");
        return null;
    }

    public bool DeleteLevel(string name)
    {
        if (name == null || !LevelNameExists(name))
        {
            Debug.Log("Name is null or doesn't exist in DB");
            return false;
        }

        bool doDelete = EditorUtility.DisplayDialog("Delete Level", $"Are you sure you want to delete {name}?", "Yes", "No");

        if (!doDelete)
        {
            return false;
        }

        _levelDictionary.Remove(name);
        if(lastLevelLoaded == name)
        {
            lastLevelLoaded = null;
        }
        return true;        
    }

    public string[] LevelNames()
    {
        return _levelDictionary.Keys.ToArray();
    }
}

