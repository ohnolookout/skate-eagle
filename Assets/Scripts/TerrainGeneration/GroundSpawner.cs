using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

//Builds ground, ground segments, and start/finish objects
[ExecuteAlways]
public class GroundSpawner : MonoBehaviour
{
    private GroundManager _groundManager;
    [SerializeField] private GameObject _groundPrefab;
    [SerializeField] private GameObject _groundSegmentPrefab;
    [SerializeField] private GameObject _finishFlagPrefab;
    [SerializeField] private GameObject _backstopPrefab;
    private GameObject _finishFlag;
    private GameObject _backstop;
    private IGroundSegment _finishSegment;
    private IGroundSegment _startSegment;


    public Action<IGroundSegment, Vector2> OnStartPointSet;
    public Action<IGroundSegment, Vector2> OnFinishPointSet;

    #region Monobehaviours
    private void Awake()
    {
        _groundManager = gameObject.GetComponentInParent<GroundManager>();
    }

    #endregion

    #region Add/Remove Segments
    public Ground AddGround()
    {
        var groundObj = Instantiate(_groundPrefab, _groundManager.transform);
        groundObj.name = "Ground " + (_groundManager.transform.childCount - 2);
        Undo.RegisterCreatedObjectUndo(groundObj, "Add Ground");
        return groundObj.GetComponent<Ground>();
    }

    public void RemoveGround(Ground ground)
    {
        Undo.DestroyObjectImmediate(ground.gameObject);
    }

    //Add segment to start at current endpoint
    public GroundSegment AddSegment(Ground ground, CurveDefinition curveDef, bool addToFront = false)
    {
        Undo.RegisterFullObjectHierarchyUndo(ground, "Add Segment");

        //Create new segment, set start point to end of current segment, and add to _segmentList
        var newSegment = Instantiate(_groundSegmentPrefab, ground.transform, true).GetComponent<GroundSegment>();
        newSegment.gameObject.name = "Segment " + ground.SegmentList.Count;
        Undo.RegisterCreatedObjectUndo(newSegment.gameObject, "Add Segment");
        
        GroundSegment? prevSegment = null;
        Vector2 startPoint = new(0, 0);
        if (!addToFront)
        {
            prevSegment = ground.SegmentList.Count == 0 ? null : ground.SegmentList[^1];
            startPoint = prevSegment == null ? new Vector2(0, 0) : prevSegment.EndPosition;
            ground.SegmentList.Add(newSegment);

        }
        else
        {
            if(ground.SegmentList.Count > 0)
            {
                ground.SegmentList[0].PreviousSegment = newSegment;
            }
            //Offset ground by x delta of new segment
            var xDelta = curveDef.TotalLength();
            var yDelta = curveDef.TotalClimb();
            startPoint = ground.SegmentList.Count == 0 ? new Vector2(0, 0) : ground.SegmentList[0].StartPosition;
            startPoint -= new Vector2(xDelta, yDelta);
            ground.SegmentList.Insert(0, newSegment);
        }

        newSegment.transform.position = startPoint;
        GenerateSegment(newSegment, ground, curveDef, prevSegment);

        //Deactivate segment. Reactivate if in editor mode.
        newSegment.gameObject.SetActive(false);
#if UNITY_EDITOR
        //Set all segments active if in editor mode to show generated level.
        if (!Application.isPlaying)
        {
            newSegment.gameObject.SetActive(true);
        }
#endif

        return newSegment;
    }

    public GroundSegment AddSegment(Ground ground)
    {
        return AddSegment(ground, new CurveDefinition());
    }

    public GroundSegment AddSegmentToFront(Ground ground, CurveDefinition curveDef = null)
    {
        if(curveDef == null)
        {
            curveDef = new();
        }
        var seg = AddSegment(ground, curveDef, true);

        RecalculateSegments(ground, 1);

        return seg;
    }

    public GroundSegment InsertSegment(Ground ground, CurveDefinition curveDef, int index)
    {
        if (index < 0 || index >= ground.SegmentList.Count)
        {
            return null;
        }
        Undo.RegisterFullObjectHierarchyUndo(ground, "Insert Segment");

        //Split segment list into two lists at index, remove all segments after index from original list
        var tempList = ground.SegmentList.GetRange(index, ground.SegmentList.Count - index);
        ground.SegmentList.RemoveRange(index, ground.SegmentList.Count - index);

        //Add segment using curveDef
        var newSegment = AddSegment(ground, curveDef);

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
        var segIndex =  ground.SegmentList.IndexOf(segment);

        if(segIndex >= ground.SegmentList.Count - 1)
        {
            return AddSegment(ground, copiedCurveDef);
        }

        return InsertSegment(ground, copiedCurveDef, segIndex);
    }

    public void RemoveSegment(Ground ground)
    {
        if (ground.SegmentList.Count == 0)
        {
            return;
        }

        RemoveSegment(ground.SegmentList[^1]);
    }

    public void RemoveSegment(GroundSegment segment)
    {
        var ground = segment.parentGround;

        Undo.RegisterFullObjectHierarchyUndo(ground, "Remove Segment");
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

        Undo.RegisterFullObjectHierarchyUndo(segment, "Remove Segment");
        Undo.DestroyObjectImmediate(segment.gameObject);

        RecalculateSegments(ground, index);

    }
    #endregion

    #region Build Segments

    #nullable enable
    public void GenerateSegment(GroundSegment segment, Ground parent, CurveDefinition curveDef, GroundSegment? previousSegment)
    {
        Undo.RegisterFullObjectHierarchyUndo(segment, "Generating segment");

        segment.parentGround = parent;
        segment.PreviousSegment = previousSegment;

        segment.Curve = new(curveDef, segment.PrevTangent);

        //Remove last collider point from previous segment if new segment is now last segment
        if (segment.IsLastSegment && previousSegment != null)
        {
            var prevCollider = previousSegment.Collider;
            Undo.RecordObject(prevCollider, "Remove collider end points");
            prevCollider.points = prevCollider.points.Take(prevCollider.pointCount - 2).ToArray();
        }

        ApplyCurveToSegment(segment, segment.Curve);


    }
#nullable disable

    public void RefreshCurve(GroundSegment segment)
    {
        Undo.RegisterFullObjectHierarchyUndo(segment, "Refreshing segment");

        segment.Curve = new(segment.Curve.curveDefinition, segment.PrevTangent);

        ApplyCurveToSegment(segment, segment.Curve);

    }

    public void ApplyCurveToSegment(GroundSegment segment, Curve curve)
    {
        //Set splines to default formatting
        GroundSegmentUtility.FormatSpline(segment.Spline, false);
        Undo.RegisterFullObjectHierarchyUndo(segment.EdgeShapeController.gameObject, "Set edge");
        GroundSegmentUtility.FormatSpline(segment.EdgeSpline, true);

        Undo.RegisterFullObjectHierarchyUndo(segment, "Generating segment");
        GroundSegmentUtility.GenerateSpline(segment.Spline, curve, segment.floorHeight);

        Undo.RegisterFullObjectHierarchyUndo(segment.EdgeShapeController.gameObject, "Set edge");
        GroundSegmentUtility.InsertCurveToOpenSpline(segment.EdgeSpline, curve);

        AddCollider(segment);

    }
    public void ResetSegment(GroundSegment segment)
    {
        segment.Curve.curveDefinition = new CurveDefinition();
        RefreshCurve(segment);
        RecalculateSegments(segment);
    }
    private EdgeCollider2D AddCollider(GroundSegment segment, float resolution = 10)
    {
        Undo.RegisterCompleteObjectUndo(segment.Collider, "Add Collider");
        CurveCollider.BuildSegmentCollider(segment, segment.ColliderMaterial);
        /*
        var firstPoint = GroundSegmentUtility.LastColliderPoint(segment);
        segment.Collider = CurveCollider.GenerateCollider(segment.Curve, segment.Collider, segment.ColliderMaterial, firstPoint, resolution);
        */
        return segment.Collider;
    }
    #endregion

    #region Adjust Ground
    //Recalculate segments beginning at startIndex
    //First segment to be recalculated also recalculates its curve.
    public void RecalculateSegments(Ground ground, int startIndex)
    {
        if(startIndex < 0 || startIndex >= ground.SegmentList.Count)
        {
            return;
        }
        Undo.RegisterCompleteObjectUndo(ground, "Recalculate Segments");
        //Copy remaining elements of segmentList to temp list, remove from segmentList
        var remainingSegments = ground.SegmentList.GetRange(startIndex, ground.SegmentList.Count - startIndex);

        for (int i = startIndex; i < ground.SegmentList.Count; i++)
        {
            //Set position of segment to end of previous segment or leave at current start position if first segment
            Vector3 startPosition = ground.SegmentList[i].PreviousSegment != null ? ground.SegmentList[i].PreviousSegment.EndPosition : ground.SegmentList[i].StartPosition;

            Undo.RegisterFullObjectHierarchyUndo(ground.SegmentList[i].gameObject, "Recalculate Segment");
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


    #endregion

    #region Start/Finish
    public void SetStartPoint(GroundSegment segment, int curvePointIndex)
    {
        _startSegment = segment;
        var startPoint = segment.transform.TransformPoint(segment.Curve.GetPoint(curvePointIndex).ControlPoint);
        OnStartPointSet?.Invoke(segment, startPoint);
    }

    public void SetFinishPoint(GroundSegment segment, int finishPointIndex)
    {
        //If finishSegment has already been assigned, make isFinish false on old segment and destroy finish objects
        if (_finishSegment != null)
        {
            _finishSegment.IsFinish = false;
            DestroyImmediate(_finishFlag);
            DestroyImmediate(_backstop);
        }

        _finishSegment = segment;
        segment.IsFinish = true;

        //Add finish flag to designated point in GroundSegment.        
        var finishPoint = segment.transform.TransformPoint(segment.Curve.GetPoint(finishPointIndex).ControlPoint);

        _finishFlag = Instantiate(_finishFlagPrefab, finishPoint, transform.rotation, segment.gameObject.transform);

        //Add backstop to endpoint of GroundSegment
        _backstop = Instantiate(_backstopPrefab, segment.EndPosition - new Vector3(75, 0), transform.rotation, segment.gameObject.transform);

        //Announce new finishPoint
        OnFinishPointSet?.Invoke(_finishSegment, finishPoint);

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
