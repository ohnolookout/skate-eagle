using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Ground : MonoBehaviour, ISerializable
{
    #region Declarations
    [SerializeField] private List<GroundSegment> _segmentList;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private bool _isFloating = false;
    [SerializeField] private bool _isInverted = false;
    [SerializeField] private bool _hasShadow = true;
    private List<CurvePointObject> _curvePointEditObjects = new();    
    [SerializeField] private GameObject _curvePointEditObjectPrefab;
    [SerializeField] private GameObject _curvePointParent;
    [SerializeField] private List<CurvePoint> _curvePoints = new();
    [SerializeField] private List<LinkedCameraTarget> _linkedCameraTargets = new();
    //Add dictionary that maps CurvePointObjects to Splinepoints

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public CurvePoint StartPoint => CurvePoints[0];
    public CurvePoint EndPoint => CurvePoints[^1];
    public bool IsFloating { get => _isFloating; set => _isFloating = value; }
    public bool IsInverted { get => _isInverted; set => _isInverted = value; }
    public bool HasShadow { get => _hasShadow; set => _hasShadow = value; }
    public GroundSegment LastSegment => _segmentList.Count > 0 ? _segmentList[^1] : null;
    public List<CurvePoint> CurvePoints => _curvePoints;
    public List<CurvePointObject> CurvePointObjects => _curvePointEditObjects;
    public List<LinkedCameraTarget> LinkedCameraTargets => _linkedCameraTargets;
    #endregion

    public IDeserializable Serialize()
    {
        return new SerializedGround(this);
    }
#if UNITY_EDITOR
    public void AddCurvePointEditObject(CurvePoint curvePoint) 
    {
        var pointObject = Instantiate(_curvePointEditObjectPrefab, _curvePointParent.transform).GetComponent<CurvePointObject>();        
        pointObject.ParentGround = this;
        pointObject.SetCurvePoint(curvePoint);
        _curvePointEditObjects.Add(pointObject);
        CurvePoints.Add(curvePoint);
        pointObject.OnCurvePointChange += OnCurvePointChanged;
    }

    private void OnCurvePointChanged(CurvePointObject point)
    {
        //Update corresponding splinepoints on curvePoint change
    }
#endif
}