using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/*
public class LevelDesigner : EditorWindow
{
    public string _name;
    public MedalTimes _medalTimes = new();
    public List<LevelSection> _levelSections = new();
    private GroundManager _groundManager;
    private LevelManager _levelManager;
    private bool isLevelEditor;
    ScriptableObject _target;
    SerializedObject _so;
    SerializedProperty _serializedMedalTimes, _serializedLevelSections;
    Vector2 _scrollPosition;
    Level _currentLevel;
    EditorLoadWindow _loadWindow;

    [MenuItem("Tools/LevelDesigner")]
    public static void ShowWindow()
    {
        GetWindow<LevelDesigner>();
    }

    private void OnEnable()
    {
        isLevelEditor = SceneManager.GetActiveScene().name == "Level_Designer";
        if (isLevelEditor)
        {
            AddTerrainGeneration();
        }
        _target = this;
        _so = new(_target);
        _currentLevel = Resources.Load<Level>("EditorLevel");
        UpdateFields();
        _serializedMedalTimes = _so.FindProperty("_medalTimes");
        _serializedLevelSections = _so.FindProperty("_levelSections");

    }


    private void OnGUI()
    {
        if ((SceneManager.GetActiveScene().name == "Level_Designer") != isLevelEditor)
        {
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
                AddTerrainGeneration();
                _groundManager.DeleteChildren();
            }
        }
        else
        {
            GUILayout.Label("Must be in Level Designer to generate levels.");
            if (GUILayout.Button("Load Level Designer Scene", GUILayout.ExpandWidth(false)))
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Level_Designer.unity");
            }
        }
        if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
        {
            SaveLevel();
        }
        if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
        {
            _loadWindow = GetWindow<EditorLoadWindow>();
            _loadWindow.Init(this);
        }
        if (GUILayout.Button("Reset to Default", GUILayout.ExpandWidth(false)))
        {
            _currentLevel.ManualReset();
            UpdateFields();
        }
    }

    private void PopulateEditorFields()
    {
        GUILayout.Label("Level Designer", EditorStyles.boldLabel);

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
        _levelManager.SetLevel(_currentLevel);
        _groundManager.GenerateGround(_currentLevel);
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
        AssetDatabase.CreateAsset(DeepCopy.CopyLevel(_currentLevel), path);
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
    }
    private void AddTerrainGeneration()
    {
        _levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        _groundManager = GameObject.FindGameObjectWithTag("TerrainManager").GetComponent<GroundManager>();

    }

}
*/
