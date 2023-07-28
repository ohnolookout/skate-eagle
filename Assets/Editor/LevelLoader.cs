using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class LevelLoader : EditorWindow
{
    private LevelEditor _levelEditor;
    private Dictionary<string, string> _levelPathsByName = new();
    private string[] _levelNames;
    private int _nameIndex;
    public static void ShowWindow()
    {
        GetWindow<LevelLoader>();
    }

    private void OnEnable()
    {
        _levelPathsByName = LevelManagement.LevelPathsByName();
        _levelNames = _levelPathsByName.Keys.ToArray();
    }

    public void Init(LevelEditor levelEditor)
    {
        _levelEditor = levelEditor;
    }

    private void OnGUI()
    {
        GUILayout.Label("Select Level to Load", EditorStyles.boldLabel);
        _nameIndex = EditorGUILayout.Popup("Level to load", _nameIndex, _levelNames);
        if(GUILayout.Button("Load Level"))
        {
            _levelEditor.LoadLevel(_levelPathsByName[_levelNames[_nameIndex]]);
            Close();
        }
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

}
