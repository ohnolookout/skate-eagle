#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public enum EditType { Insert, Shift };
//Handles all editor-specific functions for ground construction and destruction
[ExecuteInEditMode]
public class EditManager : MonoBehaviour
{
    #region Declarations
    //Level editing parameters
    private GroundManager _groundManager;
    public MedalTimes medalTimes = new();
    public Vector3 cameraStartPosition = new Vector3(116, 6);

    private GroundSpawner _groundSpawner;
    private LevelDatabase _levelDB;
    private bool _debugMode = false;
    public EditType editType = EditType.Insert;
    public const int defaultTangMang = 25;
    public static Vector3 defaultCPDelta = new(60, 0);

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

    public bool DeleteLevel(Level level)
    {
        if (level == null)
        {
            Debug.LogWarning("Cannot delete a null level.");
            return false;
        }
        if (!_levelDB.LevelDictionary.ContainsKey(level.UID))
        {
            Debug.LogWarning($"Level {level.Name} does not exist in the database.");
            return false;
        }

        var isDeleted = _levelDB.DeleteLevel(level);

        if (isDeleted)
        {
            GroundManager.ClearGround();
            _levelDB.LevelIsDirty = false;
        }
        return isDeleted;
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


    public CurvePointEditObject InsertCurvePoint(Ground ground, int index)
    {
        var cpCount = ground.CurvePoints.Count;
        Vector3 pos;
        Vector3 leftTang = new(-defaultTangMang, 0);

        if (cpCount == 0)
        {
            index = 0;
            pos = ground.transform.position;
        } else if (index == 0) {
            var cp = ground.CurvePoints[0];
            pos = cp.Position - defaultCPDelta;
        } else if(index >= cpCount || editType == EditType.Shift)
        {
            index = Math.Min(index, cpCount);
            var cp = ground.CurvePoints[index - 1];
            pos = cp.Position + defaultCPDelta;
        } else
        {
            var prevCP = ground.CurvePoints[index - 1];
            var nextCP = ground.CurvePoints[index];
            pos = BezierMath.Lerp(prevCP, nextCP, 0.5f);
            var prevRightTang = (prevCP.RightTangent.magnitude / 2) * prevCP.RightTangent.normalized;
            prevCP.Object.GetComponent<CurvePointEditObject>().RightTangentChanged(prevCP.Position + prevRightTang);
            var nextLeftTang = (nextCP.LeftTangent.magnitude / 2) * nextCP.LeftTangent.normalized;
            nextCP.Object.GetComponent<CurvePointEditObject>().LeftTangentChanged(nextCP.Position + nextLeftTang);
            leftTang = (prevCP.Position - pos) / 4;
            var invertedRightTang = (pos - nextCP.Position) / 4;
            leftTang = (leftTang + invertedRightTang) / 2;
        }

        CurvePoint newPoint = new(pos, leftTang, -leftTang, ShapeTangentMode.Continuous, true);
        Undo.RegisterFullObjectHierarchyUndo(ground.gameObject, "Inserted point");
        var cpObj = ground.SetCurvePoint(newPoint, index);

        if (editType == EditType.Shift)
        {
            ShiftCurvePoints(cpObj, defaultCPDelta);
        }

        RefreshSerializable(ground);

        return cpObj;
    }

    public void AddStart(Ground ground)
    {
        AddCurvePointsToGround(ground, CurvePointPresets.DefaultStart(ground), true);
        _groundManager.StartLine.SetStartLine(ground.CurvePointObjects[1].CurvePoint);
        ground.CurvePointObjects[2].LinkedCameraTarget.doTargetLow = true;
        
    }

    public void AddFinish(Ground ground)
    {
        AddCurvePointsToGround(ground, CurvePointPresets.DefaultFinish(ground), false);
        _groundManager.FinishLine.SetFlagPoint(ground.CurvePointObjects[^2].CurvePoint);
        _groundManager.FinishLine.SetBackstopPoint(ground.CurvePointObjects[^1].CurvePoint);
    }

    public void AddCurvePointsToGround(Ground ground, List<CurvePoint> curvePoints, bool addToFront)
    {
        Undo.RegisterFullObjectHierarchyUndo(ground.gameObject, "Added curve points to ground");
        if (addToFront)
        {
            curvePoints.Reverse();
        }
        foreach(var cp in curvePoints)
        {
            var cpObj = ground.SetCurvePoint(cp, addToFront ? 0 : -1);
        }
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

    public void RemoveCurvePoint(CurvePointEditObject cpObj, bool doSelectPrevious)
    {
        if (cpObj == null || cpObj.ParentGround == null)
        {
            Debug.LogWarning("Cannot remove a null CurvePointEditObject or its parent ground.");
            return;
        }
        var ground = cpObj.ParentGround;

        var cpIndex = cpObj.transform.GetSiblingIndex();

        Undo.DestroyObjectImmediate(cpObj.gameObject);
        RefreshSerializable(ground);

        if(doSelectPrevious && ground.CurvePointObjects.Length > 0)
        {
            var newIndex = Mathf.Max(0, cpIndex - 1);
            Selection.activeGameObject = ground.CurvePointObjects[newIndex].gameObject;
        } else
        {
            Selection.activeGameObject = ground.gameObject;
        }
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

#region CurvePoint Presets
public static class CurvePointPresets
{
    public static List<CurvePoint> DefaultStart(Ground ground)
    {
        var startPos = ground.CurvePoints.Count > 0 ? ground.CurvePoints[0].Position : new Vector3();
        var xDelta = EditManager.defaultCPDelta.x;
        var dropHeight = EditManager.defaultCPDelta.x/2;
        var tangMag = EditManager.defaultTangMang;

        var firstPoint = new CurvePoint(startPos - new Vector3(xDelta * 5, -dropHeight), ShapeTangentMode.Linear);
        var secondPoint = new CurvePoint(firstPoint.Position + new Vector3(xDelta * 3, 0), new Vector3( -tangMag, 0), new Vector3(tangMag, 0), ShapeTangentMode.Continuous, true);
        var thirdPoint = new CurvePoint(secondPoint.Position + new Vector3(60, -dropHeight), new Vector3(-tangMag, 0), new Vector3(tangMag, 0), ShapeTangentMode.Continuous, true);

        List<CurvePoint> points = new() { firstPoint, secondPoint, thirdPoint };
        return points;
    }

    public static List<CurvePoint> DefaultFinish(Ground ground)
    {
        var startPos = ground.CurvePoints.Count > 0 ? ground.CurvePoints[^1].Position : new Vector3();

        var xDelta = EditManager.defaultCPDelta.x;
        var dropHeight = EditManager.defaultCPDelta.x / 2;
        var tangMag = EditManager.defaultTangMang;

        var firstPoint = new CurvePoint(startPos + new Vector3(xDelta, -dropHeight), new Vector3(-tangMag * 1.25f, 0), new Vector3(tangMag * 1.25f, 0), ShapeTangentMode.Continuous, true);
        var secondPoint = new CurvePoint(firstPoint.Position + new Vector3(xDelta * 8, 0), ShapeTangentMode.Linear);

        List<CurvePoint> points = new() { firstPoint, secondPoint };

        return points;
    }

}





#endregion