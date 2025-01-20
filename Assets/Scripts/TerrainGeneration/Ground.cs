using UnityEngine;
using System.Collections.Generic;
using System;


public class Ground : MonoBehaviour
{
    #region Declarations
    private List<IGroundSegment> _segmentList;
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

    public IGroundSegment ActivateSegmentAtIndex(int index, bool activationStatus)
    {
        _segmentList[index].gameObject.SetActive(activationStatus);
        return _segmentList[index];
    }

    public GameObject InstantiateSegment()
    {
        var segment = Instantiate(_segmentPrefab, transform, true);
        return segment;
    }

    public void InstantiateFinish(Vector3 finishLinePoint, Vector3 backstopPoint)
    {
        _finishFlag = Instantiate(FinishFlagPrefab, finishLinePoint, transform.rotation, transform);
        _finishFlag.SetActive(false);
        _backstop = Instantiate(BackstopPrefab, backstopPoint - new Vector3(75, 0), transform.rotation, transform);
        _backstop.SetActive(false);
    }

    public void ActivateFinishObjects()
    {
        _finishFlag.SetActive(true);
        _backstop.SetActive(true);
    }

    //Need to modfy to return point relative to a given segment.
    public Vector3? LastColliderPoint(IGroundSegment currentSegment, IGroundSegment prevSegment = null)
    {
        if(prevSegment == null)
        {
            return null;
        }
        //May need to transform point
        var worldPoint = prevSegment.gameObject.transform.TransformPoint(prevSegment.Collider.points[^1]);
        return currentSegment.gameObject.transform.InverseTransformPoint(worldPoint);
    }

    public void PopulateMinMaxLists()
    {
        List<PositionObject<Vector3>> lowPoints = new(), highPoints = new();
        foreach(var segment in _segmentList)
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

    //Add segment to start at current endpoint
    public IGroundSegment AddSegment(Curve curve)
    {
        //Create new segment, set to end of current segment, and add to _segmentList
        var newSegment = Instantiate(_segmentPrefab, transform, true).GetComponent<GroundSegment>();
        newSegment.transform.position = _endPoint.ControlPoint;
        newSegment.ApplyCurve(curve);

        //Update endpoint
        _endPoint.ControlPoint += newSegment.Curve.EndPoint.ControlPoint;
        _endPoint.LeftTangent = newSegment.Curve.EndPoint.LeftTangent;
        _endPoint.RightTangent = newSegment.Curve.EndPoint.RightTangent;

        //Add collider
        if (_segmentList.Count == 0)
        {
            AddColliderToSegment(newSegment, LastColliderPoint(newSegment));
        }
        else
        {
            AddColliderToSegment(newSegment, LastColliderPoint(newSegment, _segmentList[^1]));
        }

        //Deactivate segment. Reactivate if in editor mode.
        _segmentList.Add(newSegment);
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

    //Create collider object on groundsegment, set inactive, and return collider
    public EdgeCollider2D AddColliderToSegment(IGroundSegment segment, Vector3? firstPoint, float resolution = 10)
    {
        segment.Collider = CurveCollider.GenerateCollider(segment.Curve, segment.Collider, _colliderMaterial, firstPoint, resolution);
        return segment.Collider;
    }

    public IGroundSegment AddFinish(CurveDefinition curveDef, int finishIndex, GroundManager manager)
    {
        var finishSegment = AddSegment(curveDef);

        return finishSegment;
        
    }

    public IGroundSegment AddStart(CurveDefinition curveDef, int startIndex)
    {
        var startSegment = AddSegment(curveDef);

        return startSegment;
    }
}