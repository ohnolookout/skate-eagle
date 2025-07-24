using UnityEditor;
using UnityEngine;
using System;
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
            _curvePointObject.NextRightCurvePointObject = FindNextCurvePoint(_curvePointObject, true, false, false);
            _editManager.UpdateEditorLevel();
        }

        if (GUILayout.Button("Find Right Curve Point Up"))
        {
            _curvePointObject.NextRightCurvePointObject = FindNextCurvePoint(_curvePointObject, true, true, false);
            _editManager.UpdateEditorLevel();
        }

        if (GUILayout.Button("Find Right Curve Point Down"))
        {
            _curvePointObject.NextRightCurvePointObject = FindNextCurvePoint(_curvePointObject, true, false, true);
            _editManager.UpdateEditorLevel();
        }

        if (GUILayout.Button("Find Left Curve Point Neutral"))
        {
            _curvePointObject.NextLeftCurvePointObject = FindNextCurvePoint(_curvePointObject, false, false, false);
            _editManager.UpdateEditorLevel();
        }

        if (GUILayout.Button("Find Left Curve Point Up"))
        {
            _curvePointObject.NextLeftCurvePointObject = FindNextCurvePoint(_curvePointObject, false, true, false);
            _editManager.UpdateEditorLevel();
        }

        if (GUILayout.Button("Find Left Curve Point Down"))
        {
            _curvePointObject.NextLeftCurvePointObject = FindNextCurvePoint(_curvePointObject, false, false, true);
            _editManager.UpdateEditorLevel();
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

        foreach (var ground in _editManager.GroundManager.Grounds)
        {
            foreach (var obj in ground.CurvePointObjects)
            {
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

    //public GroundSegment FindNextSegment(GroundSegment segment, bool doLookRight, bool doLookUp, bool doLookDown)
    //{
    //    if(doLookRight && segment.NextRightSegment != null)
    //    {
    //        Debug.Log("Right segment already exists!");
    //        return segment.NextRightSegment;
    //    }
    //    else if (!doLookRight && segment.NextLeftSegment != null)
    //    {
    //        Debug.Log("Left segment already exists!");
    //        return segment.NextLeftSegment;
    //    }

    //    var currentPos = segment.LowPoint.position;
    //    GroundSegment nextSegment = null;
    //    var nextStartX = doLookRight ? float.PositiveInfinity : float.NegativeInfinity;
    //    var nextStartY = doLookUp ? float.PositiveInfinity : float.NegativeInfinity;
    //    Vector2 nextPos = new(nextStartX, nextStartY);

    //    Func<Vector2, Vector2, Vector2, bool> lookHorizontal;

    //    lookHorizontal = doLookRight ? LookRight : LookLeft;

    //    Func<Vector2, Vector2, Vector2, bool> lookVertical;

    //    if ((doLookUp && doLookDown) || (!doLookUp && !doLookDown))
    //    {
    //        lookVertical = (Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos) => true;
    //    } else if (doLookUp)
    //    {
    //        lookVertical = LookUp;
    //    }
    //    else
    //    {
    //        lookVertical = LookDown;
    //    }

    //    foreach (var ground in _groundManager.Grounds)
    //    {
    //        foreach (var seg in ground.SegmentList)
    //        {
    //            if(lookHorizontal(currentPos, nextPos, seg.LowPoint.position)
    //                && lookVertical(currentPos, nextPos, seg.LowPoint.position))
    //            {
    //                nextSegment = seg;
    //                nextPos = seg.LowPoint.position;
    //            }
    //        }
    //    }

    //    return nextSegment;
    //}

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
}