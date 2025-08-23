using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

public enum FloorType
{
    Flat,
    Slanted,
    Segmented
}

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
    [SerializeField] private List<CurvePoint> _curvePoints = new();
    private FloorType _floorType = FloorType.Flat;
    private int _startFloorHeight = 100;
    private int _startFloorAngle = 0;
    private int _endFloorHeight = 100;
    private int _endFloorAngle = 0;
    public GameObject curvePointContainer;
    public int lastCPObjCount = 0;

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public CurvePoint StartPoint => CurvePoints[0];
    public CurvePoint EndPoint => CurvePoints[^1];
    public bool IsFloating { get => _isFloating; set => _isFloating = value; }
    public bool IsInverted { get => _isInverted; set => _isInverted = value; }
    public bool HasShadow { get => _hasShadow; set => _hasShadow = value; }
    public FloorType FloorType { get => _floorType; set => _floorType = value; }
    public int StartFloorHeight { get => _startFloorHeight; set => _startFloorHeight = value; }
    public int StartFloorAngle { get => _startFloorAngle; set => _startFloorAngle = value; }
    public int EndFloorHeight { get => _endFloorHeight; set => _endFloorHeight = value; }
    public int EndFloorAngle { get => _endFloorAngle; set => _endFloorAngle = value; }
    public GroundSegment LastSegment => _segmentList.Count > 0 ? _segmentList[^1] : null;
    public List<CurvePoint> CurvePoints => _curvePoints;
    public CurvePointEditObject[] CurvePointObjects => curvePointContainer.GetComponentsInChildren<CurvePointEditObject>();
    public GameObject GameObject => gameObject;
    #endregion

    public IDeserializable Serialize()
    {
        return new SerializedGround(this);
    }
#if UNITY_EDITOR
    #region Curve Points
    public CurvePointEditObject SetCurvePoint(CurvePoint curvePoint, int index = -1)
    {

        var pointObject = Instantiate(_curvePointEditObjectPrefab, curvePointContainer.transform).GetComponent<CurvePointEditObject>();
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

    #endregion

    #region Refresh Utilities
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

    public void Refresh()
    {
        Refresh(FindFirstObjectByType<GroundManager>());
    }

    public void Refresh(GroundManager groundManager)
    {
        _curvePoints = _curvePoints.Where( cp => cp.Object != null).ToList();
        var serializedGround = (SerializedGround)Serialize();

        foreach (var seg in _segmentList)
        {
            DestroyImmediate(seg.gameObject);
        }

        _segmentList = new();

        serializedGround.DeserializeEditSegment(groundManager, this);
        lastCPObjCount = curvePointContainer.transform.childCount;
    }
    #endregion
#endif
}