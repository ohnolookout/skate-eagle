using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public class LevelDesigner : EditorWindow
{
    #region Declarations

    private GroundEditManager _groundEditor;
    private GroundManager _groundManager;
    private int _tabIndex = 0;
    private GameObject _selectedObject;
    private SerializedObject _so;
    private SerializedProperty _serializedCurve;
    private SerializedProperty _serializedLeftTargetObjects;
    private SerializedProperty _serializedRightTargetObjects;
    private GroundSegment _segment;
    private Ground _ground;
    private LevelLoadWindow _loadWindow;
    private FindAdjacentSegmentWindow _findAdjacentWindow;
    private FinishLineCreatorWindow _finishWindow;
    private Vector2 _scrollPosition;
    private LevelDatabase _levelDB;
    private Scene _activeScene;

    private bool _debugMode = false;
    private bool _groundEditorFound = false;
    private Vector3? _lastTransformPosition = null;

    private string levelName = "New Level";
    public float medalTimeRed = 6;
    public float medalTimeBlue = 8;
    public float medalTimeGold = 10;
    public float medalTimeSilver = 14;
    public float medalTimeBronze = 20;

    public Vector2 cameraStartPosition = new(-35, 15);
    private Vector2 _defaultTang = new(-1, 1);

    #endregion

    [MenuItem("Tools/LevelDesigner")]
    
    #region Monobehaviors
    public static void ShowWindow()
    {
        GetWindow<LevelDesigner>();
    }

    private void OnEnable()
    {
        _activeScene = SceneManager.GetActiveScene();
        _groundEditor = FindAnyObjectByType<GroundEditManager>();
        _groundManager = FindAnyObjectByType<GroundManager>();

        if(_groundEditor == null || _groundManager == null)
        {
            _groundEditorFound = false;
            return;
        } else
        {
            _groundEditorFound = true;
        }

        _selectedObject = Selection.activeGameObject;
        OnSelectionChanged();

        Selection.selectionChanged += OnSelectionChanged;

        LoadLevelDB();
        LoadLevel(_levelDB.EditorLevel);
    }

    private void OnDisable()
    {
        if (!DoDiscardChanges())
        {
            SaveLevel();
        }
        _groundEditor = null;
        _groundManager = null;
        _selectedObject = null;
        Selection.selectionChanged -= OnSelectionChanged;
    }

    void Update()
    {
        if (_selectedObject != null && _lastTransformPosition != null && _selectedObject.transform.position != (Vector3)_lastTransformPosition)
        {
            _lastTransformPosition = _selectedObject.transform.position;
            SetLevelDirty();
        }
    }
    #endregion

    #region GUI
    private void OnGUI()
    {
        if(Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Level Designer is not available in play mode.", MessageType.Warning);
            return;
        }

        if (SceneManager.GetActiveScene() != _activeScene)
        {
            OnEnable();
        }

        if (!_groundEditorFound)
        {
            EditorGUILayout.HelpBox("No GroundConstructor found in scene. Please add one to continue.", MessageType.Warning);
            if (GUILayout.Button("Check for Ground Editor"))
            {
                OnEnable();
            }
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

        GUILayout.Label("Medal Times", EditorStyles.boldLabel);
        medalTimeRed = EditorGUILayout.FloatField("Red", medalTimeRed, GUILayout.ExpandWidth(false));
        medalTimeBlue = EditorGUILayout.FloatField("Blue", medalTimeBlue, GUILayout.ExpandWidth(false));
        medalTimeGold = EditorGUILayout.FloatField("Gold", medalTimeGold, GUILayout.ExpandWidth(false));
        medalTimeSilver = EditorGUILayout.FloatField("Silver", medalTimeSilver, GUILayout.ExpandWidth(false));
        medalTimeBronze = EditorGUILayout.FloatField("Bronze", medalTimeBronze, GUILayout.ExpandWidth(false));

        GUILayout.Label("Camera Start Point", EditorStyles.boldLabel);
        cameraStartPosition = EditorGUILayout.Vector2Field("Camera Start Point", cameraStartPosition);

        if (EditorGUI.EndChangeCheck())
        {
            SetLevelDirty();
        }

        if (GUILayout.Button("Add Ground", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _groundEditor.AddGround().gameObject;
            _tabIndex = 1;
            SetLevelDirty();
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
            _levelDB.EditorLevel = CreateLevel();
            _levelDB.LevelIsDirty = false;
            _lastTransformPosition = null;
        }

        if (GUILayout.Button("Clear Finish Line", GUILayout.ExpandWidth(false)))
        {
            if (!EditorUtility.DisplayDialog("Warning", "This will clear the current finish line.", "OK", "Cancel"))
            {
                return;
            }

            _groundEditor.ClearFinishLine();
        }

        _debugMode = EditorGUILayout.Toggle("Debug Mode", _debugMode, GUILayout.ExpandWidth(false));
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
            _tabIndex = 2;
            SetLevelDirty();
        }
        if (GUILayout.Button("Add Segment to Front", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _groundEditor.AddSegmentToFront(_ground, CurveFactory.DefaultCurve(_defaultTang)).gameObject;
            SetLevelDirty();
        }
        if (GUILayout.Button("Remove Segment", GUILayout.ExpandWidth(false)))
        {
            if (_ground.LastSegment.IsFinish)
            {
                _groundManager.FinishSegment = null;
            }

            if(_ground.LastSegment.IsStart)
            {
                _groundManager.StartSegment = null;
            }

            _groundEditor.RemoveSegment(_ground);

            if(Selection.activeGameObject == null)
            {
                Selection.activeGameObject = _ground.gameObject;
            }

            SetLevelDirty();
        }
        if(GUILayout.Button("Recalculate Segments", GUILayout.ExpandWidth(false)))
        {
            _groundEditor.RecalculateSegments(_ground, 0);
            SetLevelDirty();
        }
        if (GUILayout.Button("Delete Ground", GUILayout.ExpandWidth(false)))
        {
            _groundEditor.RemoveGround(_ground);
            
            if (Selection.activeGameObject == null)
            {
                Selection.activeGameObject = _groundManager.gameObject;
            }
            _tabIndex = 0;
            SetLevelDirty();
        }
        if (GUILayout.Button("Add Start", GUILayout.ExpandWidth(false)))
        {
            var segment = _groundEditor.AddSegmentToFront(_ground, CurveFactory.DefaultStartLine());            
            Selection.activeGameObject = segment.gameObject;

            //Remove old start segment
            if(_groundManager.StartSegment != null && _groundManager.StartSegment != segment)
            {
                _groundManager.StartSegment.IsStart = false;
            }

            _groundManager.StartSegment = segment;

            _groundEditor.SetStartPoint(segment, 1);

            SetLevelDirty();
        }
        if (GUILayout.Button("Add Finish", GUILayout.ExpandWidth(false)))
        {
            CurvePoint? startPoint = null;
            if (_ground.SegmentList.Count > 0)
            {
                startPoint = _ground.SegmentList[^1].Curve.EndPoint;
            }

            var segment = _groundEditor.AddSegment(_ground, CurveFactory.DefaultFinishLine(startPoint));
            Selection.activeGameObject = segment.gameObject;         

            _groundEditor.SetFinishLine(segment, new SerializedFinishLine(segment));
            
            SetLevelDirty();
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

        GUILayout.Space(20);

        _segment.LeftFloorHeight = EditorGUILayout.IntField("Left Floor Height", _segment.LeftFloorHeight, GUILayout.ExpandWidth(false));
        _segment.LeftFloorAngle = EditorGUILayout.IntField("Left Floor Angle", _segment.LeftFloorAngle, GUILayout.ExpandWidth(false));
        _segment.RightFloorHeight = EditorGUILayout.IntField("Right Floor Height", _segment.RightFloorHeight, GUILayout.ExpandWidth(false));
        _segment.RightFloorAngle = EditorGUILayout.IntField("Right Floor Angle", _segment.RightFloorAngle, GUILayout.ExpandWidth(false));

        EditorGUILayout.PropertyField(_serializedLeftTargetObjects, true);
        EditorGUILayout.PropertyField(_serializedRightTargetObjects, true);

        _segment.IsFloating = EditorGUILayout.Toggle("Floating", _segment.IsFloating, GUILayout.ExpandWidth(false));
        _segment.IsInverted = EditorGUILayout.Toggle("Invert Collider", _segment.IsInverted, GUILayout.ExpandWidth(false));
        _segment.HasShadow = EditorGUILayout.Toggle("Has Shadow", _segment.HasShadow, GUILayout.ExpandWidth(false));
        _segment.UseDefaultHighLowPoints = EditorGUILayout.Toggle("Use Default High/Low", _segment.UseDefaultHighLowPoints, GUILayout.ExpandWidth(false));
        _segment.DoTarget = EditorGUILayout.Toggle("Do Target", _segment.DoTarget, GUILayout.ExpandWidth(false));

        _so.ApplyModifiedProperties();
        _so.Update();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_segment, "Curve Change");
            _groundEditor.RefreshCurve(_segment, false, true);
            _groundEditor.RecalculateSegments(_segment);
            SetLevelDirty();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Find Next Segments", GUILayout.ExpandWidth(false)))
        {
            _findAdjacentWindow = GetWindow<FindAdjacentSegmentWindow>();
            _findAdjacentWindow.Init(this, _groundEditor, _segment);
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Duplicate", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _groundEditor.DuplicateSegment(_segment).gameObject;
            SetLevelDirty();
        }

        if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
        {
            _groundEditor.ResetSegment(_segment);
            SetLevelDirty();
        }

        if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
        {
            var segment = _segment;
            if (segment.NextLeftSegment != null)
            {
                Selection.activeGameObject = segment.NextLeftSegment.gameObject;
            } else
            {
                Selection.activeGameObject = segment.parentGround.gameObject;
            }

            if(segment.IsFinish)
            {
                _groundManager.FinishSegment = null;
            }
            if (segment.IsStart)
            {
                _groundManager.StartSegment = null;
            }

            _tabIndex = 1;       
            _groundEditor.RemoveSegment(segment);
            SetLevelDirty();
        }

        if (GUILayout.Button("Finish Line Options", GUILayout.ExpandWidth(false)))
        {
            _finishWindow = GetWindow<FinishLineCreatorWindow>();
            _finishWindow.Init(_segment, _groundManager, _groundEditor);
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

        if(_selectedObject.transform != null)
        {
            _lastTransformPosition = _selectedObject.transform.position;
        }

        _so = new(_selectedObject);

        if (_selectedObject.GetComponent<Ground>() != null)
        {
            _ground = _selectedObject.GetComponent<Ground>();
            _segment = _ground.SegmentList.Count > 0 ? _ground.SegmentList[^1] : null;

            if(_segment != null)
            {
                SelectSegment(_segment);
            }
            
            return;
        } 
        else if (_selectedObject.GetComponent<GroundSegment>() != null)
        {
            _segment = _selectedObject.GetComponent<GroundSegment>();
            SelectSegment(_segment);
            _ground = _segment.parentGround;
            return;
        }
        else if(_selectedObject.transform.parent != null && _selectedObject.transform.parent.GetComponent<GroundSegment>() != null)
        {
            _segment = _selectedObject.transform.parent.GetComponent<GroundSegment>();
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
        if(_groundManager == null)
        {
            return;
        }
        if (_groundManager.groundContainer.transform.childCount > 0)
        {
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
        _serializedCurve = _so.FindProperty(nameof(GroundSegment.curve));
        _serializedLeftTargetObjects = _so.FindProperty(nameof(GroundSegment.leftTargetObjects));
        _serializedRightTargetObjects = _so.FindProperty(nameof(GroundSegment.rightTargetObjects));
    }
    #endregion

    #region Level Save/Load
    private void LoadLevelDB()
    {
        _levelDB = Resources.Load<LevelDatabase>("LevelDB");
        
        if(_levelDB is null)
        {
            Debug.Log("No level database found.");
        } else
        {
            Debug.Log("Level Database loaded with " + _levelDB.LevelDictionary.Count + " levels.");
        }

    }

    public void SetLevelDirty()
    {
        _levelDB.LevelIsDirty = true;
        _levelDB.EditorLevel = CreateLevel();
    }

    private void SaveLevel()
    {
        Debug.Log("Saving level " + levelName);

        if (!(_loadWindow is null))
        {
            _loadWindow.Close();
        }


        var levelToSave = CreateLevel();
        _levelDB.EditorLevel = levelToSave;

        var levelSaved = _levelDB.SaveLevel(levelToSave);

        if (levelSaved)
        {
            Debug.Log($"Level {levelName} saved");
            _levelDB.LevelIsDirty = false;
        }
        else
        {
            Debug.Log($"Level {levelName} failed to save");
        }

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        _lastTransformPosition = null;

    }

    private Level CreateLevel()
    {
        MedalTimes medalTimes = new(medalTimeBronze, medalTimeSilver, medalTimeGold, medalTimeBlue, medalTimeRed);
        var groundsArray = GroundsArray();
        var killPlaneY = GetKillPlaneY(groundsArray);
        
        if(_groundManager.StartSegment != null)
        {
            _groundEditor.SetStartPoint(_groundManager.StartSegment, 1);
        }

        if (_groundManager.FinishSegment != null)
        {
            _groundEditor.SetFinishLine(_groundManager.FinishSegment, _groundManager.FinishLine.Parameters);
        }

        var rootCameraTarget = KDTreeBuilder.BuildKdTree(_groundEditor.GetAllCameraTargets());

        return new Level(levelName, medalTimes, groundsArray, _groundEditor.startPoint.transform.position, 
            cameraStartPosition, killPlaneY, _groundManager.FinishLine, rootCameraTarget);
    }
    public void LoadLevelByName(string levelName)
    {
        var levelToLoad = _levelDB.GetLevelByName(levelName);

        if (levelToLoad is null)
        {
            Debug.Log($"Level {levelName} failed to load");
            return;
        }
        _levelDB.LevelIsDirty = false;

        LoadLevel(levelToLoad);
    }

    private void LoadLevel(Level levelToLoad)
    {
        Debug.Log("Loading level " + levelToLoad.Name);
        _levelDB.EditorLevel = levelToLoad;
        levelName = levelToLoad.Name;

        LoadMedalTimes(levelToLoad.MedalTimes);

        _groundEditor.startPoint.transform.position = levelToLoad.StartPoint;
        cameraStartPosition = levelToLoad.CameraStartPosition;

        SerializeLevelUtility.DeserializeLevel(levelToLoad, _groundManager);
        _lastTransformPosition = null;
    }

    private void LoadMedalTimes(MedalTimes medalTimes)
    {
        medalTimeRed = medalTimes.Red;
        medalTimeBlue = medalTimes.Blue;
        medalTimeGold = medalTimes.Gold;
        medalTimeSilver = medalTimes.Silver;
        medalTimeBronze = medalTimes.Bronze;
    }
    #endregion

    #region Reset
    private void ResetMedalTimes()
    {
        medalTimeRed = 0;
        medalTimeBlue = 0;
        medalTimeGold = 0;
        medalTimeSilver = 0;
        medalTimeBronze = 0;
    }


    private bool DoDiscardChanges()
    {
        if (_debugMode)
        {
            // In debug mode, always discard changes
            return true;
        }

        if (_levelDB.LevelIsDirty || SceneManager.GetActiveScene().isDirty)
        {
            var discardChanges = EditorUtility.DisplayDialog("Warning: Unsaved Changes", $"Discard unsaved changes to {levelName}?", "Yes", "No");
            if (!discardChanges)
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                return false;
            }
            _levelDB.LevelIsDirty = false;
        }

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        return true;
    }
    #endregion

    #region Build Utilities

    private float GetKillPlaneY(Ground[] groundsArray)
    {
        float lowY = float.PositiveInfinity;
        foreach (var ground in groundsArray)
        {
            foreach (var segment in ground.SegmentList)
            {
                var newY = segment.transform.TransformPoint(segment.LowPoint.position).y;
                if (newY < lowY)
                {
                    lowY = newY;
                }
            }
        }

        return lowY - 10;

    }
    private Ground[] GroundsArray()
    {
        return _groundManager.groundContainer.GetComponentsInChildren<Ground>();
    }

    #endregion
}
