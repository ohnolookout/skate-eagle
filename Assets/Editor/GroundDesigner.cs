using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Codice.Client.Common.GameUI;
using static UnityEngine.Rendering.HableCurve;
using log4net.Core;
using UnityEditor.Build;

public class GroundDesigner : EditorWindow
{
    #region Declarations

    private GroundEditManager _groundEditor;
    private GroundManager _groundManager;
    private int _tabIndex = 0;
    private GameObject _selectedObject;
    private SerializedObject _so;
    private SerializedProperty _serializedCurve;
    private GroundSegment _segment;
    private Ground _ground;
    private LevelLoadWindow _loadWindow;
    private Vector2 _scrollPosition;
    private LevelDatabase _levelDB;
    private bool _levelIsDirty = false;

    private string levelName = "New Level";
    public float medalTimeRed = 0;
    public float medalTimeBlue = 0;
    public float medalTimeGold = 0;
    public float medalTimeSilver = 0;
    public float medalTimeBronze = 0;


    #endregion
    [MenuItem("Tools/GroundDesigner")]
    public static void ShowWindow()
    {
        GetWindow<GroundDesigner>();
    }

    private void OnEnable()
    {
        _groundEditor = FindAnyObjectByType<GroundEditManager>();
        _groundManager = FindAnyObjectByType<GroundManager>();

        _selectedObject = Selection.activeGameObject;
        OnSelectionChanged();

        Selection.selectionChanged += OnSelectionChanged;

        LoadLevelDB();
        if (_levelDB.lastLevelLoaded != null && _levelDB.LevelNameExists(_levelDB.lastLevelLoaded))
        {
            LoadLevel(_levelDB.lastLevelLoaded);
        }
    }

    private void OnDisable()
    {
        if(!DoDiscardChanges())
        {
            SaveLevel();
        }
        _groundEditor = null;
        _groundManager = null;
        _selectedObject = null;
        Selection.selectionChanged -= OnSelectionChanged;
    }

    #region GUI
    private void OnGUI()
    {
        if(_groundEditor == null)
        {
            EditorGUILayout.HelpBox("No GroundConstructor found in scene. Please add one to continue.", MessageType.Warning);
            return;
        }
        _tabIndex = GUILayout.Toolbar(_tabIndex, new string[] { "Level", "Ground", "Segment" });
        switch (_tabIndex)
        {
            case 0:
                LevelMenu();
                break;
            case 1:
                GroundMenu();
                break;
            case 2:
                SegmentMenu();
                break;
        }
    }
    private void LevelMenu()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Label("Level: " + levelName, EditorStyles.boldLabel);
        levelName = EditorGUILayout.TextField("Level Name", levelName);

        GUILayout.Label("Medal Times");
        medalTimeRed = EditorGUILayout.FloatField("Red", medalTimeRed, GUILayout.ExpandWidth(false));
        medalTimeBlue = EditorGUILayout.FloatField("Blue", medalTimeBlue, GUILayout.ExpandWidth(false));
        medalTimeGold = EditorGUILayout.FloatField("Gold", medalTimeGold, GUILayout.ExpandWidth(false));
        medalTimeSilver = EditorGUILayout.FloatField("Silver", medalTimeSilver, GUILayout.ExpandWidth(false));
        medalTimeBronze = EditorGUILayout.FloatField("Bronze", medalTimeBronze, GUILayout.ExpandWidth(false));

        if (EditorGUI.EndChangeCheck())
        {
            _levelIsDirty = true;
        }

        if (GUILayout.Button("Add Ground", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _groundEditor.AddGround().gameObject;
            _levelIsDirty = true;
        }
        if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
        {
            SaveLevel();
        }
        if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
        {
            if (!DoDiscardChanges())
            {
                return;
            }
            _loadWindow = GetWindow<LevelLoadWindow>();
            _loadWindow.Init(this, _levelDB);
        }
        if (GUILayout.Button("New Level", GUILayout.ExpandWidth(false)))
        {
            if (!DoDiscardChanges())
            {
                return;
            }
            _groundManager.ClearGround();
            levelName = "New Level";
            ResetMedalTimes();
            _levelIsDirty = false;
        }
    }
    private void GroundMenu()
    {
        if (_ground == null)
        {
            GUILayout.TextArea("No ground selected");
            return;
        }

        if (GUILayout.Button("Add Segment", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _groundEditor.AddSegment(_ground).gameObject;
            _levelIsDirty = true;
        }
        if (GUILayout.Button("Add Segment to Front", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _groundEditor.AddSegmentToFront(_ground).gameObject;
            _levelIsDirty = true;
        }
        if (GUILayout.Button("Remove Segment", GUILayout.ExpandWidth(false)))
        {
            _groundEditor.RemoveSegment(_ground);

            if(Selection.activeGameObject == null)
            {
                Selection.activeGameObject = _ground.gameObject;
            }

            _levelIsDirty = true;
        }
        if(GUILayout.Button("Recalculate Segments", GUILayout.ExpandWidth(false)))
        {
            _groundEditor.RecalculateSegments(_ground, 0);
            _levelIsDirty = true;
        }
        if (GUILayout.Button("Delete Ground", GUILayout.ExpandWidth(false)))
        {
            _groundEditor.RemoveGround(_selectedObject.GetComponent<Ground>());
            
            if (Selection.activeGameObject == null)
            {
                Selection.activeGameObject = _groundManager.gameObject;
            }
            _levelIsDirty = true;
        }
        if (GUILayout.Button("Add Start", GUILayout.ExpandWidth(false)))
        {
            var segment = _groundEditor.AddSegmentToFront(_selectedObject.GetComponent<Ground>(), _groundEditor.DefaultStart());
            _groundEditor.SetStartPoint(segment, 1);
            _levelIsDirty = true;
        }
        if (GUILayout.Button("Add Finish", GUILayout.ExpandWidth(false)))
        {
            var segment = _groundEditor.AddSegment(_selectedObject.GetComponent<Ground>(), _groundEditor.DefaultFinish());
            _groundEditor.SetFinishPoint(segment, 1);
            _levelIsDirty = true;
        }
    }

    private void SegmentMenu()
    {
        if(_segment == null)
        {
            GUILayout.TextArea("No segment selected");
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_serializedCurve, true);

        _so.ApplyModifiedProperties();
        _so.Update();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_segment, "Curve Change");
            _groundEditor.RefreshCurve(_segment);
            _groundEditor.RecalculateSegments(_segment);
            _levelIsDirty = true;
        }
        if (GUILayout.Button("Duplicate", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _groundEditor.DuplicateSegment(_segment).gameObject;
            _levelIsDirty = true;
        }

        if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
        {
            _groundEditor.ResetSegment(_segment);
            _levelIsDirty = true;
        }

        if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
        {
            _groundEditor.RemoveSegment(_segment);
            if (_segment.PreviousSegment != null)
            {
                Selection.activeGameObject = _segment.PreviousSegment.gameObject;
            } else
            {
                Selection.activeGameObject = _segment.parentGround.gameObject;
            }
            _levelIsDirty = true;
            return;
        }

        if(GUILayout.Button("Set As Start", GUILayout.ExpandWidth(false)))
        {
            _groundEditor.SetStartPoint(_segment, 1);
            _levelIsDirty = true;
        }

        if(GUILayout.Button("Set As Finish", GUILayout.ExpandWidth(false)))
        {
            _groundEditor.SetFinishPoint(_segment, 1);
            _levelIsDirty = true;
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region State Mgmt

    private void OnSelectionChanged()
    {
        _selectedObject = Selection.activeGameObject;
        _segment = null;
        _ground = null;

        if (_selectedObject == null)
        {
            AssignFirstGround();
            return;
        }

        _so = new(_selectedObject);

        if (_selectedObject.GetComponent<Ground>() != null)
        {
            _ground = _selectedObject.GetComponent<Ground>();
            _segment = _ground.SegmentList.Count > 0 ? _ground.SegmentList[^1] : null;
            SelectSegment(_segment);
            return;
        } 
        else if (_selectedObject.GetComponent<GroundSegment>() != null)
        {
            _segment = _selectedObject.GetComponent<GroundSegment>();
            SelectSegment(_segment);
            _ground = _segment.parentGround;
            return;
        }
        else
        {
            AssignFirstGround();
        }
    }

    private void AssignFirstGround()
    {
        if (_groundManager.groundContainer.transform.childCount > 0)
        {
            Debug.Log("Selecting first ground in groundcontainer...");
            _ground = _groundManager.groundContainer.transform.GetChild(0).GetComponent<Ground>();
            if (_segment == null && _ground.SegmentList.Count > 0)
            {
                _segment = _ground.SegmentList[^1];
                SelectSegment(_segment);
            }
        }
    }

    private void SelectSegment(GroundSegment segment)
    {
        _so = new(segment);
        _serializedCurve = _so.FindProperty("curve");
    }
    #endregion

    #region Level Save/Load
    private void LoadLevelDB()
    {
        var path = "Assets/LevelDatabase/LevelDB.asset";
        _levelDB = (LevelDatabase)AssetDatabase.LoadAssetAtPath(path, typeof(LevelDatabase));
        
        if(_levelDB is null)
        {
            Debug.Log("No level database found. Creating new database");
            _levelDB = CreateInstance<LevelDatabase>();
            AssetDatabase.CreateAsset(_levelDB, path);
        } else
        {
            Debug.Log("Level Database loaded with " + _levelDB.LevelDictionary.Count + " levels.");
        }

    }
    private void SaveLevel()
    {
        if (!(_loadWindow is null))
        {
            _loadWindow.Close();
        }

        MedalTimes medalTimes = new(medalTimeBronze, medalTimeSilver, medalTimeGold, medalTimeBlue, medalTimeRed);
        var levelToSave = new Level(levelName, medalTimes, GroundsArray());
        var levelSaved = _levelDB.SaveLevel(levelToSave);

        if (levelSaved)
        {
            Debug.Log($"Level {levelName} saved");
            _levelIsDirty = false;
        }
        else
        {
            Debug.Log($"Level {levelName} failed to save");
        }

    }
    public void LoadLevel(string levelName)
    {
        var loadedLevel = _levelDB.LoadLevel(levelName);

        if (loadedLevel is null)
        {
            Debug.Log($"Level {levelName} failed to load");
            return;
        }

        Debug.Log("Loading level " + loadedLevel.Name);
        this.levelName = loadedLevel.Name;

        LoadMedalTimes(loadedLevel.MedalTimes);

        SerializeLevelUtility.DeserializeLevel(loadedLevel, _groundManager);

        _levelIsDirty = false;
    }

    private void LoadMedalTimes(MedalTimes medalTimes)
    {
        medalTimeRed = medalTimes.Red;
        medalTimeBlue = medalTimes.Blue;
        medalTimeGold = medalTimes.Gold;
        medalTimeSilver = medalTimes.Silver;
        medalTimeBronze = medalTimes.Bronze;
    }

    private void ResetMedalTimes()
    {
        medalTimeRed = 0;
        medalTimeBlue = 0;
        medalTimeGold = 0;
        medalTimeSilver = 0;
        medalTimeBronze = 0;
    }

    private Ground[] GroundsArray()
    {
        return _groundManager.groundContainer.GetComponentsInChildren<Ground>(); 
    }

    private bool DoDiscardChanges()
    {
        if (_levelIsDirty)
        {
            var discardChanges = EditorUtility.DisplayDialog("Warning: Unsaved Changes", $"Discard unsaved changes to {levelName}?", "Yes", "No");
            if (!discardChanges)
            {
                return false;
            }
        }
        return true;
    }

    #endregion
}
