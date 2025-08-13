using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

[CustomEditor(typeof(EditManager))]
public class EditManagerInspector : Editor
{
    private EditManager _editManager;
    private LevelDatabase _levelDB;
    private bool _showMedals = false;
    private bool _showCamStart = false;

    public bool debugMode = false;
    public static string[] editModeStrings = { "Insert", "Shift" };
    public static int editMode = 0;
    public override void OnInspectorGUI()
    {
        _editManager = (EditManager)target;

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
        SaveLoadBar(_editManager, _levelDB);


        if (_levelDB.lastLevelLoaded != null)
        {
            GUILayout.Space(20);
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _showMedals = EditorGUILayout.Foldout(_showMedals, "Medal Times");
            if (_showMedals)
            {
                _editManager.medalTimes.Red = EditorGUILayout.FloatField("Red", _editManager.medalTimes.Red, GUILayout.ExpandWidth(false));
                _editManager.medalTimes.Blue = EditorGUILayout.FloatField("Blue", _editManager.medalTimes.Blue, GUILayout.ExpandWidth(false));
                _editManager.medalTimes.Gold = EditorGUILayout.FloatField("Gold", _editManager.medalTimes.Gold, GUILayout.ExpandWidth(false));
                _editManager.medalTimes.Silver = EditorGUILayout.FloatField("Silver", _editManager.medalTimes.Silver, GUILayout.ExpandWidth(false));
                _editManager.medalTimes.Bronze = EditorGUILayout.FloatField("Bronze", _editManager.medalTimes.Bronze, GUILayout.ExpandWidth(false));
            }

            _showCamStart = EditorGUILayout.Foldout(_showCamStart, "Camera Start Position");
            if (_showCamStart)
            {
                _editManager.cameraStartPosition = EditorGUILayout.Vector2Field("Camera Start Point", _editManager.cameraStartPosition);
            }

            if (EditorGUI.EndChangeCheck())
            {
                _editManager.UpdateEditorLevel();
            }

            GUILayout.Space(20);
            GUILayout.Label("Utilities", EditorStyles.boldLabel);

            if (GUILayout.Button("Add Ground", GUILayout.ExpandWidth(false)))
            {
                Selection.activeGameObject = _editManager.AddGround().gameObject;
                _editManager.UpdateEditorLevel();
            }

            if (GUILayout.Button("Populate Default Targets", GUILayout.ExpandWidth(false)))
            {
                var grounds = _editManager.GroundManager.GetGrounds();

                foreach (var ground in grounds)
                {
                    Undo.RecordObject(ground, "Populating default targets.");
                    GroundInspector.PopulateDefaultTargets(ground, _editManager);
                }

                _editManager.UpdateEditorLevel();
            }

        }
        else
        {
            GUILayout.Label("No Level Found. Save, load, or create new level to edit.", EditorStyles.boldLabel);
        }

        EditorGUI.BeginChangeCheck();
        editMode = GUILayout.Toolbar(editMode, editModeStrings);
        if (EditorGUI.EndChangeCheck())
        {
            _editManager.doShiftEdits = editMode == 1;
        }

        debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode, GUILayout.ExpandWidth(false));
    }


    public static void SaveLoadBar(EditManager levelEditManager, LevelDatabase levelDB)
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
    private EditManager _levelEditManager;
    private string _newName;
    private Level _level;

    public void Init(EditManager levelEditManager, Level level)
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
    private EditManager _levelEditManager;
    private string _newName;

    public void Init(EditManager levelEditManager)
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
    private EditManager _editManager;
    private LevelDatabase _levelDB;
    private CameraManager _cameraManager;

    [MenuItem("Window/Editor Toolbar")]
    public static void ShowWindow()
    {
        GetWindow<EditorToolbar>();
    }

    private void OnGUI()
    {
        if (_editManager == null)
        {
            _editManager = FindFirstObjectByType<EditManager>();
        }

        if (_editManager == null)
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


        EditManagerInspector.SaveLoadBar(_editManager, _levelDB);

        GUILayout.Space(10);
        GUILayout.Label("Edit Mode", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        EditManagerInspector.editMode = GUILayout.Toolbar(EditManagerInspector.editMode, EditManagerInspector.editModeStrings);
        if (EditorGUI.EndChangeCheck())
        {
            _editManager.doShiftEdits = EditManagerInspector.editMode == 1;
        }        

        if (GUILayout.Button("Refresh Level", GUILayout.ExpandWidth(false)))
        {
            _editManager.UpdateEditorLevel();
        }

        GUILayout.Space(10);
        GUILayout.Label("Add/Remove", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Ground", GUILayout.ExpandWidth(false)))
        {
            Selection.activeGameObject = _editManager.AddGround().gameObject;
            _editManager.UpdateEditorLevel();
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