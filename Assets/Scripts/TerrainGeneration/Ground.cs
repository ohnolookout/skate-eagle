using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using static UnityEngine.Rendering.HableCurve;

public class Ground : MonoBehaviour
{
    #region Declarations
    [SerializeField] private List<GroundSegment> _segmentList;
    private PositionalList<PositionObject<Vector3>> _lowPointList, _highPointList;
    private float _minMaxBuffer = 100;
    private GameObject _finishFlag, _backstop;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    public GameObject FinishFlagPrefab, BackstopPrefab;

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public PositionalList<PositionObject<Vector3>> LowPointList { get => _lowPointList; set => _lowPointList = value; }
    public PositionalList<PositionObject<Vector3>> HighPointList { get => _highPointList; set => _highPointList = value; }
    public CurvePoint StartPoint => _segmentList[0].Curve.StartPoint;
    public CurvePoint EndPoint => _segmentList[^1].Curve.EndPoint;
    #endregion

}