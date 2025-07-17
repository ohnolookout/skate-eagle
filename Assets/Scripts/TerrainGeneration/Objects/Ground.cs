using UnityEngine;
using System.Collections.Generic;

public class Ground : MonoBehaviour, ISerializable
{
    #region Declarations
    [SerializeField] private List<GroundSegment> _segmentList;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private bool _isFloating = false;
    private List<CurvePointEditObject> _curvePointEditObjects = new();    
    [SerializeField] private GameObject _curvePointEditObjectPrefab;
    [SerializeField] private GameObject _curvePointParent;
    [SerializeField] private List<CurvePoint> _curvePoints = new();

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public CurvePoint StartPoint => _segmentList[0].Curve.StartPoint;
    public CurvePoint EndPoint => _segmentList[^1].Curve.EndPoint;
    public bool IsFloating { get => _isFloating; set => _isFloating = value; }
    public GroundSegment LastSegment => _segmentList.Count > 0 ? _segmentList[^1] : null;
    public List<CurvePoint> CurvePoints => _curvePoints;
    #endregion

    public IDeserializable Serialize()
    {
        var name = gameObject.name;
        var position = transform.position;
        var segmentList = new List<IDeserializable>();
        foreach (GroundSegment segment in SegmentList)
        {
            segmentList.Add(segment.Serialize());
        }
        return new SerializedGround(name, position, segmentList);
    }

    public void AddCurvePointEditObject(CurvePoint curvePoint) 
    {
        var point = Instantiate(_curvePointEditObjectPrefab, _curvePointParent.transform).GetComponent<CurvePointEditObject>();
        point.groundTransform = transform;
        point.SetCurvePoint(curvePoint);

    }
}