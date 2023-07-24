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
    public LevelData2 _currentLevelData = new();
    public GroundSpawner _groundSpawner;
    ScriptableObject _target;
    SerializedObject _so;
    SerializedProperty _serializedMedalTimes, _serializedLevelSections;
    Vector2 _scrollPosition;
    ScriptableLevelData2 _currentLevel;
    LevelLoader _loadWindow;

    [MenuItem("Tools/LevelEditor")]
    // Start is called before the first frame update
    public static void ShowWindow()
    {
        GetWindow<LevelEditor>();
    }

    private void OnEnable()
    {

        _target = this;
        _so = new(_target);
        if(_levelSections.Count < 1)
        {
            _levelSections.Add(new LevelSection());
        }
        if (_length == 0)
        {
            _length = 1500;
        }
        if (_name == "")
        {
            _name = "New Level";
        }
        _serializedMedalTimes = _so.FindProperty("_medalTimes");
        _serializedLevelSections = _so.FindProperty("_levelSections");
        _groundSpawner = GameObject.FindGameObjectWithTag("GroundSpawner").GetComponent<GroundSpawner>();
        _currentLevel = (ScriptableLevelData2)AssetDatabase.LoadAssetAtPath("Assets/Session Data/CurrentLevel.asset", typeof(ScriptableLevelData2));

    }


    private void OnGUI()
    {  
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);
        _name = EditorGUILayout.TextField("Level Name", _name);
        _length = EditorGUILayout.FloatField("Level Length", _length);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        EditorGUILayout.PropertyField(_serializedMedalTimes, true);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_serializedLevelSections, true);
        if (EditorGUI.EndChangeCheck()) {
            ValidateLevelSections();
        }
        EditorGUILayout.EndScrollView();
        _so.ApplyModifiedProperties();
        _so.Update();

        //_currentPath = EditorGUI.Popup(position, "Level to load", _currentPath, levelGuids);
        if (GUILayout.Button("Generate", GUILayout.ExpandWidth(false)))
        {
            ValidateLevelSections();
            RefreshGroundSpawner();
            _currentLevelData = GenerateLevelData();
            _currentLevel.ReassignValues(_currentLevelData);
            _groundSpawner.testMode = true;
            _groundSpawner.GenerateLevelFromSections(_currentLevelData);
            _groundSpawner.testMode = false;
        }
        if (GUILayout.Button("Clear Level", GUILayout.ExpandWidth(false)))
        {
            RefreshGroundSpawner();
            _groundSpawner.DeleteChildren();
        }
        if(GUILayout.Button("Save Level", GUILayout.ExpandWidth(false)))
        {
            if(!(_loadWindow is null))
            {
                _loadWindow.Close();
            }
            if(_name == "" || _name is null)
            {
                //Show dialogue that it must have a name.
                EditorUtility.DisplayDialog("Unnamed Level", "You can't save a level without a name!", "OK", "OK");
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
            ScriptableLevelData2 saveLevel = CreateInstance<ScriptableLevelData2>();
            saveLevel.ReassignValues(DeepCopyLevel(GenerateLevelData()));
            AssetDatabase.CreateAsset(saveLevel, $"Assets/Levels/{_name}.asset");
        }
        if(GUILayout.Button("Load Level", GUILayout.ExpandWidth(false)))
        {
            _loadWindow = GetWindow<LevelLoader>();
            _loadWindow.Init(this);
            //LoadLevel(levelGuids[_currentPath]);
        }
    }

    

    private LevelData2 GenerateLevelData()
    {
        return new LevelData2(_name, _length, _medalTimes, _levelSections);
    }

    public void LoadLevel(string path) 
    {
        ScriptableLevelData2 _levelToLoad = (ScriptableLevelData2)AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableLevelData2));
        _currentLevelData = DeepCopyLevel(_levelToLoad.LevelData);
        _levelSections = _currentLevelData.LevelSections;
        _medalTimes = _currentLevelData.MedalTimes;
        _name = _currentLevelData.Name;
        _length = _currentLevelData.Length;
    }

    private void ValidateLevelSections() 
    {
        if(_levelSections.Count < 1)
        {
            _levelSections.Add(new LevelSection());
        }
        _levelSections = _levelSections.OrderBy(section => section._startT).ToList();
        _levelSections[0]._startT = 0;
        for(int i = 0; i < _levelSections.Count; i++)
        {
            if (i < _levelSections.Count - 1)
            {
                if (_levelSections[i]._startT == _levelSections[i + 1]._startT)
                {
                    _levelSections[i + 1]._startT += 0.1f;
                }
            }
                _levelSections[i]._name = $"{(_levelSections[i]._startT * 100)}%";
            if (_levelSections[i]._curves.Count < 1)
            {
                _levelSections[i]._curves.Add(new CombinedCurveDefinition());
            }
        }


    }

    private void RefreshGroundSpawner()
    {
        if (_groundSpawner == null)
        {
            _groundSpawner = GameObject.FindGameObjectWithTag("GroundSpawner").GetComponent<GroundSpawner>();
        }
    }

    public static LevelData2 DeepCopyLevel(LevelData2 level)
    {
        string name = level.Name;
        float length = level.Length;
        MedalTimes medalTimes = DeepCopyMedalTimes(level.MedalTimes);
        List<LevelSection> levelSections = new();
        foreach(LevelSection section in level.LevelSections)
        {
            levelSections.Add(DeepCopyLevelSection(section));
        }
        return new LevelData2(name, length, medalTimes, levelSections);
    }

    public static LevelSection DeepCopyLevelSection(LevelSection section)
    {
        GradeData grade = DeepCopyGradeData(section.Grade);
        List<CombinedCurveDefinition> curves = new();
        foreach(CombinedCurveDefinition combinedCurve in section.Curves)
        {
            curves.Add(DeepCopyCombinedCurve(combinedCurve));
        }
        return new LevelSection(section.StartT, grade, curves);
    }

    public static CombinedCurveDefinition DeepCopyCombinedCurve(CombinedCurveDefinition combinedCurve)
    {
        CurveDefinition valley = DeepCopySingleCurve(combinedCurve.Valley);
        CurveDefinition peak = DeepCopySingleCurve(combinedCurve.Peak);
        return new CombinedCurveDefinition(combinedCurve.Name, valley, peak, combinedCurve.Weight);
    }

    public static CurveDefinition DeepCopySingleCurve(CurveDefinition curve)
    {
        return new CurveDefinition(curve.Length, curve.Shape, curve.Slope, curve.Skew);
    }

    public static GradeData DeepCopyGradeData(GradeData gradeData)
    {
        return new GradeData(gradeData.MinClimb, gradeData.MaxClimb);
    }

    public static MedalTimes DeepCopyMedalTimes(MedalTimes medalTimes)
    {
        return new MedalTimes(medalTimes._bronzeTime, medalTimes._silverTime, medalTimes._goldTime, medalTimes._blueTime, medalTimes._redTime);
    }
}
