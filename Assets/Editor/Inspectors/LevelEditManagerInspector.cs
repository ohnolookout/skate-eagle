using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

[CustomEditor(typeof(LevelEditManager))]
public class LevelEditManagerInspector : Editor
{
    private LevelEditManager _levelEditManager;
    private LevelDatabase _levelDB;
    private LevelLoadWindow _loadWindow;

    public bool debugMode = false;
    public override void OnInspectorGUI()
    {
        _levelEditManager = (LevelEditManager)target;
        _levelDB = Resources.Load<LevelDatabase>("LevelDB");

        if(_levelDB == null)
        {
            return;
        }

        if (_levelDB.lastLevelLoaded != null)
        {
            GUILayout.Label("Level: " + _levelDB.lastLevelLoaded.Name, EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Medal Times", EditorStyles.boldLabel);
            _levelEditManager.medalTimes.Red = EditorGUILayout.FloatField("Red", _levelEditManager.medalTimes.Red, GUILayout.ExpandWidth(false));
            _levelEditManager.medalTimes.Blue = EditorGUILayout.FloatField("Blue", _levelEditManager.medalTimes.Blue, GUILayout.ExpandWidth(false));
            _levelEditManager.medalTimes.Gold = EditorGUILayout.FloatField("Gold", _levelEditManager.medalTimes.Gold, GUILayout.ExpandWidth(false));
            _levelEditManager.medalTimes.Silver = EditorGUILayout.FloatField("Silver", _levelEditManager.medalTimes.Silver, GUILayout.ExpandWidth(false));
            _levelEditManager.medalTimes.Bronze = EditorGUILayout.FloatField("Bronze", _levelEditManager.medalTimes.Bronze, GUILayout.ExpandWidth(false));

            GUILayout.Label("Camera Start Point", EditorStyles.boldLabel);
            _levelEditManager.cameraStartPosition = EditorGUILayout.Vector2Field("Camera Start Point", _levelEditManager.cameraStartPosition);

            if (EditorGUI.EndChangeCheck())
            {
                _levelEditManager.UpdateEditorLevel();
            }

            if (GUILayout.Button("Add Ground", GUILayout.ExpandWidth(false)))
            {
                Selection.activeGameObject = _levelEditManager.AddGround().gameObject;
                _levelEditManager.UpdateEditorLevel();
            }
        } else
        {
            GUILayout.Label("No Level Found. Save, load, or create new level to edit.", EditorStyles.boldLabel);
        }

        if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
        {
            _levelEditManager.SaveLevel();
        }

        if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
        {
            if (!_levelEditManager.DoDiscardChanges())
            {
                return;
            }
            _loadWindow = EditorWindow.GetWindow<LevelLoadWindow>();
            _loadWindow.Init(_levelEditManager, _levelDB);
        }

        if (GUILayout.Button("Rename Level", GUILayout.ExpandWidth(false)))
        {
            if (!_levelEditManager.DoDiscardChanges())
            {
                return;
            }
            var renameWindow = EditorWindow.GetWindow<RenameLevelWindow>();
            renameWindow.Init(_levelEditManager, _levelDB.lastLevelLoaded);
        }

        if (GUILayout.Button("New Level", GUILayout.ExpandWidth(false)))
        {
            if (!_levelEditManager.DoDiscardChanges())
            {
                return;
            }

            var newLevelWindow = EditorWindow.GetWindow<NewLevelWindow>();
            newLevelWindow.Init(_levelEditManager);
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

        debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode, GUILayout.ExpandWidth(false));
    }



    #region Editor Windows
    private class RenameLevelWindow : EditorWindow
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
                } else
                {
                    EditorUtility.DisplayDialog("Name Unchanged", $"Level is already named {_newName}.", "OK");
                }
            }

            if(GUILayout.Button("Save as New", GUILayout.ExpandWidth(false)))
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

            if(GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
            {
                Close();
            }
        }
    }


    private class NewLevelWindow : EditorWindow
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

            if(GUILayout.Button("Create Level", GUILayout.ExpandWidth(false)))
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


    #endregion
}
