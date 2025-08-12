using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

[CustomEditor(typeof(LevelEditManager))]
public class LevelEditManagerInspector : Editor
{
    private LevelEditManager _levelEditManager;
    private LevelDatabase _levelDB;
    private bool _showMedals = false;
    private bool _showCamStart = false;

    public bool debugMode = false;
    public override void OnInspectorGUI()
    {
        _levelEditManager = (LevelEditManager)target;

        if (_levelDB == null)
        {
            _levelDB = Resources.Load<LevelDatabase>("LevelDB");
        }

        if(_levelDB == null)
        {
            return;
        }

        if (_levelDB.lastLevelLoaded != null)
        {
            GUILayout.Label("Level: " + _levelDB.lastLevelLoaded.Name, EditorStyles.boldLabel);
        }

        GUILayout.Space(20);
        SaveLoadBar(_levelEditManager, _levelDB);


        if (_levelDB.lastLevelLoaded != null)
        {
            GUILayout.Space(20);
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _showMedals = EditorGUILayout.Foldout(_showMedals, "Medal Times");
            if (_showMedals)
            {
                _levelEditManager.medalTimes.Red = EditorGUILayout.FloatField("Red", _levelEditManager.medalTimes.Red, GUILayout.ExpandWidth(false));
                _levelEditManager.medalTimes.Blue = EditorGUILayout.FloatField("Blue", _levelEditManager.medalTimes.Blue, GUILayout.ExpandWidth(false));
                _levelEditManager.medalTimes.Gold = EditorGUILayout.FloatField("Gold", _levelEditManager.medalTimes.Gold, GUILayout.ExpandWidth(false));
                _levelEditManager.medalTimes.Silver = EditorGUILayout.FloatField("Silver", _levelEditManager.medalTimes.Silver, GUILayout.ExpandWidth(false));
                _levelEditManager.medalTimes.Bronze = EditorGUILayout.FloatField("Bronze", _levelEditManager.medalTimes.Bronze, GUILayout.ExpandWidth(false));
            }

            _showCamStart = EditorGUILayout.Foldout(_showCamStart, "Camera Start Position");
            if (_showCamStart)
            {
                _levelEditManager.cameraStartPosition = EditorGUILayout.Vector2Field("Camera Start Point", _levelEditManager.cameraStartPosition);
            }

            if (EditorGUI.EndChangeCheck())
            {
                _levelEditManager.UpdateEditorLevel();
            }

            GUILayout.Space(20);
            GUILayout.Label("Utilities", EditorStyles.boldLabel);

            if (GUILayout.Button("Add Ground", GUILayout.ExpandWidth(false)))
            {
                Selection.activeGameObject = _levelEditManager.AddGround().gameObject;
                _levelEditManager.UpdateEditorLevel();
            }

            if (GUILayout.Button("Populate Default Targets", GUILayout.ExpandWidth(false)))
            {
                var grounds = _levelEditManager.GroundManager.GetGrounds();

                foreach (var ground in grounds)
                {
                    Undo.RecordObject(ground, "Populating default targets.");
                    GroundInspector.PopulateDefaultTargets(ground, _levelEditManager);
                }

                _levelEditManager.UpdateEditorLevel();
            }

        }
        else
        {
            GUILayout.Label("No Level Found. Save, load, or create new level to edit.", EditorStyles.boldLabel);
        }

        debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode, GUILayout.ExpandWidth(false));
        _levelEditManager.doShiftEdits = EditorGUILayout.Toggle("Shift Mode", _levelEditManager.doShiftEdits, GUILayout.ExpandWidth(false));
    }


    public static void SaveLoadBar(LevelEditManager levelEditManager, LevelDatabase levelDB)
    {
        GUILayout.Label("Save/Load", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", GUILayout.ExpandWidth(true)))
        {
            levelEditManager.SaveLevel();
        }

        if (GUILayout.Button("Load", GUILayout.ExpandWidth(true)))
        {
            if (!levelEditManager.DoDiscardChanges())
            {
                return;
            }
            var loadWindow = EditorWindow.GetWindow<LevelLoadWindow>();
            loadWindow.Init(levelEditManager, levelDB);
        }

        if (GUILayout.Button("Rename", GUILayout.ExpandWidth(true)))
        {
            if (!levelEditManager.DoDiscardChanges())
            {
                return;
            }
            var renameWindow = EditorWindow.GetWindow<RenameLevelWindow>();
            renameWindow.Init(levelEditManager, levelDB.lastLevelLoaded);
        }

        if (GUILayout.Button("New", GUILayout.ExpandWidth(true)))
        {
            if (!levelEditManager.DoDiscardChanges())
            {
                return;
            }

            var newLevelWindow = EditorWindow.GetWindow<NewLevelWindow>();
            newLevelWindow.Init(levelEditManager);
        }
        GUILayout.EndHorizontal();
    }


}



#region Editor Windows
public class RenameLevelWindow : EditorWindow
{
    private LevelEditManager _levelEditManager;
    private string _newName;
    private Level _level;

    public void Init(LevelEditManager levelEditManager, Level level)
    {
        _levelEditManager = levelEditManager;
        _level = level;
        _newName = _level.Name;
    }

    private void OnLostFocus()
    {
        Focus();
    }

    private void OnGUI()
    {
        GUILayout.Label("Rename Level", EditorStyles.boldLabel);
        _newName = EditorGUILayout.TextField("Level Name", _newName);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Rename", GUILayout.ExpandWidth(false)))
        {
            if (_newName != _level.Name)
            {
                _levelEditManager.RenameLevel(_level, _newName);
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Name Unchanged", $"Level is already named {_newName}.", "OK");
            }
        }

        if (GUILayout.Button("Save as New", GUILayout.ExpandWidth(false)))
        {
            if (_newName != _level.Name && !_levelEditManager.LevelNameExists(_newName))
            {
                _levelEditManager.SaveLevelAsNew(_level, _newName);
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Name Already Exists", $"Level already exists with name {_newName}.", "OK");
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
        {
            Close();
        }
    }
}


public class NewLevelWindow : EditorWindow
{
    private LevelEditManager _levelEditManager;
    private string _newName;

    public void Init(LevelEditManager levelEditManager)
    {
        _levelEditManager = levelEditManager;
        _newName = "New Level";
    }

    private void OnLostFocus()
    {
        Focus();
    }

    private void OnGUI()
    {
        GUILayout.Label("New Level", EditorStyles.boldLabel);
        _newName = EditorGUILayout.TextField("Level Name", _newName);

        if (GUILayout.Button("Create Level", GUILayout.ExpandWidth(false)))
        {
            if (_levelEditManager.LevelNameExists(_newName))
            {
                EditorUtility.DisplayDialog("Level Name Exists", $"{_newName} already exists.", "OK");
            }
            else
            {
                _levelEditManager.NewLevel(_newName);
                Close();
            }
        }

        if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
        {
            Close();
        }
    }

}


public class EditorToolbar : EditorWindow
{
    private LevelEditManager _levelEditManager;
    private LevelDatabase _levelDB;
    private CameraManager _cameraManager;

    [MenuItem("Window/Editor Toolbar")]
    public static void ShowWindow()
    {
        GetWindow<EditorToolbar>();
    }

    private void OnGUI()
    {
        if (_levelEditManager == null)
        {
            _levelEditManager = FindFirstObjectByType<LevelEditManager>();            
        }

        if (_levelEditManager == null)
        {
            GUILayout.Label("Edit manager not found. Add edit manager to use toolbar.");
            return;
        }

        if (_levelDB == null)
        {
            _levelDB = Resources.Load<LevelDatabase>("LevelDB");
        }

        if (_levelDB == null)
        {
            GUILayout.Label("Level database not found. Add edit manager to use toolbar.");
            return;
        }

        if (_cameraManager == null)
        {
            _cameraManager = FindFirstObjectByType<CameraManager>();
        }


        LevelEditManagerInspector.SaveLoadBar(_levelEditManager, _levelDB);

        GUILayout.Space(10);
        GUILayout.Label("Edit Mode", EditorStyles.boldLabel);
        _levelEditManager.doShiftEdits = EditorGUILayout.Toggle("Shift Mode", _levelEditManager.doShiftEdits, GUILayout.ExpandWidth(false));

        if (GUILayout.Button("Refresh Level", GUILayout.ExpandWidth(false)))
        {
            _levelEditManager.UpdateEditorLevel();
        }

        GUILayout.Space(10);
        GUILayout.Label("Add/Remove", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Ground", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _levelEditManager.AddGround().gameObject;
            _levelEditManager.UpdateEditorLevel();
        }

        if (_cameraManager != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Camera Management", EditorStyles.boldLabel);

            _cameraManager.doSetStartPosition = EditorGUILayout.Toggle("Set Camera Start Position", _cameraManager.doSetStartPosition, GUILayout.ExpandWidth(false));
        }

    }
}
#endregion