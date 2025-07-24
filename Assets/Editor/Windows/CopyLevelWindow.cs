using UnityEngine;
using UnityEditor;
using System;

public class CopyLevelWindow : EditorWindow
{
    private LevelDatabase _copyFromDB;
    private LevelDatabase _copyToDB;
    private string[] _levelNames;
    private string _levelNameToCopy;
    private int _nameIndex = 0;
    private bool _initialized = false;
    public static void ShowWindow()
    {
        GetWindow<CopyLevelWindow>();
    }


    public void Init(LevelDatabase levelDB)
    {
        _copyFromDB = levelDB;
        _copyToDB = GetMainDB();

        _levelNames = _copyFromDB.LevelNames();
        _levelNameToCopy = _levelNames.Length > 0 ? _levelNames[0] : string.Empty;

        _initialized = true;
    }

    private void OnGUI()
    {
        if (!_initialized)
        {
            return;
        }

        GUILayout.Label("Select Level To Copy", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        _nameIndex = EditorGUILayout.Popup("Level to Copy", _nameIndex, _levelNames);

        if(EditorGUI.EndChangeCheck())
        {
            _levelNameToCopy = _levelNames[_nameIndex];
        }

        _levelNameToCopy = EditorGUILayout.TextField("Level Name to Copy", _levelNameToCopy);

        if (GUILayout.Button("Copy Level"))
        {
            var levelToCopy = _copyFromDB.LoadByName(_levelNames[_nameIndex]);
            bool copySuccessful = _copyToDB.CopyLevel(levelToCopy, _levelNameToCopy);
            
            string message = copySuccessful ? "Level copied successfully." : "Failed to copy level.";
            string title = copySuccessful ? "Copy Successful" : "Copy Failed";

            EditorUtility.DisplayDialog(title, message, "OK");
        }

        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

    private LevelDatabase GetMainDB()
    {
        var mainDB = Resources.Load<LevelDatabase>("LevelDB");
        if (mainDB == null)
        {
            EditorUtility.DisplayDialog("No DB Found", "Level DB not found in Resources. Make sure level DB is in resources and named 'LevelDB.'", "OK");
            Close();
        }

        if(mainDB == _copyFromDB)
        {
            EditorUtility.DisplayDialog("Main DB Selected", "Main level DB already selected. Select backup DB to copy from.", "OK");
            Close();
        }

        return mainDB;
    }

}

