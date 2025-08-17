using UnityEngine;
using UnityEditor;
using System;

public class LevelLoadWindow : EditorWindow
{
    private EditManager _editManager;
    private LevelDatabase _levelDB;
    private string[] _levelNames;
    private int _nameIndex;

    public void Init(EditManager editManager, LevelDatabase levelDB)
    {
        _editManager = editManager;
        _levelDB = levelDB;
        _levelNames = _levelDB.LevelNames();
        if (_levelDB.lastLevelLoaded != null && _levelDB.UIDExists(_levelDB.lastLevelLoaded.UID))
        {
            _nameIndex = Array.IndexOf(_levelNames, _levelDB.lastLevelLoaded.Name);
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
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

}

