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

        GUILayout.Label("Level: " + _levelEditManager.levelName, EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        _levelEditManager.levelName = EditorGUILayout.TextField("Level Name", _levelEditManager.levelName);

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
        if (GUILayout.Button("New Level", GUILayout.ExpandWidth(false)))
        {
            if (!_levelEditManager.DoDiscardChanges())
            {
                return;
            }

            _levelEditManager.NewLevel();
        }

        if (GUILayout.Button("Clear Finish Line", GUILayout.ExpandWidth(false)))
        {
            if (!EditorUtility.DisplayDialog("Warning", "This will clear the current finish line.", "OK", "Cancel"))
            {
                return;
            }

            _levelEditManager.ClearFinishLine();
        }

        debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode, GUILayout.ExpandWidth(false));
    }
}
