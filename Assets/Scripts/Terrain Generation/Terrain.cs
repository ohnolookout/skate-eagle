using UnityEngine;
using System.Collections.Generic;
using System;


public class Terrain : MonoBehaviour
{
    private List<GroundSegment> _segmentList;
    private List<EdgeCollider2D> _colliderList;
    private GameObject _finishFlag, _backstop;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] private GameObject _finishFlagPrefab, _backstopPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;

    public GroundSegment ActivateSegmentAtIndex(int index, bool activationStatus)
    {
        _segmentList[index].gameObject.SetActive(activationStatus);
        return _segmentList[index];
    }

    public GroundSegment InstantiateSegment()
    {
        return Instantiate(_segmentPrefab, transform, true).GetComponent<GroundSegment>();
    }

    public void InstantiateFinish(Vector3 finishLinePoint, Vector3 backstopPoint)
    {
        _finishFlag = Instantiate(_finishFlagPrefab, finishLinePoint, transform.rotation, transform);
        _finishFlag.SetActive(false);
        _backstop = Instantiate(_backstopPrefab, backstopPoint - new Vector3(75, 0), transform.rotation, transform);
    }

    public Vector3 LastColliderPoint()
    {
        return _colliderList[^1].points[^1];
    }

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public List<EdgeCollider2D> ColliderList { get => _colliderList; set => _colliderList = value; }
    public GameObject FinishFlag { get => _finishFlag; set => _finishFlag = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public GameObject Backstop { get => _backstop; set => _backstop = value; }
    public GameObject SegmentPrefab { get => _segmentPrefab; set => _segmentPrefab = value; }
    public GameObject FinishFlagPrefab { get => _finishFlagPrefab; set => _finishFlagPrefab = value; }
}