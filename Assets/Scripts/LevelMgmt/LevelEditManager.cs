#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

//Handles all editor-specific functions for ground construction and destruction
[ExecuteInEditMode]
public class LevelEditManager : MonoBehaviour
{
    //Level editing parameters
    private GroundManager _groundManager;
    public string levelName = "New Level";
    public MedalTimes medalTimes = new();
    public Vector3 cameraStartPosition = new Vector3(116, 6);

    private GroundSpawner _groundSpawner;
    private LevelDatabase _levelDB;
    private bool _debugMode = false;

    public GroundManager GroundManager => _groundManager;
    public GroundSpawner GroundSpawner => _groundSpawner;

    #region Monobehaviors
    private void Awake()
    {
        _groundManager = gameObject.GetComponentInParent<GroundManager>();
        _groundSpawner = _groundManager.groundSpawner;

        if (_groundManager == null)
        {
            Debug.LogWarning("GroundManager not found in parent hierarchy.");
            return;
        }

        _levelDB = Resources.Load<LevelDatabase>("LevelDB");

        if (_levelDB is null)
        {
            Debug.Log("No level database found.");
            return;
        }
        else
        {
            Debug.Log("Level Database loaded with " + _levelDB.LevelDictionary.Count + " levels.");
        }

        LoadLevel(_levelDB.EditorLevel);
    }

    #endregion

    #region Save/Load Level
    public void SaveLevel()
    {
        var levelToSave = new Level(levelName, medalTimes, _groundManager, cameraStartPosition);
        var levelSaved = _levelDB.SaveLevel(levelToSave);

        if (levelSaved)
        {
            Debug.Log($"Level {levelName} saved");
        }
        else
        {
            Debug.Log($"Level {levelName} failed to save");
        }

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    public void LoadLevel(string levelName)
    {
        var level = _levelDB.GetLevelByName(levelName);
        LoadLevel(level);
    }

    public void LoadLevel(Level level)
    {
        _levelDB.LoadInEditMode(level);
        levelName = level.Name;
        medalTimes = level.MedalTimes;
        cameraStartPosition = level.CameraStartPosition;

        SerializeLevelUtility.DeserializeLevel(level, _groundManager);
    }

    public void NewLevel()
    {
        levelName = "New Level";
        DefaultMedalTimes();
        _groundManager.ClearGround();
        UpdateEditorLevel();
    }

    public bool DoDiscardChanges()
    {
        if (_debugMode)
        {
            // In debug mode, always discard changes
            return true;
        }

        if (_levelDB.LevelIsDirty)
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

    #region Add
    public Ground AddGround()
    {
        var ground = _groundSpawner.AddGround();

        ground.gameObject.name = "Ground " + (_groundManager.groundContainer.transform.childCount - 1);

        return ground;
    }

    #endregion

    #region Remove
    public void RemoveGround(Ground ground)
    {
        var index = _groundManager.Grounds.IndexOf(ground);

        Undo.RegisterFullObjectHierarchyUndo(_groundManager, "Remove ground");
        _groundManager.Grounds.Remove(ground);
        Undo.DestroyObjectImmediate(ground.gameObject);
        RenameAll(index);

    }

    #endregion

    #region Recalculation

    private void RenameAll(int groundIndex)
    {
        Debug.Log("Renaming " + groundIndex + " to " + _groundManager.Grounds.Count);
        for (int i = groundIndex; i < _groundManager.Grounds.Count; i++)
        {
            var ground = _groundManager.Grounds[i];
            ground.gameObject.name = "Ground " + i;
        }
    }

    #endregion

    #region Start/Finish

    public void ClearFinishLine()
    {
        Undo.RegisterFullObjectHierarchyUndo(_groundManager.FinishLine.gameObject, "Clear finish line");
        _groundManager.FinishLine.Clear();
    }

    //private bool ValidateFinishParameters(GroundSegment segment, SerializedFinishLine parameters)
    //{
    //    if (segment == null)
    //    {
    //        return false;
    //    }

    //    if (parameters == null)
    //    {
    //        return false;
    //    }

    //    if (parameters.flagPointIndex < 0 || parameters.flagPointIndex >= segment.Curve.Count)
    //    { 
    //        return false;
    //    }

    //    if (parameters.backstopPointIndex < 0 || parameters.backstopPointIndex >= segment.Curve.Count)
    //    { 
    //        return false;
    //    }

    //    return true;
    //}

    #endregion

    public void UpdateEditorLevel()
    {
        _levelDB.UpdateEditorLevel(levelName, _groundManager, medalTimes, cameraStartPosition);
    }

    public void DefaultMedalTimes()
    {
        medalTimes.Red = 6;
        medalTimes.Blue = 8;
        medalTimes.Gold = 10;
        medalTimes.Silver = 14;
        medalTimes.Bronze = 20;
    }

    public Level CreateLevel(string name, MedalTimes medalTimes, GroundManager manager, Vector3? cameraStartPoint = null)
    {
        return new Level(name, medalTimes, _groundManager);
    }

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
}

#endif