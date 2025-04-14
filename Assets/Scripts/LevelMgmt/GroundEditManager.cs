using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

//Handles all editor-specific functions for ground construction and destruction
[ExecuteInEditMode]
public class GroundEditManager : MonoBehaviour
{
    private GroundManager _groundManager;
    private GroundSpawner _groundSpawner;
    public GameObject startPoint;
    public GameObject finishPoint;
    private Vector2 _defaultStartTang = new(1, 1);

    #region Monobehaviors
    private void Awake()
    {
        _groundManager = gameObject.GetComponentInParent<GroundManager>();
        _groundSpawner = _groundManager.groundSpawner;

        var levelDB = (LevelDatabase)AssetDatabase.LoadAssetAtPath("Assets/LevelDatabase/LevelDB.asset", typeof(LevelDatabase));
        var level = levelDB.GetLevelByUID(levelDB.lastLevelLoadedUID);
        if (level != null)
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
        if (finishPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(finishPoint.transform.position, 2f);
        }
    }
    #endregion

    #region Add
    public Ground AddGround()
    {
        return _groundSpawner.AddGround();
    }
    public GroundSegment AddSegment(Ground ground)
    {
        var prevTang = ground.LastSegment?.Curve.EndPoint.RightTangent ?? null;
        return _groundSpawner.AddSegment(ground, CurveFactory.DefaultCurve(prevTang));
    }

    public GroundSegment AddSegment(Ground ground, Curve curve)
    {
        return _groundSpawner.AddSegment(ground, curve);
    }

    public GroundSegment AddSegmentToFront(Ground ground, Curve curve)
    {
        var segment = InsertSegment(ground, 0, curve);
        ground.transform.position -= curve.XYDelta;
        return segment;
    }

    public GroundSegment InsertSegment(Ground ground, int index, Curve curve)
    {
#if UNITY_EDITOR
        if (index < 0)
        {
            index = 0;
        } else if(index > ground.SegmentList.Count)
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
            tempList[0].PreviousSegment = newSegment;
            newSegment.NextSegment = tempList[0];
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

        if (segIndex >= ground.SegmentList.Count - 1)
        {
            return _groundSpawner.AddSegment(ground, newCurve);
        }

        return InsertSegment(ground, segIndex, newCurve);
    }

    #endregion

    #region Remove
    public void RemoveGround(Ground ground)
    {
        Undo.DestroyObjectImmediate(ground.gameObject);
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
            ground.SegmentList[index].PreviousSegment = null;
        }
        else if (index < ground.SegmentList.Count)
        {
            ground.SegmentList[index].PreviousSegment = ground.SegmentList[index - 1];
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

        for (int i = startIndex; i < ground.SegmentList.Count; i++)
        {
            //Set position of segment to end of previous segment or leave at current start position if first segment
            Vector3 startPosition = ground.SegmentList[i].PreviousSegment != null ? ground.SegmentList[i].PreviousSegment.EndPosition : ground.SegmentList[i].StartPosition;

#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(ground.SegmentList[i].gameObject, "Recalculate Segment");
#endif

            ground.SegmentList[i].gameObject.transform.position = startPosition;
            ground.SegmentList[i].gameObject.name = "Segment " + i;

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
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(segment, "Refreshing segment");
#endif
        Vector2? startTang = null;

        if(doUsePrevEndTang && segment.PreviousSegment != null)
        {
            startTang = segment.PreviousSegment.Curve.EndPoint.RightTangent;
        }

        segment.Curve.UpdateCurveSections(startTang); 
        
        if ((doSetPrevSeg && segment.PreviousSegment != null) || segment.Curve.CurveSections[0].Type == CurveDirection.Flat)
        {
            segment.PreviousSegment.Curve.CurveSections[^1].SetEndPointTangent(segment.Curve.CurveSections[0].StartPoint.LeftTangent);
            segment.PreviousSegment.Curve.UpdateCurveSections();
            _groundSpawner.ApplyCurveToSegment(segment.PreviousSegment, segment.PreviousSegment.Curve);
        }

        _groundSpawner.ApplyCurveToSegment(segment, segment.Curve);

        segment.UpdateShadow();

        if (segment.IsStart)
        {
            SetStartPoint(segment, 1);
        }

        if(segment.IsFinish)
        {
            SetFinishLine(segment);
        }

    }

    public void ResetSegment(GroundSegment segment)
    {
        segment.Curve = CurveFactory.DefaultCurve(segment.PrevTangent);
        RefreshCurve(segment);
        RecalculateSegments(segment);
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

    public void SetFinishLine(GroundSegment segment)
    {
        Undo.RegisterFullObjectHierarchyUndo(segment.gameObject, "Set finish line");
        segment.IsFinish = true;
        segment.SetLowPoint(2);

        var flagPosition = segment.transform.TransformPoint(segment.Curve.GetPoint(2).Position + new Vector3(50, 0));
        var backstopPosition = segment.transform.TransformPoint(segment.Curve.GetPoint(3).Position);

        _groundSpawner.SetFinishLine(flagPosition, backstopPosition);
    }

    #endregion
}
