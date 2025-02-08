using UnityEngine;
using System.Collections.Generic;

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
        //Create new segment, set start point to end of current segment, and add to _segmentList
        var newSegment = Instantiate(_segmentPrefab, transform, true).GetComponent<GroundSegment>();
        var prevSegment = _segmentList.Count == 0 ? null : _segmentList[^1];
        Vector2 startPoint = prevSegment == null ? new Vector2(0, 0) : prevSegment.EndPositionAsWorldPoint();

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
        RemoveSegment(_segmentList.Count - 1);
    }

    public void RemoveSegment(int index)
    {
        if (index < 0 || index >= _segmentList.Count)
        {
            return;
        }

        var segmentObj = _segmentList[index].gameObject;
        _segmentList.RemoveAt(index);
        DestroyImmediate(segmentObj);
        
        RecalculateSegmentsFromIndex(index);
    }
    public void RemoveSegment(GroundSegment segment)
    {
        var index = _segmentList.IndexOf(segment);
        _segmentList.Remove(segment);
        
        if(index == 0) 
        {
            _segmentList[index].PreviousSegment = null;
        }
        else
        {
            _segmentList[index].PreviousSegment = _segmentList[index - 1];
        }

        DestroyImmediate(segment.gameObject);

        RecalculateSegmentsFromIndex(index);
    }

    #endregion
    /*
    #region Set End Point

    //Sets endpoint to the endpoint of given segment index and return segment
    private GroundSegment SetEndPointToIndex(int index)
    {
        if (index < 0 || index >= _segmentList.Count)
        {
            throw new Exception("Index out of range");
        }

        var segment = _segmentList[index]; 
        _endPoint = segment.Curve.EndPoint;
        _endPoint.ControlPoint = segment.gameObject.transform.TransformPoint(_endPoint.ControlPoint);
        return segment;        
    }

    //Sets endpoint to segment preceding given index. If index is 0, sets endpoint to default curvepoint at (0, 0)

    private GroundSegment SetEndPointToPreviousSegment(int index)
    {
        if (index < 0 || index >= _segmentList.Count + 1)
        {
            throw new Exception("Index out of range");
        }

        if(index == 0)
        {
            _endPoint = new CurvePoint(new(0, 0));
            return null;
        }

        return SetEndPointToIndex(index - 1);
    }

    private GroundSegment SetEndPointToLastSegment()
    {
        return SetEndPointToIndex(_segmentList.Count - 1);
    }

    #endregion
    */
    #region Adjust Segments

    //Recalculate segments beginning at startIndex
    //First segment to be recalculated also recalculates its curve.
    private void RecalculateSegmentsFromIndex(int startIndex)
    {

        //Copy remaining elements of segmentList to temp list, remove from segmentList
        var remainingSegments = _segmentList.GetRange(startIndex, _segmentList.Count - startIndex);

        for (int i = startIndex; i < _segmentList.Count; i++)
        {
            _segmentList[i].gameObject.transform.position = _segmentList[i].PreviousSegment.EndPositionAsWorldPoint();

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
        while(transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        _segmentList.Clear();
        _endPoint = new CurvePoint(new(0, 0));
    }
    #endregion
}