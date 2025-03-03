using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//Handles all editor-specific functions for ground construction and destruction
[ExecuteAlways]
public class GroundEditManager : MonoBehaviour
{
    private GroundManager _groundManager;
    private GroundSpawner _groundSpawner;

    #region Monobehaviors
    private void Awake()
    {
        if(!Application.isEditor)
        {
            Destroy(this);
        }

        _groundManager = gameObject.GetComponentInParent<GroundManager>();
        _groundSpawner = _groundManager.groundSpawner;

        var levelDB = (LevelDatabase)AssetDatabase.LoadAssetAtPath("Assets/LevelDatabase/LevelDB.asset", typeof(LevelDatabase));
        var level = levelDB.GetLevelByUID(levelDB.lastLevelLoaded);
        if (level != null)
        {
            SerializeLevelUtility.DeserializeLevel(level, _groundManager);
        }
        else
        {
            _groundManager.ClearGround();
        }
    }
    #endregion

    #region Add
    public Ground AddGround()
    {
        return _groundSpawner.AddGround();
    }
    public GroundSegment AddSegment(Ground ground, CurveDefinition curveDef, bool addToFront = false)
    {
        return _groundSpawner.AddSegment(ground, curveDef, addToFront);
    }
    public GroundSegment AddSegment(Ground ground)
    {
        return _groundSpawner.AddSegment(ground, new CurveDefinition());
    }

    public GroundSegment AddSegmentToFront(Ground ground, CurveDefinition curveDef = null)
    {
        if (curveDef == null)
        {
            curveDef = new();
        }
        var seg = _groundSpawner.AddSegment(ground, curveDef, true);

        RecalculateSegments(ground, 1);

        return seg;
    }

    public GroundSegment InsertSegment(Ground ground, CurveDefinition curveDef, int index)
    {
#if UNITY_EDITOR
        if (index < 0 || index >= ground.SegmentList.Count)
        {
            return null;
        }
        Undo.RegisterFullObjectHierarchyUndo(ground, "Insert Segment");
#endif

        //Split segment list into two lists at index, remove all segments after index from original list
        var tempList = ground.SegmentList.GetRange(index, ground.SegmentList.Count - index);
        ground.SegmentList.RemoveRange(index, ground.SegmentList.Count - index);

        //Add segment using curveDef
        var newSegment = _groundSpawner.AddSegment(ground, curveDef);

        //Update previous segment of first segment in tempList
        if (tempList.Count > 0)
        {
            tempList[0].PreviousSegment = newSegment;
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
        var copiedCurveDef = DeepCopy.CopyCurveDefinition(segment.Curve.curveDefinition);
        var segIndex = ground.SegmentList.IndexOf(segment);

        if (segIndex >= ground.SegmentList.Count - 1)
        {
            return _groundSpawner.AddSegment(ground, copiedCurveDef);
        }

        return InsertSegment(ground, copiedCurveDef, segIndex);
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

            if (i == startIndex)
            {
                RefreshCurve(ground.SegmentList[i]);
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

    public void RefreshCurve(GroundSegment segment)
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(segment, "Refreshing segment");
#endif

        segment.Curve = new(segment.Curve.curveDefinition, segment.PrevTangent);

        _groundSpawner.ApplyCurveToSegment(segment, segment.Curve);

    }

    public void ResetSegment(GroundSegment segment)
    {
        segment.Curve.curveDefinition = new CurveDefinition();
        RefreshCurve(segment);
        RecalculateSegments(segment);
    }
    #endregion

    #region Start/Finish
    public Vector2 SetStartPoint(GroundSegment segment, int curvePointIndex)
    {
        return _groundSpawner.SetStartPoint(segment, curvePointIndex);
    }

    public Vector2 SetFinishPoint(GroundSegment segment, int curvePointIndex)
    {
        return _groundSpawner.SetFinishPoint(segment, curvePointIndex);
    }

    public CurveDefinition DefaultStart()
    {
        CurveSection firstSection = new(SectionType.Peak, 120, 0.5f, 0, 0);
        CurveSection secondSection = new(SectionType.Valley, 45, 0.5f, 1.25f, -20);
        return new(new List<CurveSection> { firstSection, secondSection });
    }

    public CurveDefinition DefaultFinish()
    {
        CurveSection firstSection = new(SectionType.Valley, 50, 0.5f, 0.5f, -25);
        CurveSection secondSection = new(SectionType.Valley, 300, 0, 0, 0);
        return new(new List<CurveSection> { firstSection, secondSection });
    }
    #endregion
}
