#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

//Handles all editor-specific functions for ground construction and destruction
[ExecuteInEditMode]
public class LevelEditManager : MonoBehaviour
{
    //Level editing parameters
    private GroundManager _groundManager;
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
        var levelToSave = new Level(_levelDB.lastLevelLoaded.Name, medalTimes, _groundManager, cameraStartPosition);
        var levelSaved = _levelDB.SaveLevel(levelToSave);

        if (levelSaved)
        {
            Debug.Log($"Level {_levelDB.lastLevelLoaded.Name} saved");
        }
        else
        {
            Debug.Log($"Level {_levelDB.lastLevelLoaded.Name} failed to save");
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
        medalTimes = level.MedalTimes;
        cameraStartPosition = level.CameraStartPosition;

        SerializeLevelUtility.DeserializeLevel(level, _groundManager);
    }

    public void NewLevel(string levelName = "New Level")
    {
        //Clear level params
        DefaultMedalTimes();
        _groundManager.ClearGround();

        var newLevel = new Level(levelName, medalTimes, _groundManager);

        _levelDB.SaveLevel(newLevel);
    }

    public void RenameLevel(Level level, string newName)
    {
        var levelToSave = new Level(newName, medalTimes, _groundManager, cameraStartPosition);
        _levelDB.ChangeLevelName(level, newName);
    }

    public void SaveLevelAsNew(Level level, string name)
    {
        var levelToSave = new Level(name, medalTimes, _groundManager, cameraStartPosition);
        _levelDB.SaveLevel(levelToSave);
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
            var discardChanges = EditorUtility.DisplayDialog("Warning: Unsaved Changes", $"Discard unsaved changes to {_levelDB.lastLevelLoaded.Name}?", "Yes", "No");
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

    public bool LevelNameExists(string name)
    {
        return _levelDB.LevelNameExists(name);
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
        var grounds = _groundManager.GetGrounds();
        var index = Array.IndexOf(grounds, ground);

        Undo.DestroyObjectImmediate(ground.gameObject);
        RenameAll(index, grounds);

    }

    #endregion

    #region Recalculation
    public void OnUpdateTransform()
    {
        GroundManager.FinishLine.UpdateFinish();
    }
    public void UpdateEditorLevel()
    {
        _levelDB.UpdateEditorLevel(_levelDB.lastLevelLoaded.Name, _groundManager, medalTimes, cameraStartPosition);
    }

    public void RefreshLevelGeneration()
    {
        string selectedObjName = Selection.activeGameObject.name;

        _levelDB.UpdateEditorLevel(_levelDB.lastLevelLoaded.Name, _groundManager, medalTimes, cameraStartPosition);
        SerializeLevelUtility.DeserializeLevel(_levelDB.EditorLevel, _groundManager);

        Selection.activeGameObject = GameObject.Find(selectedObjName);
    }

    public void RefreshSerializable(ISerializable serializable)
    {
        _levelDB.UpdateEditorLevel(_levelDB.lastLevelLoaded.Name, _groundManager, medalTimes, cameraStartPosition);
        var deserializable = serializable.Serialize();


    }
    private void RenameAll(int startIndex, Ground[] grounds)
    {
        for (int i = startIndex; i < grounds.Length; i++)
        {
            var ground = grounds[i];
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

    

    public void DefaultMedalTimes()
    {
        medalTimes.Red = 6;
        medalTimes.Blue = 8;
        medalTimes.Gold = 10;
        medalTimes.Silver = 14;
        medalTimes.Bronze = 20;
    }

}

#endif