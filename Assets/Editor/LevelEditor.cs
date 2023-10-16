using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class LevelEditor : EditorWindow
{
    public string _name;
    public MedalTimes _medalTimes = new();
    public List<LevelSection> _levelSections = new();
    public GroundSpawner _groundSpawner;
    private LiveRunManager _logic;
    private bool isLevelEditor;
    private LevelDataManager levelManager;
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
        isLevelEditor = SceneManager.GetActiveScene().name == "Level_Editor";
        if (isLevelEditor)
        {
            levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelDataManager>();
            AddTerrainGeneration();
            levelManager.currentLevel = _currentLevel;
        }
        _target = this;
        _so = new(_target);
        _currentLevel = (Level)AssetDatabase.LoadAssetAtPath("Assets/Session Data/EditorLevel.asset", typeof(Level));
        _currentLevel.CacheSections();
        UpdateFields();
        _serializedMedalTimes = _so.FindProperty("_medalTimes");
        _serializedLevelSections = _so.FindProperty("_levelSections");

    }


    private void OnGUI()
    {
        if ((SceneManager.GetActiveScene().name == "Level_Editor") != isLevelEditor) {
            isLevelEditor = !isLevelEditor;
            if (isLevelEditor)
            {
                AddTerrainGeneration();
            }
        }
        PopulateEditorFields();
        if (isLevelEditor)
        {
            if (GUILayout.Button("Generate", GUILayout.ExpandWidth(false)))
            {
                GenerateLevel();
            }
            if (GUILayout.Button("Clear Level", GUILayout.ExpandWidth(false)))
            {
                _groundSpawner.DeleteChildren();
            }
        }
        else
        {
            GUILayout.Label("Must be in Level Editor to generate levels."); 
            if (GUILayout.Button("Load Level Editor Scene", GUILayout.ExpandWidth(false)))
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Level_Editor.unity");
            }
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
        if(GUILayout.Button("Log Sections", GUILayout.ExpandWidth(false))){
            LogAllSections();
        }
    }

    private void PopulateEditorFields()
    {
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _name = EditorGUILayout.TextField("Level Name", _name);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.PropertyField(_serializedMedalTimes, true);
        EditorGUILayout.PropertyField(_serializedLevelSections, true);
        EditorGUILayout.EndScrollView();
        _so.ApplyModifiedProperties();
        _so.Update();
        if (EditorGUI.EndChangeCheck())
        {
            UpdateLevel();
            _currentLevel.ValidateSections();
        }
    }

    private void GenerateLevel()
    {
        AddTerrainGeneration();
        if (!_currentLevel.Validate())
        {
            return;
        }
        UpdateLevel();
        _logic.SetLevel(_currentLevel);
        bool testModeStatus = _groundSpawner.testMode;
        _groundSpawner.testMode = true;
        _groundSpawner.GenerateLevel(_currentLevel);
        _groundSpawner.testMode = testModeStatus;
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
        if (LevelFileManagement.LevelNames().Contains($"{_name}.asset"))
        {
            bool overwrite = EditorUtility.DisplayDialog("Overwrite Level", $"Are you sure you want to overwrite {_name}?", "Yes", "No");
            if (!overwrite)
            {
                return;
            }
        }
        UpdateLevel();
        string path = $"Assets/Levels/{_name}.asset";
        AssetDatabase.CreateAsset(_currentLevel.DeepCopy(), path);
    }


    public void LoadLevel(string path) 
    {
        Level levelToLoad = (Level)AssetDatabase.LoadAssetAtPath(path, typeof(Level));
        _currentLevel.ReassignValues(levelToLoad);
        UpdateFields();
    }

    public void UpdateFields()
    {
        _levelSections = _currentLevel.LevelSections;
        _medalTimes = _currentLevel.MedalTimes;
        _name = _currentLevel.Name;
    }

    public void UpdateLevel()
    {
        _currentLevel.ReassignValues(_name, _medalTimes, _levelSections);
        _currentLevel.CacheSections();
    }
    private void AddTerrainGeneration()
    {
        _logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        _groundSpawner = GameObject.FindGameObjectWithTag("GroundSpawner").GetComponent<GroundSpawner>();

    }

    private void LogAllSections()
    {
        UpdateLevel();
        foreach(LevelSection section in _currentLevel.LevelSections)
        { 
            section.LogSectionCache();
        }
    }
}
