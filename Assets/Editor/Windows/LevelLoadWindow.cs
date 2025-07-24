using UnityEngine;
using UnityEditor;
using System;

public class LevelLoadWindow : EditorWindow
{
    private LevelEditManager _editManager;
    private LevelDatabase _levelDB;
    private string[] _levelNames;
    private int _nameIndex;

    public void Init(LevelEditManager editManager, LevelDatabase levelDB)
    {
        _editManager = editManager;
        _levelDB = levelDB;
        _levelNames = _levelDB.LevelNames();
        if (_levelDB.UIDExists(_levelDB.lastLevelLoadedUID))
        {
            _nameIndex = Array.IndexOf(_levelNames, _levelDB.UIDToNameDictionary[_levelDB.lastLevelLoadedUID]);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Select Level to Load", EditorStyles.boldLabel);
        _nameIndex = EditorGUILayout.Popup("Level to load", _nameIndex, _levelNames);

        if (GUILayout.Button("Load Level"))
        {
            _editManager.LoadLevel(_levelNames[_nameIndex]);
            Close();
        }
        if(GUILayout.Button("Delete Level"))
        {
            var isDeleted = _levelDB.DeleteLevel(_levelNames[_nameIndex]);

            if(isDeleted)
            {
                _levelNames = _levelDB.LevelNames();
                _nameIndex = 0;
            }
        }
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

}

