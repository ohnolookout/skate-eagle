using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using RotaryHeart.Lib.SerializableDictionary;

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
    public Level lastLevelLoaded;
    public SerializableDictionaryBase<string, Level> LevelDictionary => _levelDictionary; //Levels stored by UID
    public SerializableDictionaryBase<string, string> NameToUIDDictionary => _nameToUIDDictionary;
    public SerializableDictionaryBase<string, string> UIDToNameDictionary => _uidToNameDictionary;
    public List<string> LevelOrder => _levelOrder;
    public Level EditorLevel => _editorLevel;
    public bool LevelIsDirty { get => _levelIsDirty; set => _levelIsDirty = value; }
    public bool LevelOrderIsDirty { get => _levelOrderIsDirty; set => _levelOrderIsDirty = value; }

    public LevelDatabase()
    {
        _levelDictionary = new();
        _editorLevel = new("Editor Level");
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
        lastLevelLoaded = level;
        _editorLevel = level;

        EditorUtility.SetDirty(this);
        _levelIsDirty = false;
        return true;        
    }

    public bool CopyLevel(Level levelToCopy, string name)
    {
        var overwrite = false;

        // Create a copy of the level to avoid modifying the original
        Level copiedLevel = new(name)
        {
            UID = levelToCopy.UID,
            MedalTimes = levelToCopy.MedalTimes,
            SerializedObjects = levelToCopy.SerializedObjects,
            LeaderboardKey = levelToCopy.LeaderboardKey,
            CameraStartPosition = levelToCopy.CameraStartPosition,
            KillPlaneY = levelToCopy.KillPlaneY,
            StartTarget = levelToCopy.StartTarget,
            RootCameraTarget = levelToCopy.RootCameraTarget
        };

        if (UIDExists(copiedLevel.UID))
        {
            overwrite = EditorUtility.DisplayDialog("Overwrite Level", $"Level with UID {copiedLevel.UID} already exists in the target DB. Do you want to overwrite it?", "Yes", "No");
            if (!overwrite)
            {
                return false;
            }
            else
            {
                copiedLevel.UID = Guid.NewGuid().ToString();
            }
        }

        if (!overwrite && LevelNameExists(copiedLevel.Name))
        {
            EditorUtility.DisplayDialog("Duplicate Level Name", "Level name already exists. Pick unique level name to continue.", "OK");
            return false;
        }

        UpdateDictionaries(copiedLevel);
        EditorUtility.SetDirty(this);
        return true;
    }

    private void UpdateDictionaries(Level level)
    {
        _levelDictionary[level.UID] = level;
        _nameToUIDDictionary[level.Name] = level.UID;
        _uidToNameDictionary[level.UID] = level.Name;
    }

    public bool DeleteLevel(Level level)
    {
        return DeleteLevel(level.Name);
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

        _levelDictionary.Remove(uid);
        _nameToUIDDictionary.Remove(name);
        _uidToNameDictionary.Remove(uid);
        _levelOrder.Remove(name);

        if (lastLevelLoaded != null && lastLevelLoaded.UID == uid)
        {
            lastLevelLoaded = null;
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

    #region Level Editing
    public Level LoadInEditModeByName(string name)
    {
        var level = GetLevelByName(name);
        LoadInEditMode(level);
        return level;
    }

    public Level LoadInEditModeByUID(string uid)
    {
        var level = GetLevelByUID(uid);
        LoadInEditMode(level);
        return level;
    }

    public void LoadInEditMode(Level level)
    {
        _editorLevel = level;
        LoadLevel(level);
    }

    public void UpdateEditorLevel(string name, GroundManager groundManager, MedalTimes medalTimes, Vector3 cameraStartPosition = new())
    {
        _editorLevel = new(name, medalTimes, groundManager, cameraStartPosition);
        _levelIsDirty = true;
    }
    #endregion
#endif

    #region Load Level
    public Level LoadByName(string name)
    {
        var level = GetLevelByName(name);
        LoadLevel(level);
        return level;
    }

    public Level LoadByUID(string uid)
    {
        var level = GetLevelByUID(uid);
        LoadLevel(level);
        return level;
    }

    public Level LoadByIndex(int index)
    {
        var level = GetLevelByIndex(index);
        LoadLevel(level);
        return level;
    }

    public void LoadLevel(Level level)
    {
        lastLevelLoaded = level;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif

    }
    #endregion

    #region Get Level
    public Level GetLevelByIndex(int index)
    {
        if (index < 0 || index >= _levelOrder.Count)
        {
            return null;
        }
        var uid = _nameToUIDDictionary[_levelOrder[index]];
        return _levelDictionary[uid];
    }

    public Level GetLevelByName(string name)
    {
        if (name == null || !LevelNameExists(name))
        {
            return null;
        }
        var uid = GetUIDByName(name);
        return _levelDictionary[uid];
    }

    public Level GetLevelByUID(string uid)
    {
        if (uid == null || !UIDExists(uid))
        {
            return null;
        }
        return _levelDictionary[uid];
    }

    #endregion

    #region GetNext/Previous Level

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
    public int GetLevelIndex(string name)
    {
        if (name == null || !LevelNameExists(name) || !_levelOrder.Contains(name))
        {
            return -1;
        }

        return _levelOrder.IndexOf(name);
    }
    #endregion

    #region Name/UID Exists
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

    private string GetUIDByName(string name)
    {
        if (name == null || !LevelNameExists(name))
        {
            return null;
        }
        return _nameToUIDDictionary[name];
    }

    private string GetNameByUID(string uid)
    {
        if (uid == null || !UIDExists(uid))
        {
            return null;
        }
        return _uidToNameDictionary[uid];
    }

    #endregion

}

