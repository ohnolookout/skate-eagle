using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using static UnityEngine.Rendering.HableCurve;

public class Ground : MonoBehaviour, ISerializable
{
    #region Declarations
    [SerializeField] private List<GroundSegment> _segmentList;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private bool _isFloating = false;
    public GameObject FinishFlagPrefab, BackstopPrefab;

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public CurvePoint StartPoint => _segmentList[0].Curve.StartPoint;
    public CurvePoint EndPoint => _segmentList[^1].Curve.EndPoint;
    public bool IsFloating { get => _isFloating; set => _isFloating = value; }
    public GroundSegment LastSegment => _segmentList.Count > 0 ? _segmentList[^1] : null;
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
}