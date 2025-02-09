using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class Ground : MonoBehaviour
{
    #region Declarations
    [SerializeField] private List<GroundSegment> _segmentList;
    private PositionalList<PositionObject<Vector3>> _lowPointList, _highPointList;
    private float _minMaxBuffer = 100;
    private GameObject _finishFlag, _backstop;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    public GameObject FinishFlagPrefab, BackstopPrefab;
    [SerializeField] private CurvePoint _startPoint = new(new(0, 0));
    [SerializeField] private CurvePoint _endPoint = new(new(0, 0));

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public PositionalList<PositionObject<Vector3>> LowPointList { get => _lowPointList; set => _lowPointList = value; }
    public PositionalList<PositionObject<Vector3>> HighPointList { get => _highPointList; set => _highPointList = value; }
    public CurvePoint StartPoint { get => _startPoint; set => _startPoint = value; }
    public CurvePoint EndPoint { get => _endPoint; set => _endPoint = value; }
    #endregion


    
    #region Add/Remove Segments
    //Add segment to start at current endpoint
    public GroundSegment AddSegment(CurveDefinition curveDef)
    {
        Undo.RegisterFullObjectHierarchyUndo(this, "Add Segment");

        //Create new segment, set start point to end of current segment, and add to _segmentList
        var newSegment = Instantiate(_segmentPrefab, transform, true).GetComponent<GroundSegment>();

        var prevSegment = _segmentList.Count == 0 ? null : _segmentList[^1];
        Vector2 startPoint = prevSegment == null ? new Vector2(0, 0) : prevSegment.EndPosition;

        //Move segment to current endpoint, update endpoint, and add to segmentList
        newSegment.transform.position = startPoint;
        newSegment.Generate(this, curveDef, prevSegment);

        _segmentList.Add(newSegment);

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

    public GroundSegment InsertSegment(CurveDefinition curveDef, int index)
    {
        if (index < 0 || index >= _segmentList.Count)
        {
            return null;
        }
        Undo.RegisterFullObjectHierarchyUndo(this, "Insert Segment");

        //Split segment list into two lists at index, remove all segments after index from original list
        var tempList = _segmentList.GetRange(index, _segmentList.Count-index);
        _segmentList.RemoveRange(index, _segmentList.Count - index);

        //Add segment using curveDef
        var newSegment = AddSegment(curveDef);

        //Update previous segment of first segment in tempList
        if (tempList.Count > 0)
        {
            tempList[0].PreviousSegment = newSegment;
        }
        _segmentList.AddRange(tempList);

        //Recalculate segment positions after index
        RecalculateSegmentsFromIndex(index + 1);

        return newSegment;
    }

    public void RemoveSegment()
    {
        if (_segmentList.Count == 0)
        {
            return;
        }

        RemoveSegment(_segmentList[^1]);
    }

    public void RemoveSegment(GroundSegment segment)
    {
        Undo.RegisterFullObjectHierarchyUndo(this, "Remove Segment");
        var index = _segmentList.IndexOf(segment);
        _segmentList.Remove(segment);
        
        //Update previous segment of segment after removal
        if(index == 0) 
        {
            _segmentList[index].PreviousSegment = null;
        }
        else if(index < _segmentList.Count)
        {
            _segmentList[index].PreviousSegment = _segmentList[index - 1];
        }

        Undo.RegisterFullObjectHierarchyUndo(segment, "Remove Segment");
        Undo.DestroyObjectImmediate(segment.gameObject);

        RecalculateSegmentsFromIndex(index);

    }

    #endregion
    #region Adjust Segments

    //Recalculate segments beginning at startIndex
    //First segment to be recalculated also recalculates its curve.
    private void RecalculateSegmentsFromIndex(int startIndex)
    {
        Undo.RegisterCompleteObjectUndo(this, "Recalculate Segments");
        //Copy remaining elements of segmentList to temp list, remove from segmentList
        var remainingSegments = _segmentList.GetRange(startIndex, _segmentList.Count - startIndex);

        for (int i = startIndex; i < _segmentList.Count; i++)
        {
            Vector3 endPosition = _segmentList[i].PreviousSegment != null ? _segmentList[i].PreviousSegment.EndPosition : Vector3.zero;

            Undo.RegisterFullObjectHierarchyUndo(_segmentList[i].gameObject, "Recalculate Segment");
            _segmentList[i].gameObject.transform.position = endPosition;

            if(i == startIndex)
            {
                _segmentList[i].RefreshCurve();
            }
        }
    }

    public void RecalculateSegmentsFromSegment(GroundSegment segment)
    {
        int index = _segmentList.IndexOf(segment);
        if (index == -1)
        {
            return;
        }
        RecalculateSegmentsFromIndex(index + 1);
    }


    #endregion

    #region Build Segments
    public void PopulateMinMaxLists()
    {
        List<PositionObject<Vector3>> lowPoints = new(), highPoints = new();
        foreach (var segment in _segmentList)
        {
            var lowPoint = segment.gameObject.transform.TransformPoint(segment.Curve.Lowpoint);
            var highPoint = segment.gameObject.transform.TransformPoint(segment.Curve.Highpoint);
            lowPoints.Add(new PositionObject<Vector3>(lowPoint, lowPoint));
            highPoints.Add(new PositionObject<Vector3>(highPoint, highPoint));
        }

        Transform camTransform = Camera.main.transform;
        _lowPointList = PositionalListFactory<PositionObject<Vector3>>.TransformTracker(lowPoints, camTransform, _minMaxBuffer, _minMaxBuffer);
        _highPointList = PositionalListFactory<PositionObject<Vector3>>.TransformTracker(highPoints, camTransform, _minMaxBuffer, _minMaxBuffer);
    }

    public void ResetSegmentList()
    {
        Undo.RegisterFullObjectHierarchyUndo(this, "Reset Segment List");
        while (transform.childCount > 0)
        {
            Undo.RegisterFullObjectHierarchyUndo(transform.GetChild(0).gameObject, "Destroy Segment");
            Undo.DestroyObjectImmediate(transform.GetChild(0).gameObject);
        }
        _segmentList.Clear();
    }
    #endregion
}