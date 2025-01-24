using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityEngine.Rendering.HableCurve;


public class Ground : MonoBehaviour
{
    #region Declarations
    [SerializeField] private List<IGroundSegment> _segmentList;
    private PositionalList<PositionObject<Vector3>> _lowPointList, _highPointList;
    private float _minMaxBuffer = 100;
    private GameObject _finishFlag, _backstop;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    public GameObject FinishFlagPrefab, BackstopPrefab;
    private CurvePoint _startPoint = new(new(0, 0));
    private CurvePoint _endPoint = new(new(0, 0));

    public List<IGroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public PositionalList<PositionObject<Vector3>> LowPointList { get => _lowPointList; set => _lowPointList = value; }
    public PositionalList<PositionObject<Vector3>> HighPointList { get => _highPointList; set => _highPointList = value; }
    public CurvePoint StartPoint { get => _startPoint; set => _startPoint = value; }
    public CurvePoint EndPoint { get => _endPoint; set => _endPoint = value; }
    #endregion


    
    #region Add/Remove Segments
    //Add segment to start at current endpoint
    public IGroundSegment AddSegment(Curve curve)
    {
        //Create new segment, set to end of current segment, and add to _segmentList
        var newSegment = Instantiate(_segmentPrefab, transform, true).GetComponent<GroundSegment>();
        newSegment.AssignParent(this);
        newSegment.ApplyCurve(curve);

        //Move segment to current endpoint, update endpoint, and add to segmentList
        newSegment.transform.position = _endPoint.ControlPoint;

        //Add collider
        if (_segmentList.Count == 0)
        {
            AddColliderToSegment(newSegment, LastColliderPoint(newSegment));
        }
        else
        {
            AddColliderToSegment(newSegment, LastColliderPoint(newSegment, _segmentList[^1]));
        }

        _segmentList.Add(newSegment);
        SetEndPointToLastSegment();

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

    public IGroundSegment AddSegment(CurveDefinition curveDef)
    {
        var curve = CurveFactory.CurveFromDefinition(curveDef, _endPoint);
        return AddSegment(curve);
    }

    public IGroundSegment InsertSegment(CurveDefinition curveDef, int index)
    {
        if (index < 0 || index >= _segmentList.Count)
        {
            return null;
        }

        //Change endpoint to endpoint of segment before index
        SetEndPointToPreviousSegment(index);

        //Split segment list into two lists at index, remove all segments after index from original list
        var tempList = _segmentList.GetRange(index, _segmentList.Count-index);
        _segmentList.RemoveRange(index, _segmentList.Count - index);

        //Add segment using curveDef
        var newSegment = AddSegment(curveDef);

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

        var segment = _segmentList[index].gameObject;
        _segmentList.RemoveAt(index);
        DestroyImmediate(segment);
        
        //SetEndPointToIndex(_segmentList.Count - 1);
        RecalculateSegmentsFromIndex(index);
    }

    #endregion

    #region Set End Point

    //Sets endpoint to the endpoint of given segment index and return segment
    private IGroundSegment SetEndPointToIndex(int index)
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

    private IGroundSegment SetEndPointToPreviousSegment(int index)
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

    private IGroundSegment SetEndPointToLastSegment()
    {
        return SetEndPointToIndex(_segmentList.Count - 1);
    }

    #endregion

    #region Adjust Segments

    private void MoveSegmentToCurvePoint(IGroundSegment segment, CurvePoint startPoint, bool doUpdateCollider, IGroundSegment previousSegment = null)
    {
        segment.gameObject.transform.position = startPoint.ControlPoint;

        if (doUpdateCollider)
        {
            var segmentStartPoint = segment.Curve.curvePoints[0];
            segmentStartPoint.RightTangent = -startPoint.LeftTangent;

            AddColliderToSegment(segment, LastColliderPoint(segment, previousSegment));
        }
    }

    //
    private void RecalculateSegmentsFromIndex(int startIndex)
    {
        IGroundSegment previousSegment = SetEndPointToPreviousSegment(startIndex);

        //Copy remaining elements of segmentList to temp list, remove from segmentList
        var remainingSegments = _segmentList.GetRange(startIndex, _segmentList.Count - startIndex);
        //_segmentList.RemoveRange(index, _segmentList.Count - index);

        for (int i = startIndex; i < _segmentList.Count; i++)
        {
            MoveSegmentToCurvePoint(_segmentList[i], _endPoint, i == startIndex, previousSegment);
            SetEndPointToIndex(i);
        }
    }

    #endregion

    #region Build Segments

    //Create collider object on groundsegment, set inactive, and return collider
    public EdgeCollider2D AddColliderToSegment(IGroundSegment segment, Vector3? firstPoint, float resolution = 10)
    {
        segment.Collider = CurveCollider.GenerateCollider(segment.Curve, segment.Collider, _colliderMaterial, firstPoint, resolution);
        return segment.Collider;
    }

    //This is fucked up as of 1-22-25
    public Vector3? LastColliderPoint(IGroundSegment currentSegment, IGroundSegment prevSegment = null)
    {
        if (prevSegment == null)
        {
            Debug.Log("No previous segment found. Returning null.");
            return null;
        }
        var worldPoint = prevSegment.gameObject.transform.TransformPoint(prevSegment.Collider.points[^1]);
        return currentSegment.gameObject.transform.InverseTransformPoint(worldPoint);
    }

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
    #endregion
}