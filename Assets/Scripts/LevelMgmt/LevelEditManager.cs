#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

//Handles all editor-specific functions for ground construction and destruction
[ExecuteInEditMode]
public class LevelEditManager : MonoBehaviour
{
    #region Declarations
    //Level editing parameters
    private GroundManager _groundManager;
    public MedalTimes medalTimes = new();
    public Vector3 cameraStartPosition = new Vector3(116, 6);

    private GroundSpawner _groundSpawner;
    private LevelDatabase _levelDB;
    private bool _debugMode = false;
    public bool doShiftEdits = false;

    public GroundManager GroundManager => _groundManager;
    public GroundSpawner GroundSpawner => _groundSpawner;
    #endregion

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
        Undo.ClearAll();
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

    private readonly Vector3 _cpDelta = new(40, 0);

    public CurvePointEditObject InsertCurvePoint(Ground ground, int index)
    {
        var cpCount = ground.CurvePoints.Count;
        Vector3 pos;
        Vector3 leftTang;

        if (cpCount == 0)
        {
            index = 0;
            pos = ground.transform.position;
            leftTang = new(-10, 8);
        } else if (index == 0) {
            var cp = ground.CurvePoints[0];
            pos = cp.Position - _cpDelta;
            leftTang = new(cp.LeftTangent.x, -cp.LeftTangent.y);
        } else
        {
            index = Math.Min(index, cpCount);
            var cp = ground.CurvePoints[index - 1];
            pos = cp.Position + _cpDelta;
            leftTang = new Vector3(-cp.RightTangent.x, cp.RightTangent.y);
        }

        CurvePoint newPoint = new(pos, leftTang, -leftTang);
        Undo.RegisterFullObjectHierarchyUndo(ground.gameObject, "Inserted point");
        var cpObj = ground.SetCurvePoint(newPoint, index);

        if (doShiftEdits)
        {
            ShiftCurvePoints(cpObj, _cpDelta);
        }

        RefreshSerializable(ground);

        return cpObj;
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
    public void OnUpdateTransform(GameObject obj)
    {
        if (GroundManager.FinishLine.IsParentGround(obj))
        {
            GroundManager.FinishLine.Refresh();
        }

        if (GroundManager.StartLine.IsParentGround(obj))
        {
            GroundManager.StartLine.Refresh();
        }
    }
    public void UpdateEditorLevel()
    {
        _levelDB.UpdateEditorLevel(_levelDB.lastLevelLoaded.Name, _groundManager, medalTimes, cameraStartPosition);
    }

    public void RefreshSerializable(ISerializable serializable)
    {
        _levelDB.UpdateEditorLevel(_levelDB.lastLevelLoaded.Name, _groundManager, medalTimes, cameraStartPosition);

        Undo.RegisterFullObjectHierarchyUndo(serializable.GameObject, "Update ISerializable");
        serializable.Refresh(_groundManager);

        if (GroundManager.FinishLine.IsParentGround(serializable.GameObject))
        {
            GroundManager.FinishLine.Refresh();
        }

    }
    private void RenameAll(int startIndex, Ground[] grounds)
    {
        for (int i = startIndex; i < grounds.Length; i++)
        {
            var ground = grounds[i];
            ground.gameObject.name = "Ground " + i;
        }
    }
    
    public void DefaultMedalTimes()
    {
        medalTimes.Red = 6;
        medalTimes.Blue = 8;
        medalTimes.Gold = 10;
        medalTimes.Silver = 14;
        medalTimes.Bronze = 20;
    }

    public void ShiftCurvePoints(CurvePointEditObject editedCP, Vector3 delta)
    {
        var index = editedCP.transform.GetSiblingIndex() + 1;
        var groundCPs = editedCP.ParentGround.CurvePointObjects;

        for (int i = index; i < groundCPs.Length; i++)
        {
            groundCPs[i].PositionChanged(groundCPs[i].transform.position + delta);
        }
    }

    #endregion

}

#endif