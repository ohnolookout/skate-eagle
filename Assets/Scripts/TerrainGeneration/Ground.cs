using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using static UnityEngine.Rendering.HableCurve;

public class Ground : MonoBehaviour
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
    #endregion

}