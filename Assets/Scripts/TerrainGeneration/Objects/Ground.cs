using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class Ground : MonoBehaviour, ISerializable
{
    #region Declarations
    [SerializeField] private List<GroundSegment> _segmentList;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private bool _isFloating = false;
    [SerializeField] private bool _isInverted = false;
    [SerializeField] private bool _hasShadow = true;
    [SerializeField] private GameObject _curvePointEditObjectPrefab;
    [SerializeField] private GameObject _curvePointContainer;
    [SerializeField] private List<CurvePoint> _curvePoints = new();

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public CurvePoint StartPoint => CurvePoints[0];
    public CurvePoint EndPoint => CurvePoints[^1];
    public bool IsFloating { get => _isFloating; set => _isFloating = value; }
    public bool IsInverted { get => _isInverted; set => _isInverted = value; }
    public bool HasShadow { get => _hasShadow; set => _hasShadow = value; }
    public GroundSegment LastSegment => _segmentList.Count > 0 ? _segmentList[^1] : null;
    public List<CurvePoint> CurvePoints => _curvePoints;
    public CurvePointEditObject[] CurvePointObjects => _curvePointContainer.GetComponentsInChildren<CurvePointEditObject>();
    public GameObject GameObject => gameObject;
    #endregion

    public IDeserializable Serialize()
    {
        return new SerializedGround(this);
    }
#if UNITY_EDITOR
    public CurvePointEditObject SetCurvePoint(CurvePoint curvePoint, int index = -1)
    {

        var pointObject = Instantiate(_curvePointEditObjectPrefab, _curvePointContainer.transform).GetComponent<CurvePointEditObject>();
        pointObject.ParentGround = this;
        pointObject.name = curvePoint.name;
        pointObject.SetCurvePoint(curvePoint);
        
        if (index == -1)
        {
            CurvePoints.Add(curvePoint);
        } else
        {
            index = Math.Min(index, CurvePoints.Count);
            CurvePoints.Insert(index, curvePoint);
            pointObject.transform.SetSiblingIndex(index);
        }

        return pointObject;
    }

    public void Clear()
    {
        _curvePoints = new();

        foreach (var point in CurvePointObjects)
        {
            DestroyImmediate(point.gameObject);
        }

        foreach(var seg in _segmentList)
        {
            DestroyImmediate(seg.gameObject);
        }

        _segmentList = new();

        _isFloating = false;
        _isInverted = false;
        _hasShadow = true;
    }

    public void Refresh(GroundManager groundManager)
    {
        var serializedGround = (SerializedGround)Serialize();

        foreach (var seg in _segmentList)
        {
            DestroyImmediate(seg.gameObject);
        }

        _segmentList = new();

        serializedGround.DeserializeEditSegment(groundManager, this);
    }
#endif
}