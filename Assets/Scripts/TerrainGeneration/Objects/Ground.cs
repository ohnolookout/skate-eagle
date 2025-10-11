using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

public enum FloorType
{
    Flat,
    Slanted,
    Segmented,
    Floating
}

public class Ground : MonoBehaviour, ISerializable
{
    #region Declarations
    [SerializeField] private List<GroundSegment> _segmentList;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private bool _isInverted = false;
    [SerializeField] private bool _hasShadow = true;
    [SerializeField] private GameObject _curvePointEditObjectPrefab;    
    [SerializeField] private List<CurvePoint> _curvePoints = new();
    private List<CurvePoint> _lowPoints = new();
    [SerializeField] private List<CurvePoint> _zoomPoints = new();
    [SerializeField] private CurvePointEditObject _manualLeftCamTarget;
    [SerializeField] private CurvePointEditObject _manualRightCamTarget;
    private FloorType _floorType = FloorType.Flat;
    public GameObject curvePointContainer;
    public int lastCPObjCount = 0;

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public bool IsFloating => _floorType == FloorType.Floating;
    public bool IsInverted { get => _isInverted; set => _isInverted = value; }
    public bool HasShadow { get => _hasShadow; set => _hasShadow = value; }
    public FloorType FloorType { get => _floorType; set => _floorType = value; }
    public GroundSegment LastSegment => _segmentList.Count > 0 ? _segmentList[^1] : null;
    public List<CurvePoint> CurvePoints => _curvePoints;
    public List<CurvePoint> LowPoints { get => _lowPoints; set => _lowPoints = value; }
    public List<CurvePoint> ZoomPoints { get => _zoomPoints; set => _zoomPoints = value; }
    public CurvePointEditObject ManualLeftCamTarget { get => _manualLeftCamTarget; set => _manualLeftCamTarget = value; }
    public CurvePointEditObject ManualRightCamTarget { get => _manualRightCamTarget; set => _manualRightCamTarget = value; }
    public CurvePointEditObject[] CurvePointObjects => curvePointContainer.GetComponentsInChildren<CurvePointEditObject>();
    public GameObject GameObject => gameObject;
    public int StartFloorHeight
    {
        get
        {
            if (_curvePoints.Count > 0)
            {
                return _curvePoints[0].FloorHeight;
            }
            else
            {
                return 100;
            }
        }
        set
        {
            if (_curvePoints.Count > 0)
            {
                _curvePoints[0].FloorHeight = value;
            }
        }
    }
    public int StartFloorAngle
    {
        get
        {
            if (_curvePoints.Count > 0)
            {
                return _curvePoints[0].FloorAngle;
            }
            else
            {
                return 0;
            }
        }
        set
        {
            if (_curvePoints.Count > 0)
            {
                _curvePoints[0].FloorAngle = value;
            }
        }
    }

    public int EndFloorHeight
    {
        get
        {
            if (_curvePoints.Count > 1)
            {
                return _curvePoints[^1].FloorHeight;
            }
            else
            {
                return 100;
            }
        }
        set
        {
            if (_curvePoints.Count > 1)
            {
                _curvePoints[^1].FloorHeight = value;
            }
        }
    }
    public int EndFloorAngle
    {
        get
        {
            if (_curvePoints.Count > 1)
            {
                return _curvePoints[^1].FloorAngle;
            }
            else
            {
                return 0;
            }
        }
        set
        {
            if (_curvePoints.Count > 1)
            {
                _curvePoints[^1].FloorAngle = value;
            }
        }
    }
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

        if (index == -1)
        {
            CurvePoints.Add(curvePoint);
        }
        else
        {
            index = Math.Min(index, CurvePoints.Count);
            CurvePoints.Insert(index, curvePoint);
            pointObject.transform.SetSiblingIndex(index);
        }

        pointObject.SetCurvePoint(curvePoint);
        
        return pointObject;
    }

    #endregion

#endif
    #region Refresh Utilities
    public void Clear()
    {
#if UNITY_EDITOR

        _curvePoints = new();

        foreach (var point in CurvePointObjects)
        {
            DestroyImmediate(point.gameObject);
        }

        foreach(var seg in _segmentList)
        {
            DestroyImmediate(seg.gameObject);
        }

#endif
        _segmentList = new();

        _floorType = FloorType.Flat;
        _isInverted = false;
        _hasShadow = true;
    }

    public void Refresh()
    {
        Refresh(FindFirstObjectByType<GroundManager>());
    }

    public void Refresh(GroundManager groundManager)
    {
#if UNITY_EDITOR
        _curvePoints = _curvePoints.Where( cp => cp.Object != null).ToList();
        _lowPoints = GetLowPoints();
        var serializedGround = (SerializedGround)Serialize();

        foreach (var seg in _segmentList)
        {
            DestroyImmediate(seg.gameObject);
        }

        _segmentList = new();

        serializedGround.DeserializeRuntimeSegments(groundManager, this);
        lastCPObjCount = curvePointContainer.transform.childCount;
#endif
    }
    #endregion
    public LinkedCameraTarget FindNearestLeftLowPoint(Vector3 target, GroundSegment segment)
    {

        var startTarget = segment.StartTarget;

        return CameraTargetUtility.FindNearestLeftTarget(target.x, startTarget);

    }

    private List<CurvePoint> GetLowPoints()
    {
        return _curvePoints.Where(cp => cp.LinkedCameraTarget.doLowTarget).ToList();
    }
}