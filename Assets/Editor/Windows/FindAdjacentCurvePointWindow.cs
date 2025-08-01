using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
public class FindAdjacentCurvePointWindow : EditorWindow
{
    private LevelEditManager _editManager;
    private CurvePointObject _curvePointObject;

    public void Init(CurvePointObject curvePointObject)
    {
        _editManager = FindFirstObjectByType<LevelEditManager>();
        if (_editManager == null)
        {
            Debug.LogWarning("No GroundEditManager found in the scene. Closeing curve point window.");
            Close();
        }

        _curvePointObject = curvePointObject;
    }

    private void OnGUI()
    {
        GUILayout.Label("Find Adjacent Curve Point", EditorStyles.boldLabel);

        GUILayout.Space(20);

        if (GUILayout.Button("Find Right Curve Point Neutral"))
        {
            var curvePointObj = FindNextCurvePoint(_curvePointObject, true, false, false);
            AddNewTarget(curvePointObj, true);
        }

        if (GUILayout.Button("Find Right Curve Point Up"))
        {
            var curvePointObj = FindNextCurvePoint(_curvePointObject, true, true, false);
            AddNewTarget(curvePointObj, true);
        }

        if (GUILayout.Button("Find Right Curve Point Down"))
        {
            var curvePointObj = FindNextCurvePoint(_curvePointObject, true, false, true);
            AddNewTarget(curvePointObj, true);
        }

        if (GUILayout.Button("Find Left Curve Point Neutral"))
        {
            var curvePointObj = FindNextCurvePoint(_curvePointObject, false, false, false);
            AddNewTarget(curvePointObj, false);
        }

        if (GUILayout.Button("Find Left Curve Point Up"))
        {
            var curvePointObj = FindNextCurvePoint(_curvePointObject, false, true, false);
            AddNewTarget(curvePointObj, false);
        }

        if (GUILayout.Button("Find Left Curve Point Down"))
        {
            var curvePointObj = FindNextCurvePoint(_curvePointObject, false, false, true);
            AddNewTarget(curvePointObj, false);
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Close"))
        {
            Close();
        }
    }
    #region CameraTargeting
    private CurvePointObject FindNextCurvePoint(CurvePointObject curvePointObject, bool doLookRight, bool doLookUp, bool doLookDown)
    {
        if (doLookRight && curvePointObject.NextRightCurvePointObject != null)
        {
            Debug.Log("Right segment already exists!");
            return curvePointObject.NextRightCurvePointObject;
        }
        else if (!doLookRight && curvePointObject.NextLeftCurvePointObject != null)
        {
            Debug.Log("Left segment already exists!");
            return curvePointObject.NextLeftCurvePointObject;
        }

        var currentPos = curvePointObject.CurvePoint.Position;
        CurvePointObject nextCurvePointObject = null;
        var nextStartX = doLookRight ? float.PositiveInfinity : float.NegativeInfinity;
        var nextStartY = doLookUp ? float.PositiveInfinity : float.NegativeInfinity;
        Vector2 nextPos = new(nextStartX, nextStartY);

        Func<Vector2, Vector2, Vector2, bool> lookHorizontal;

        lookHorizontal = doLookRight ? LookRight : LookLeft;

        Func<Vector2, Vector2, Vector2, bool> lookVertical;

        if ((doLookUp && doLookDown) || (!doLookUp && !doLookDown))
        {
            lookVertical = (Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos) => true;
        }
        else if (doLookUp)
        {
            lookVertical = LookUp;
        }
        else
        {
            lookVertical = LookDown;
        }

        var grounds = _editManager.GroundManager.GetGrounds();
        foreach (var ground in grounds)
        {
            foreach (var obj in ground.CurvePointObjects)
            {
                if (!obj.DoTargetLow)
                {
                    continue;
                }
                if (lookHorizontal(currentPos, nextPos, obj.CurvePoint.Position)
                    && lookVertical(currentPos, nextPos, obj.CurvePoint.Position))
                {
                    nextCurvePointObject = obj;
                    nextPos = obj.CurvePoint.Position;
                }
            }
        }

        return nextCurvePointObject;
    }

    private static bool LookRight(Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos)
    {
        if (candidatePos.x > currentPos.x && candidatePos.x < nextPos.x)
        {
            return true;
        }

        return false;
    }

    private static bool LookLeft(Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos)
    {
        if (candidatePos.x < currentPos.x && candidatePos.x > nextPos.x)
        {
            return true;
        }

        return false;
    }

    private static bool LookUp(Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos)
    {
        if (candidatePos.y >= currentPos.y && candidatePos.y < nextPos.y)
        {
            return true;
        }
        return false;
    }
    private static bool LookDown(Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos)
    {
        if (candidatePos.y <= currentPos.y && candidatePos.y > nextPos.y)
        {
            return true;
        }
        return false;
    }

    #endregion

    #region Add Targets
    private void AddNewTarget(CurvePointObject newCurvePoint, bool isRight)
    {
        var targetObjList = isRight ? _curvePointObject.RightTargetObjects : _curvePointObject.LeftTargetObjects;
        var linkedCameraTargetList = isRight ? _curvePointObject.LinkedCameraTarget.RightTargets : _curvePointObject.LinkedCameraTarget.LeftTargets;

        if (newCurvePoint == null)
        {
            Debug.LogWarning("FindAdjacentCurvePoint: No target found.");
            return;
        }
        if (targetObjList.Contains(newCurvePoint.gameObject))
        {
            Debug.LogWarning("FindAdjacentCurvePoint: Curve point already exists in the target list.");
        }
        else
        {
            targetObjList.Add(newCurvePoint.gameObject);
        }

        if(linkedCameraTargetList.Contains(newCurvePoint.LinkedCameraTarget))
        {
            Debug.LogWarning("FindAdjacentCurvePoint: Linked camera target already exists in the target list.");
        }
        else
        {
            linkedCameraTargetList.Add(newCurvePoint.LinkedCameraTarget);
        }

            _editManager.UpdateEditorLevel();
    }


    #endregion
}