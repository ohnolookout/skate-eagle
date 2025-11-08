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

public class Ground : MonoBehaviour, ISerializable, IObjectResync
{
    #region Declarations
    [SerializeField] private List<GroundSegment> _segmentList;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private bool _isInverted = false;
    [SerializeField] private bool _hasShadow = true;
    [SerializeField] private GameObject _curvePointEditObjectPrefab;    
    [SerializeField] private List<CurvePoint> _curvePoints = new();
    private List<LinkedCameraTarget> _lowTargets = new();
    private CurvePointEditObject _manualLeftTargetObj;
    private CurvePointEditObject _manualRightTargetObj;
    private LinkedCameraTarget _manualLeftCamTarget;
    private LinkedCameraTarget _manualRightCamTarget;
    private ResyncRef<CurvePointEditObject> _leftEndTargetObjRef = new();
    private ResyncRef<CurvePointEditObject> _rightEndTargetObjRef = new();
    private ResyncRef<LinkedCameraTarget> _leftEndCamTargetRef = new();
    private ResyncRef<LinkedCameraTarget> _rightEndCamTargetRef = new();
    private List<ResyncRef<CurvePointEditObject>> _zoomPointRefs = new();
    private List<LinkedHighPoint> _highTargets = new();
    private FloorType _floorType = FloorType.Flat;
    public GameObject curvePointContainer;
    public int lastCPObjCount = 0;
    public string UID { get; set; }

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public bool IsFloating => _floorType == FloorType.Floating;
    public bool IsInverted { get => _isInverted; set => _isInverted = value; }
    public bool HasShadow { get => _hasShadow; set => _hasShadow = value; }
    public FloorType FloorType { get => _floorType; set => _floorType = value; }
    public GroundSegment LastSegment => _segmentList.Count > 0 ? _segmentList[^1] : null;
    public List<CurvePoint> CurvePoints {  get => _curvePoints; set => _curvePoints = value; }
    public List<LinkedCameraTarget> LowTargets { get => _lowTargets; set => _lowTargets = value; }
    public List<LinkedHighPoint> HighTargets { get => _highTargets; set => _highTargets = value; }
    public CurvePointEditObject ManualLeftTargetObj 
    {
        get
        {
            return _leftEndTargetObjRef.Value;
            var checkVal = _leftEndTargetObjRef.Value;
            Debug.Assert(checkVal != null, "Left end curveobjectref not found for " + name);
            return _manualLeftTargetObj;
        }
        set
        {
            _leftEndTargetObjRef.Value = value;
            _manualLeftTargetObj = value;
        }
    }
    public CurvePointEditObject ManualRightTargetObj 
    {
        get
        {
            return _rightEndTargetObjRef.Value;
            var checkVal = _rightEndTargetObjRef.Value;
            Debug.Assert(checkVal != null, "Right end curveobjectref not found for " + name);
            return _manualRightTargetObj;
        }
        set
        {
            _rightEndTargetObjRef.Value = value;
            _manualRightTargetObj = value;
        }
    }
    public CurvePointEditObject[] CurvePointObjects => curvePointContainer.GetComponentsInChildren<CurvePointEditObject>();
    public GameObject GameObject => gameObject;
    public LinkedCameraTarget ManualLeftCamTarget {
        //get
        get
        {
            if (ManualLeftTargetObj != null)
            {
                _manualLeftCamTarget = ManualLeftTargetObj.LinkedCameraTarget;
                _leftEndCamTargetRef.Value = ManualLeftTargetObj.LinkedCameraTarget;
                return _leftEndCamTargetRef.Value;
                return _manualLeftCamTarget;
            }
            else
            {
                return _leftEndCamTargetRef.Value;
                return _manualLeftCamTarget;
            }
        }
        set
        {
            _manualLeftCamTarget = value;
            _leftEndCamTargetRef.Value = value;
        }
    }
    public LinkedCameraTarget ManualRightCamTarget
    {
        get
        {
            if (ManualRightTargetObj != null)
            {
                _manualRightCamTarget = ManualRightTargetObj.LinkedCameraTarget;
                _rightEndCamTargetRef.Value = ManualRightTargetObj.LinkedCameraTarget;
                return _rightEndCamTargetRef.Value;
                return _manualRightCamTarget;
            }
            else
            {
                return _rightEndCamTargetRef.Value;
                return _manualRightCamTarget;
            }
        }
        set
        {
            _rightEndCamTargetRef.Value = value;
            _manualRightCamTarget = value;
        }
    }
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

    public ResyncRef<CurvePointEditObject> LeftEndTargetObjRef { get => _leftEndTargetObjRef; set => _leftEndTargetObjRef = value; }
    public ResyncRef<CurvePointEditObject> RightEndTargetObjRef { get => _rightEndTargetObjRef; set => _rightEndTargetObjRef = value; }
    public ResyncRef<LinkedCameraTarget> LeftEndCamTargetRef { get => _leftEndCamTargetRef; set => _leftEndCamTargetRef = value; }
    public ResyncRef<LinkedCameraTarget> RightEndCamTargetRef { get => _rightEndCamTargetRef; set => _rightEndCamTargetRef = value; }
    public List<ResyncRef<CurvePointEditObject>> ZoomPointRefs { get => _zoomPointRefs; set => _zoomPointRefs = value; }
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

    #region Resync

    public List<ObjectResync> GetObjectResyncs()
    {

        return new();
        List<ObjectResync> resyncs = new();

        if (ManualLeftCamTarget != null)
        {
            var leftResync = new ObjectResync(ManualLeftCamTarget.serializedObjectLocation);
            leftResync.resyncFunc = (obj) =>
            {
                ManualLeftTargetObj = obj.GetComponent<CurvePointEditObject>();
            };

            resyncs.Add(leftResync);
        }

        if (ManualRightCamTarget != null)
        {
            var rightResync = new ObjectResync(ManualRightCamTarget.serializedObjectLocation);
            rightResync.resyncFunc = (obj) =>
            {
                ManualRightTargetObj = obj.GetComponent<CurvePointEditObject>();
            };
            resyncs.Add(rightResync);
        }

        return resyncs;
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
        _curvePoints = _curvePoints.Where( cp => cp.CPObject != null).ToList();
        _lowTargets = GetLowTargets();
        var serializedGround = (SerializedGround)Serialize();

        foreach (var seg in _segmentList)
        {
            DestroyImmediate(seg.gameObject);
        }

        _segmentList = new();

        serializedGround.DeserializeSegments(groundManager, this);
        lastCPObjCount = curvePointContainer.transform.childCount;
#endif
    }
    #endregion

    public List<LinkedCameraTarget> GetLowTargets()
    {
        var lowPoints = _curvePoints.Where(cp => cp.LinkedCameraTarget.doLowTarget).Select(cp => cp.LinkedCameraTarget).ToList();
        
        if(ManualLeftTargetObj != null)
        {
            lowPoints.Insert(0, ManualLeftCamTarget);
        }

        if(ManualRightTargetObj != null)
        {
            lowPoints.Add(ManualRightCamTarget);
        }

        _lowTargets = lowPoints;
        return lowPoints;
    }

    public List<LinkedCameraTarget> GetZoomPoints()
    {
        return _zoomPointRefs.Select(z => z.Value.LinkedCameraTarget).ToList();
    }
    public void RegisterResync()
    {
        LevelManager.ResyncHub.RegisterResync(this);
    }


}