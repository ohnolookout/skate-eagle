#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.HableCurve;

//Handles all editor-specific functions for ground construction and destruction
[ExecuteInEditMode]
public class GroundEditManager : MonoBehaviour
{
    private GroundManager _groundManager;
    private GroundSpawner _groundSpawner;
    public GameObject startPoint;

    #region Monobehaviors
    private void Awake()
    {
        if (Application.isPlaying)
        {
            Destroy(this);
        }
        _groundManager = gameObject.GetComponentInParent<GroundManager>();
        _groundSpawner = _groundManager.groundSpawner;

        var levelDB = Resources.Load<LevelDatabase>("LevelDB");
        var level = levelDB.GetLevelByUID(levelDB.lastLevelLoadedUID);
        if (level != null && Application.isPlaying)
        {
            SerializeLevelUtility.DeserializeLevel(level, _groundManager);
        }
        else
        {
            _groundManager.ClearGround();
        }

    }

    private void OnDrawGizmosSelected()
    {
        if (startPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint.transform.position, 2f);
        }
    }
    #endregion

    #region Add
    public Ground AddGround()
    {
        var ground = _groundSpawner.AddGround();

        ground.gameObject.name = "Ground " + (_groundManager.groundContainer.transform.childCount - 1);

        return ground;
    }
    public GroundSegment AddSegment(Ground ground)
    {
        var prevTang = ground.LastSegment?.Curve.EndPoint.RightTangent ?? null;
        var segment = _groundSpawner.AddSegment(ground, CurveFactory.DefaultCurve(prevTang));
        segment.gameObject.name = SegmentName(segment);
        return segment;
    }

    public GroundSegment AddSegment(Ground ground, Curve curve)
    {
        var segment = _groundSpawner.AddSegment(ground, curve);
        segment.gameObject.name = SegmentName(segment);
        return segment;
    }

    public GroundSegment AddSegmentToFront(Ground ground, Curve curve)
    {
        var segment = InsertSegment(ground, 0, curve);
        ground.transform.position -= curve.XYDelta;
        segment.gameObject.name = SegmentName(segment);
        return segment;
    }

    public GroundSegment InsertSegment(Ground ground, int index, Curve curve)
    {
#if UNITY_EDITOR
        if (index < 0)
        {
            index = 0;
        } else if (index > ground.SegmentList.Count)
        {
            index = ground.SegmentList.Count;
        }
        Undo.RegisterFullObjectHierarchyUndo(ground, "Insert Segment");
#endif

        //Split segment list into two lists at index, remove all segments after index from original list
        var tempList = ground.SegmentList.GetRange(index, ground.SegmentList.Count - index);
        ground.SegmentList.RemoveRange(index, ground.SegmentList.Count - index);

        //Add segment using curveDef
        var newSegment = _groundSpawner.AddSegment(ground, curve);

        //Update previous segment of first segment in tempList
        if (tempList.Count > 0)
        {
            tempList[0].NextLeftSegment = newSegment;
            newSegment.NextRightSegment = tempList[0];
        }

        ground.SegmentList.AddRange(tempList);

        //Recalculate segment positions after index
        RecalculateSegments(ground, index + 1);

        return newSegment;
    }

    public GroundSegment DuplicateSegment(GroundSegment segment)
    {
        var ground = segment.parentGround;
        var index = ground.SegmentList.IndexOf(segment);
        if (index == -1)
        {
            return null;
        }

        //Copy curve definition so that original def is not modified
        List<StandardCurveSection> curveSections = DeepCopy.CopyCurveSectionList(segment.Curve.CurveSections);
        Curve newCurve = new(curveSections);
        var segIndex = ground.SegmentList.IndexOf(segment);
        GroundSegment newSeg;
        if (segIndex >= ground.SegmentList.Count - 1)
        {
            newSeg = _groundSpawner.AddSegment(ground, newCurve);
        }
        else
        {
            newSeg = InsertSegment(ground, segIndex, newCurve);
        }
        newSeg.gameObject.name = SegmentName(newSeg);

        return newSeg;
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

    public void RemoveSegment(GroundSegment segment)
    {
        var ground = segment.parentGround;
        var isLast = segment.IsLastSegment;

        if (segment.IsFinish) {
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(_groundManager.FinishLine.gameObject, "Turn off finish");
#endif
            _groundManager.FinishLine.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(ground, "Remove Segment");
#endif
        var index = ground.SegmentList.IndexOf(segment);
        ground.SegmentList.Remove(segment);

        //Update previous segment of segment after removal
        if (index == 0 && ground.SegmentList.Count > 0)
        {
            ground.SegmentList[index].NextLeftSegment = null;
        }
        else if (index < ground.SegmentList.Count)
        {
            ground.SegmentList[index].NextLeftSegment = ground.SegmentList[index - 1];
        }

#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(segment, "Remove Segment");
        Undo.DestroyObjectImmediate(segment.gameObject);
#endif

        //Rebuild collider from new last segment
        if (isLast && ground.LastSegment != null)
        {
            _groundSpawner.BuildCollider(ground.LastSegment);
        }

        RecalculateSegments(ground, index);

    }
    public void RemoveSegment(Ground ground)
    {
        if (ground.SegmentList.Count == 0)
        {
            return;
        }

        RemoveSegment(ground.SegmentList[^1]);
    }

    #endregion

    #region Recalculation
    //Recalculate segments beginning at startIndex
    //First segment to be recalculated also recalculates its curve.
    public void RecalculateSegments(Ground ground, int startIndex)
    {
#if UNITY_EDITOR
        if (startIndex < 0 || startIndex >= ground.SegmentList.Count)
        {
            return;
        }
        Undo.RegisterCompleteObjectUndo(ground, "Recalculate Segments");
#endif

        //Copy remaining elements of segmentList to temp list, remove from segmentList
        var remainingSegments = ground.SegmentList.GetRange(startIndex, ground.SegmentList.Count - startIndex);
        var groundPrefix = GroundPrefix(ground);


        for (int i = startIndex; i < ground.SegmentList.Count; i++)
        {
            //Set position of segment to end of previous segment or leave at current start position if first segment
            Vector3 startPosition = ground.SegmentList[i].NextLeftSegment != null ? ground.SegmentList[i].NextLeftSegment.EndPosition : ground.SegmentList[i].StartPosition;

#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(ground.SegmentList[i].gameObject, "Recalculate Segment");
#endif

            ground.SegmentList[i].gameObject.transform.position = startPosition;
            ground.SegmentList[i].gameObject.name = SegmentName(ground.SegmentList[i], groundPrefix);

            //Update curve for first segment to be recalculated and include tang from previous segment
            if (i == startIndex)
            {
                RefreshCurve(ground.SegmentList[i], true);
            }
        }
    }
    public void RecalculateSegments(GroundSegment segment)
    {
        var ground = segment.parentGround;
        int index = ground.SegmentList.IndexOf(segment);
        if (index == -1)
        {
            return;
        }

        RecalculateSegments(ground, index + 1);
    }

    public void RefreshCurve(GroundSegment segment, bool doUsePrevEndTang = false, bool doSetPrevSeg = false)
    {
        if (segment.LeftFloorHeight == 0)
        {
            segment.LeftFloorHeight = 100;
        }
        if (segment.RightFloorHeight == 0)
        {
            segment.RightFloorHeight = 100;
        }
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(segment, "Refreshing segment");
#endif
        Vector2? startTang = null;

        if (doUsePrevEndTang && segment.NextLeftSegment != null)
        {
            startTang = segment.NextLeftSegment.Curve.EndPoint.RightTangent;
        }

        segment.Curve.UpdateCurveSections(startTang);

        if (segment.NextLeftSegment != null && (doSetPrevSeg || segment.Curve.CurveSections[0].Type == CurveDirection.Flat))
        {
            segment.NextLeftSegment.Curve.CurveSections[^1].SetEndPointTangent(segment.Curve.CurveSections[0].StartPoint.LeftTangent);
            segment.NextLeftSegment.Curve.UpdateCurveSections();
            _groundSpawner.ApplyCurveToSegment(segment.NextLeftSegment, segment.NextLeftSegment.Curve);
        }

        _groundSpawner.ApplyCurveToSegment(segment, segment.Curve);

        segment.UpdateShadow();

        if (segment.IsStart)
        {
            SetStartPoint(segment, 1);
        }

        if (segment.IsFinish)
        {
            SetFinishLine(segment, _groundManager.FinishLine.Parameters);
        }

    }

    public void ResetSegment(GroundSegment segment)
    {
        segment.Curve = CurveFactory.DefaultCurve(segment.PrevTangent);
        RefreshCurve(segment);
        RecalculateSegments(segment);
    }

    private void RenameAll(int groundIndex)
    {
        Debug.Log("Renaming " + groundIndex + " to " + _groundManager.Grounds.Count);
        for (int i = groundIndex; i < _groundManager.Grounds.Count; i++)
        {
            var ground = _groundManager.Grounds[i];
            ground.gameObject.name = "Ground " + i;
            var groundPrefix = GroundPrefix(ground);
            for (int j = 0; j < ground.SegmentList.Count; j++)
            {
                ground.SegmentList[j].gameObject.name = SegmentName(ground.SegmentList[j], groundPrefix);
            }
        }
    }

    private string GroundPrefix(Ground ground)
    {
        return ground.gameObject.name.Remove(1, ground.gameObject.name.Length - 2);
    }

    private string SegmentName(GroundSegment segment, string groundPrefix = null)
    {
        groundPrefix = groundPrefix ?? GroundPrefix(segment.parentGround);
        return groundPrefix + " Segment " + segment.parentGround.SegmentList.IndexOf(segment);
    }
    #endregion

    #region Start/Finish
    public Vector2 SetStartPoint(GroundSegment segment, int curvePointIndex)
    {
        Undo.RegisterFullObjectHierarchyUndo(segment.gameObject, "Set start");
        segment.IsStart = true;
        segment.SetLowPoint(curvePointIndex);
        startPoint.transform.position = _groundSpawner.SetStartPoint(segment, curvePointIndex);
        return startPoint.transform.position;
    }

    public void SetFinishLine(GroundSegment segment, FinishLineParameters finishParams)
    {
        if (!ValidateFinishParameters(segment, finishParams))
        {
            return;
        }
                
        Undo.RegisterFullObjectHierarchyUndo(segment.gameObject, "Set finish line");

        if (_groundManager.FinishSegment != null && _groundManager.FinishSegment != segment)
        {
            Undo.RegisterFullObjectHierarchyUndo(_groundManager.FinishSegment.gameObject, "Remove finish line");
            _groundManager.FinishSegment.IsFinish = false;
        }

        _groundManager.FinishSegment = segment;
        segment.IsFinish = true;
        segment.SetLowPoint(finishParams.flagPointIndex);

        Undo.RegisterFullObjectHierarchyUndo(_groundManager.FinishLine.gameObject, "Set finish line");
        _groundManager.FinishLine.SetFinishLine(finishParams, null);
    }

    public void ClearFinishLine()
    {
        if (_groundManager.FinishSegment != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(_groundManager.FinishSegment.gameObject, "Clear finish line");
            _groundManager.FinishSegment.IsFinish = false;
            _groundManager.FinishSegment = null;
        }

        Undo.RegisterFullObjectHierarchyUndo(_groundManager.FinishLine.gameObject, "Clear finish line");
        _groundManager.FinishLine.ClearFinishLine();
    }

    private bool ValidateFinishParameters(GroundSegment segment, FinishLineParameters parameters)
    {
        if (segment == null)
        {
            return false;
        }

        if (parameters == null)
        {
            return false;
        }

        if (parameters.flagPointIndex < 0 || parameters.flagPointIndex >= segment.Curve.Count)
        { 
            return false;
        }

        if (parameters.backstopPointIndex < 0 || parameters.backstopPointIndex >= segment.Curve.Count)
        { 
            return false;
        }

        return true;
    }

    #endregion

        #region CameraTargeting

    public List<LinkedCameraTarget> GetAllCameraTargets()
    {
        var targets = new List<LinkedCameraTarget>();
        foreach (var ground in _groundManager.Grounds)
        {
            foreach (var segment in ground.SegmentList)
            {
                if (segment.LinkedCameraTarget != null)
                {
                    targets.Add(segment.LinkedCameraTarget);
                }
            }
        }
        return targets;
    }

    public GroundSegment FindNextSegment(GroundSegment segment, bool doLookRight, bool doLookUp, bool doLookDown)
    {
        if(doLookRight && segment.NextRightSegment != null)
        {
            Debug.Log("Right segment already exists!");
            return segment.NextRightSegment;
        }
        else if (!doLookRight && segment.NextLeftSegment != null)
        {
            Debug.Log("Left segment already exists!");
            return segment.NextLeftSegment;
        }

        var currentPos = segment.LowPoint.position;
        GroundSegment nextSegment = null;
        var nextStartX = doLookRight ? float.PositiveInfinity : float.NegativeInfinity;
        var nextStartY = doLookUp ? float.PositiveInfinity : float.NegativeInfinity;
        Vector2 nextPos = new(nextStartX, nextStartY);

        Func<Vector2, Vector2, Vector2, bool> lookHorizontal;

        lookHorizontal = doLookRight ? LookRight : LookLeft;

        Func<Vector2, Vector2, Vector2, bool> lookVertical;

        if ((doLookUp && doLookDown) || (!doLookUp && !doLookDown))
        {
            lookVertical = (Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos) => true;
        } else if (doLookUp)
        {
            lookVertical = LookUp;
        }
        else
        {
            lookVertical = LookDown;
        }

        foreach (var ground in _groundManager.Grounds)
        {
            foreach (var seg in ground.SegmentList)
            {
                if(lookHorizontal(currentPos, nextPos, seg.LowPoint.position)
                    && lookVertical(currentPos, nextPos, seg.LowPoint.position))
                {
                    nextSegment = seg;
                    nextPos = seg.LowPoint.position;
                }
            }
        }

        return nextSegment;
    }

    private bool LookRight(Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos)
    {
        if (candidatePos.x > currentPos.x && candidatePos.x < nextPos.x)
        {
            return true;
        }

        return false;
    }

    private bool LookLeft(Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos)
    {
        if (candidatePos.x < currentPos.x && candidatePos.x > nextPos.x)
        {
            return true;
        }

        return false;
    }

    private bool LookUp(Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos)
    {
        if (candidatePos.y >= currentPos.y && candidatePos.y < nextPos.y)
        {
            return true;
        }
        return false;
    }
    private bool LookDown(Vector2 currentPos, Vector2 nextPos, Vector2 candidatePos)
    {
        if (candidatePos.y <= currentPos.y && candidatePos.y > nextPos.y)
        {
            return true;
        }
        return false;
    }

    #endregion
}

#endif