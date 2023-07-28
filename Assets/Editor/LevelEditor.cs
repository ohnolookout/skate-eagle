using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class LevelEditor : EditorWindow
{
    public float _length;
    public string _name;
    public MedalTimes _medalTimes = new();
    public List<LevelSection> _levelSections = new();
    public GroundSpawner _groundSpawner;
    ScriptableObject _target;
    SerializedObject _so;
    SerializedProperty _serializedMedalTimes, _serializedLevelSections;
    Vector2 _scrollPosition;
    Level _currentLevel;
    LevelLoader _loadWindow;

    [MenuItem("Tools/LevelEditor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditor>();
    }

    private void OnEnable()
    {

        _target = this;
        _so = new(_target);
        _currentLevel = (Level)AssetDatabase.LoadAssetAtPath("Assets/Session Data/CurrentLevel.asset", typeof(Level));
        _currentLevel.ManualReset();
        UpdateFields();
        _serializedMedalTimes = _so.FindProperty("_medalTimes");
        _serializedLevelSections = _so.FindProperty("_levelSections");
        RefreshGroundSpawner();

    }


    private void OnGUI()
    {
        PopulateEditorFields();
        if (GUILayout.Button("Generate", GUILayout.ExpandWidth(false)))
        {
            GenerateLevel();
        }
        if (GUILayout.Button("Clear Level", GUILayout.ExpandWidth(false)))
        {
            RefreshGroundSpawner();
            _groundSpawner.DeleteChildren();
        }
        if(GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
        {
            SaveLevel();
        }
        if(GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
        {
            _loadWindow = GetWindow<LevelLoader>();
            _loadWindow.Init(this);
        }
        if(GUILayout.Button("Reset to Default", GUILayout.ExpandWidth(false)))
        {
            _currentLevel.ManualReset();
            UpdateFields();
        }
    }

    private void PopulateEditorFields()
    {
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);
        _name = EditorGUILayout.TextField("Level Name", _name);
        _length = EditorGUILayout.FloatField("Level Length", _length);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        EditorGUILayout.PropertyField(_serializedMedalTimes, true);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_serializedLevelSections, true);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateLevel();
            _currentLevel.ValidateSections();
        }
        EditorGUILayout.EndScrollView();
        _so.ApplyModifiedProperties();
        _so.Update();
    }

    private void GenerateLevel()
    {
        if (!_currentLevel.Validate())
        {
            return;
        }
        RefreshGroundSpawner();
        _currentLevel.ReassignValues(_name, _length, _medalTimes, _levelSections);
        _groundSpawner.testMode = true;
        _groundSpawner.GenerateLevelFromSections(_currentLevel);
        _groundSpawner.testMode = false;
    }

    private void SaveLevel()
    {
        if (!(_loadWindow is null))
        {
            _loadWindow.Close();
        }
        if (!_currentLevel.Validate())
        {
            return;
        }
        if (LevelManagement.LevelNames().Contains($"{_name}.asset"))
        {
            bool overwrite = EditorUtility.DisplayDialog("Overwrite Level", $"Are you sure you want to overwrite {_name}?", "Yes", "No");
            if (!overwrite)
            {
                return;
            }
        }
        UpdateLevel();
        Level saveLevel = _currentLevel.DeepCopy();
        AssetDatabase.CreateAsset(saveLevel, $"Assets/Levels/{_name}.asset");
    }


    public void LoadLevel(string path) 
    {
        Level _levelToLoad = (Level)AssetDatabase.LoadAssetAtPath(path, typeof(Level));
        _currentLevel.ReassignValues(_levelToLoad);
        UpdateFields();
    }

    public void UpdateFields()
    {
        _levelSections = _currentLevel.LevelSections;
        _medalTimes = _currentLevel.MedalTimes;
        _name = _currentLevel.Name;
        _length = _currentLevel.Length;
    }

    public void UpdateLevel()
    {
        _currentLevel.ReassignValues(_name, _length, _medalTimes, _levelSections);
    }
    private void RefreshGroundSpawner()
    {
        if (_groundSpawner == null)
        {
            _groundSpawner = GameObject.FindGameObjectWithTag("GroundSpawner").GetComponent<GroundSpawner>();
        }
    }

    
}
