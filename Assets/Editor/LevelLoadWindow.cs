using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

public class LevelLoadWindow : EditorWindow
{
    private GroundDesigner _groundDesigner;
    private LevelDatabase _levelDB;
    private Dictionary<string, string> _levelPathsByName = new();
    private string[] _levelNames;
    private int _nameIndex;
    public static void ShowWindow()
    {
        GetWindow<LevelLoadWindow>();
    }


    public void Init(GroundDesigner groundDesigner, LevelDatabase levelDB)
    {
        _groundDesigner = groundDesigner;
        _levelDB = levelDB;
        _levelNames = _levelDB.LevelNames();
        if (_levelDB.LevelNameExists(_levelDB.lastLevelLoaded))
        {
            _nameIndex = Array.IndexOf(_levelNames, _levelDB.lastLevelLoaded);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Select Level to Load", EditorStyles.boldLabel);
        _nameIndex = EditorGUILayout.Popup("Level to load", _nameIndex, _levelNames);

        if (GUILayout.Button("Load Level"))
        {
            _groundDesigner.LoadLevel(_levelNames[_nameIndex]);
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

