using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Codice.Client.Common.GameUI;
using static UnityEngine.Rendering.HableCurve;
using log4net.Core;

public class GroundDesigner : EditorWindow
{
    #region Declarations
    private enum GroundDesignerState
    {
        GroundSelected,
        GroundSegmentSelected,
        NoGroundSelected
    }

    private GroundSpawner _groundSpawner;
    private GroundManager _groundManager;
    private GroundDesignerState _selectionState;
    private GameObject _selectedObject;
    private SerializedObject _so;
    private SerializedProperty _serializedCurve;
    private GroundSegment _segment;
    private Ground _ground;
    private EditorLoadWindow _loadWindow;
    private Vector2 _scrollPosition;
    private LevelDatabase _levelDB;

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
        _groundSpawner = FindAnyObjectByType<GroundSpawner>();
        _groundManager = FindAnyObjectByType<GroundManager>();

        _selectedObject = Selection.activeGameObject;
        OnSelectionChanged();

        Selection.selectionChanged += OnSelectionChanged;

        LoadLevelDB();
    }

    private void OnDisable()
    {
        _groundSpawner = null;
        _groundManager = null;
        _selectedObject = null;
        Selection.selectionChanged -= OnSelectionChanged;
    }

    #region GUI
    private void OnGUI()
    {
        if(_groundSpawner == null)
        {
            EditorGUILayout.HelpBox("No GroundConstructor found in scene. Please add one to continue.", MessageType.Warning);
            return;
        }

        switch(_selectionState)
        {
            case GroundDesignerState.GroundSelected:
                OnGroundSelected();
                break;
            case GroundDesignerState.GroundSegmentSelected:
                OnGroundSegmentSelected();
                break;
            case GroundDesignerState.NoGroundSelected:
                OnNoGroundSelected();
                break;
        }
    }

    private void OnGroundSelected()
    {
        if (_ground == null)
        {
            OnSelectionChanged();
            return;
        }

        if (GUILayout.Button("Add Segment", GUILayout.ExpandWidth(false)))
        {
            _groundSpawner.AddSegment(_ground);
        }
        if (GUILayout.Button("Add Segment to Front", GUILayout.ExpandWidth(false)))
        {
            _groundSpawner.AddSegmentToFront(_ground);
        }
        if (GUILayout.Button("Remove Segment", GUILayout.ExpandWidth(false)))
        {
            _groundSpawner.RemoveSegment(_ground);
        }
        if(GUILayout.Button("Recalculate Segments", GUILayout.ExpandWidth(false)))
        {
            _groundSpawner.RecalculateSegments(_ground, 0);
        }
        if (GUILayout.Button("Delete Ground", GUILayout.ExpandWidth(false)))
        {
            _groundSpawner.RemoveGround(_selectedObject.GetComponent<Ground>());
        }
        if (GUILayout.Button("Add Start", GUILayout.ExpandWidth(false)))
        {
            var segment = _groundSpawner.AddSegmentToFront(_selectedObject.GetComponent<Ground>(), _groundSpawner.DefaultStart());
            _groundManager.SetStartPoint(segment, 1);
        }
        if (GUILayout.Button("Add Finish", GUILayout.ExpandWidth(false)))
        {
            var segment = _groundSpawner.AddSegment(_selectedObject.GetComponent<Ground>(), _groundSpawner.DefaultFinish());
            _groundManager.SetFinishPoint(segment, 1);
        }
    }

    private void OnGroundSegmentSelected()
    {
        if(_segment == null)
        {
            OnSelectionChanged();
            return;
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_serializedCurve, true);

        _so.ApplyModifiedProperties();
        _so.Update();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_segment, "Curve Change");
            _groundSpawner.RefreshCurve(_segment);
            _groundSpawner.RecalculateSegments(_segment);
        }
        if (GUILayout.Button("Duplicate", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _groundSpawner.DuplicateSegment(_segment).gameObject;
        }

        if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
        {
            _groundSpawner.ResetSegment(_segment);
        }

        if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
        {
            _groundSpawner.RemoveSegment(_segment);
            if (_segment.PreviousSegment != null)
            {
                Selection.activeGameObject = _segment.PreviousSegment.gameObject;
            } else
            {
                Selection.activeGameObject = _segment.parentGround.gameObject;
            }
            return;
        }

        if(GUILayout.Button("Set As Start", GUILayout.ExpandWidth(false)))
        {
            _groundManager.SetStartPoint(_segment, 1);
        }

        if(GUILayout.Button("Set As Finish", GUILayout.ExpandWidth(false)))
        {
            _groundManager.SetFinishPoint(_segment, 1);
        }
    }

    private void OnNoGroundSelected()
    {
        GUILayout.Label("Level: " + levelName, EditorStyles.boldLabel);
        levelName = EditorGUILayout.TextField("Level Name", levelName);

        GUILayout.Label("Medal Times");
        medalTimeRed = EditorGUILayout.FloatField("Red", medalTimeRed, GUILayout.ExpandWidth(false));
        medalTimeBlue = EditorGUILayout.FloatField("Blue", medalTimeBlue, GUILayout.ExpandWidth(false));
        medalTimeGold = EditorGUILayout.FloatField("Gold", medalTimeGold, GUILayout.ExpandWidth(false));
        medalTimeSilver = EditorGUILayout.FloatField("Silver", medalTimeSilver, GUILayout.ExpandWidth(false));
        medalTimeBronze = EditorGUILayout.FloatField("Bronze", medalTimeBronze, GUILayout.ExpandWidth(false));

        if (GUILayout.Button("Add Ground", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _groundSpawner.AddGround().gameObject;
        }
        if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
        {
            SaveLevel();
        }
        if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
        {
            _loadWindow = GetWindow<EditorLoadWindow>();
            _loadWindow.Init(this, _levelDB);
        }
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
            _selectionState = GroundDesignerState.NoGroundSelected;
        }
        else
        {
            _so = new(_selectedObject);

            if (_selectedObject.GetComponent<Ground>() != null)
            {
                _selectionState = GroundDesignerState.GroundSelected;
                _ground = _selectedObject.GetComponent<Ground>();
                return;
            } 
            else if (_selectedObject.GetComponent<GroundSegment>() != null)
            {
                _segment = _selectedObject.GetComponent<GroundSegment>();
                _selectionState = GroundDesignerState.GroundSegmentSelected;
                _so = new(_segment);
                _serializedCurve = _so.FindProperty("curve");
                return;
            }
            else
            {
                _selectionState = GroundDesignerState.NoGroundSelected;
            }

        }
        Repaint();
    }
    #endregion

    #region Level Save/Load
    private void LoadLevelDB()
    {
        var path = "Assets/LevelDatabase/LevelDB.asset";
        _levelDB = (LevelDatabase)AssetDatabase.LoadAssetAtPath(path, typeof(LevelDatabase));
        
        if(_levelDB is null)
        {
            _levelDB = CreateInstance<LevelDatabase>();
            AssetDatabase.CreateAsset(_levelDB, path);
        }

    }
    private void SaveLevel()
    {
        if (!(_loadWindow is null))
        {
            _loadWindow.Close();
        }

        MedalTimes medalTimes = new(medalTimeRed, medalTimeBlue, medalTimeGold, medalTimeSilver, medalTimeBronze);
        var levelToSave = new Level(levelName, medalTimes, GroundsArray());
        var levelSaved = _levelDB.SaveLevel(levelToSave);

        if (levelSaved)
        {
            Debug.Log($"Level {levelName} saved");
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

        this.levelName = loadedLevel.Name;

        LoadMedalTimes(loadedLevel.MedalTimes);

        _groundSpawner.DeserializeLevel(loadedLevel);
    }

    private void LoadMedalTimes(MedalTimes medalTimes)
    {
        medalTimeRed = medalTimes.Red;
        medalTimeBlue = medalTimes.Blue;
        medalTimeGold = medalTimes.Gold;
        medalTimeSilver = medalTimes.Silver;
        medalTimeBronze = medalTimes.Bronze;
    }

    private Ground[] GroundsArray()
    {
        return _groundManager.groundContainer.GetComponentsInChildren<Ground>(); 
    }

    #endregion
}
