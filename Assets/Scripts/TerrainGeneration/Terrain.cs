using UnityEngine;
using System.Collections.Generic;
using System;


public class Terrain : MonoBehaviour
{
    #region Declarations
    private List<IGroundSegment> _segmentList;
    private List<EdgeCollider2D> _colliderList;
    private List<PositionalEdgeCollider> _positionalColliderList = new();
    private PositionalList<PositionObject<Vector3>> _lowPointList, _highPointList;
    private float _minMaxBuffer = 100;
    private GameObject _finishFlag, _backstop;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    public GameObject FinishFlagPrefab, BackstopPrefab;

    public List<IGroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public List<EdgeCollider2D> ColliderList { get => _colliderList; set => _colliderList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public List<PositionalEdgeCollider> PositionalColliderList { get => _positionalColliderList; set => _positionalColliderList = value; }
    public PositionalList<PositionObject<Vector3>> LowPointList { get => _lowPointList; set => _lowPointList = value; }
    public PositionalList<PositionObject<Vector3>> HighPointList { get => _highPointList; set => _highPointList = value; }
    #endregion

    public IGroundSegment ActivateSegmentAtIndex(int index, bool activationStatus)
    {
        _segmentList[index].gameObject.SetActive(activationStatus);
        return _segmentList[index];
    }

    public GameObject InstantiateSegment()
    {
        return Instantiate(_segmentPrefab, transform, true);
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

    public Vector3 LastColliderPoint()
    {
        return _colliderList[^1].points[^1];
    }

    public void PopulateMinMaxLists()
    {
        List<PositionObject<Vector3>> lowPoints = new(), highPoints = new();
        foreach(var segment in _segmentList)
        {
            lowPoints.Add(new PositionObject<Vector3>(segment.Curve.Lowpoint, segment.Curve.Lowpoint));
            highPoints.Add(new PositionObject<Vector3>(segment.Curve.Highpoint, segment.Curve.Highpoint));
        }

        Transform camTransform = Camera.main.transform;
        _lowPointList = PositionalListFactory<PositionObject<Vector3>>.TransformTracker(lowPoints, camTransform, _minMaxBuffer, _minMaxBuffer);
        _highPointList = PositionalListFactory<PositionObject<Vector3>>.TransformTracker(highPoints, camTransform, _minMaxBuffer, _minMaxBuffer);
    }
}