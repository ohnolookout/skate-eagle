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
    #region Declarations
    [SerializeField] private SerializableDictionaryBase<string, Level> _levelDictionary;
    [SerializeField] private SerializableDictionaryBase<string, string> _nameToUIDDictionary;
    [SerializeField] private SerializableDictionaryBase<string, string> _uidToNameDictionary;
    [SerializeField] private List<string> _levelOrder;
    [SerializeField] private Level _editorLevel;
    [SerializeField] private bool _levelIsDirty = false;
    [SerializeField] private bool _levelOrderIsDirty = false;
    public string lastLevelLoadedUID;
    public SerializableDictionaryBase<string, Level> LevelDictionary => _levelDictionary; //Levels stored by UID
    public SerializableDictionaryBase<string, string> NameToUIDDictionary => _nameToUIDDictionary;
    public SerializableDictionaryBase<string, string> UIDToNameDictionary => _uidToNameDictionary;
    public List<string> LevelOrder => _levelOrder;
    public Level EditorLevel {get => _editorLevel; set => _editorLevel = value; }
    public bool LevelIsDirty { get => _levelIsDirty; set => _levelIsDirty = value; }
    public bool LevelOrderIsDirty { get => _levelOrderIsDirty; set => _levelOrderIsDirty = value; }

    public LevelDatabase()
    {
        _levelDictionary = new();
        _editorLevel = new("Editor Level", new(), new Ground[0]);
    }
    #endregion

#if UNITY_EDITOR
    #region Save/Delete Level
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

            level.UID = _nameToUIDDictionary[level.Name];

        } else
        {
            level.UID = Guid.NewGuid().ToString();
        }
        UpdateDictionaries(level);
        lastLevelLoadedUID = level.UID;

        EditorUtility.SetDirty(this);
        return true;        
    }

    private void UpdateDictionaries(Level level)
    {
        _levelDictionary[level.UID] = level;
        _nameToUIDDictionary[level.Name] = level.UID;
        _uidToNameDictionary[level.UID] = level.Name;
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
        var uid = _nameToUIDDictionary[name];

        _levelDictionary.Remove(name);
        _nameToUIDDictionary.Remove(name);
        _uidToNameDictionary.Remove(uid);
        _levelOrder.Remove(name);

        if (lastLevelLoadedUID == uid)
        {
            lastLevelLoadedUID = null;
        }

        EditorUtility.SetDirty(this);
        return true;
    }

    public void ChangeLevelName(Level level, string newName)
    {
        var uid = level.UID;
        var oldName = level.Name;
        level.Name = newName;        

        _nameToUIDDictionary.Remove(oldName);
        _nameToUIDDictionary[newName] = uid;
        _uidToNameDictionary[uid] = newName;
        _levelDictionary[uid] = level; 

        if (_levelOrder.Contains(oldName))
        {
            var index = _levelOrder.IndexOf(oldName);
            _levelOrder[index] = newName;
        }
        EditorUtility.SetDirty(this);
    }
    #endregion
#endif

    #region Get Level Methods

    public Level GetLevelByName(string name)
    {
        if (name == null || !LevelNameExists(name))
        {
            return null;
        }

        var uid = _nameToUIDDictionary[name];
        lastLevelLoadedUID = uid;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif

        return _levelDictionary[uid];
    }

    public Level GetLevelByUID(string uid)
    {
        if (uid == null || !UIDExists(uid))
        {
            return null;
        }
        lastLevelLoadedUID = uid;
        return _levelDictionary[uid];
    }

    public Level GetLevelByIndex(int index)
    {
        if (index < 0 || index >= _levelOrder.Count)
        {
            return null;
        }
        return GetLevelByName(_levelOrder[index]);
    }

    public int GetLevelIndex(string name)
    {
        if (name == null || !LevelNameExists(name) || !_levelOrder.Contains(name))
        {
            return -1;
        }

        return _levelOrder.IndexOf(name);
    }

    public int GetLevelIndex(Level level)
    {
        return GetLevelIndex(level.Name);
    }

    public Level GetNextLevel(Level currentLevel)
    {
        return GetNextLevel(currentLevel.Name);
    }

    public Level GetNextLevel(string name)
    {
        if (name == null || !LevelNameExists(name))
        {
            return null;
        }
        var index = GetLevelIndex(name);

        if (index == -1 || index == _levelOrder.Count - 1)
        {
            return null;
        }

        return GetLevelByIndex(index + 1);
    }

    public Level GetPreviousLevel(Level currentLevel)
    {
        return GetPreviousLevel(currentLevel.Name);
    }

    public Level GetPreviousLevel(string name)
    {
        if (name == null || !LevelNameExists(name))
        {
            return null;
        }

        var index = GetLevelIndex(name);

        if (index == -1 || index == 0)
        {
            return null;
        }

        return GetLevelByIndex(index - 1);
    }

    public string GetUID(string name)
    {
        if (name == null || !LevelNameExists(name))
        {
            return null;
        }
        return _nameToUIDDictionary[name];
    }

    public string GetName(string uid)
    {
        if (uid == null || !UIDExists(uid))
        {
            return null;
        }
        return _uidToNameDictionary[uid];
    }

    public string[] LevelNames()
    {
        return _nameToUIDDictionary.Keys.ToArray();
    }

    public bool LevelNameExists(string name)
    {
        if (name == null)
        {
            return false;
        }
        return _nameToUIDDictionary.ContainsKey(name);
    }

    public bool UIDExists(string uid)
    {
        if (uid == null)
        {
            return false;
        }
        return _uidToNameDictionary.ContainsKey(uid);
    }

    #endregion

}

