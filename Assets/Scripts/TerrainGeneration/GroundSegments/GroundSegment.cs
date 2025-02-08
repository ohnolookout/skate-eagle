using UnityEngine;
using UnityEngine.U2D;
using System;

[ExecuteAlways]
public class GroundSegment : MonoBehaviour, IGroundSegment
{
    #region Declarations
    public Curve _curve;
    [SerializeField] private SpriteShapeController _fillShapeController, _edgeShapeController;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private EdgeCollider2D _collider;
    private Spline _masterSpline;
    private int _floorHeight = 100;
    private int _containmentBuffer = 20;
    public bool isFinish = false;
    private Ground _parentGround;
#nullable enable
    private GroundSegment? _previousSegment;
    public GroundSegment? PreviousSegment { get => _previousSegment; set => _previousSegment = value; }
#nullable disable
    public Action<GroundSegment> OnActivate { get; set; }
    public Curve Curve { get => _curve; }
    public Spline Spline { get => _masterSpline; }
    public Vector3 StartPosition => transform.TransformPoint(_curve.StartPoint.ControlPoint);
    public Vector3 EndPosition => transform.TransformPoint(_curve.EndPoint.ControlPoint);
    public Vector3 Position { get => transform.TransformPoint(_curve.StartPoint.ControlPoint); set => transform.position = value; }
    public CurveType Type { get => _curve.Type; }
    public EdgeCollider2D Collider { get => _collider; set => _collider = value; }
    public new GameObject gameObject { get => transform.gameObject; }
    public bool IsFinish { get => isFinish; set => isFinish = value; }
    #endregion


    void Awake()
    {
        _collider = gameObject.GetComponentInChildren<EdgeCollider2D>();
        _masterSpline = _fillShapeController.spline;
    }

    void OnEnable()
    {
        if (isFinish)
        {
            OnActivate?.Invoke(this);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(_curve.Lowpoint, 1);
    }
#endif
#nullable enable
    public void Generate(Ground parent, CurveDefinition curveDef, GroundSegment? previousSegment)
    {
        _parentGround = parent;
        _previousSegment = previousSegment;
        var prevTang = _previousSegment != null ? -_previousSegment.Curve.EndPoint.LeftTangent : Vector3.zero;
        _curve = CurveFactory.CurveFromDefinition(curveDef, prevTang);

        GroundSegmentUtility.FormatSpline(_masterSpline, false);

        GroundSegmentUtility.GenerateSpline(_masterSpline, _curve, _floorHeight);
        GroundSegmentUtility.InsertCurveToOpenSpline(_edgeShapeController.spline, _curve);
        AddCollider();
    }
#nullable disable

    public void RefreshCurve()
    {
        var prevTang = _previousSegment != null ? -_previousSegment.Curve.EndPoint.LeftTangent : Vector3.zero;
        _curve.Refresh(prevTang);

        GroundSegmentUtility.FormatSpline(_masterSpline, false);
        GroundSegmentUtility.FormatSpline(_edgeShapeController.spline, true);

        GroundSegmentUtility.GenerateSpline(_masterSpline, _curve, _floorHeight);
        GroundSegmentUtility.InsertCurveToOpenSpline(_edgeShapeController.spline, _curve);
        AddCollider();
    }

    public void Delete()
    {
        _parentGround.RemoveSegment(this);
    }

    public void TriggerGroundRecalculation()
    {
        _parentGround.RecalculateSegmentsFromSegment(this);
    }

    public bool StartsAfterX(float startX)
    {
        return _curve.StartPoint.ControlPoint.x >= startX;
    }

    public bool EndsBeforeX(float endX)
    {
        return _curve.EndPoint.ControlPoint.x <= endX;
    }
    public bool ContainsX(float targetX)
    {
        return (targetX > _curve.StartPoint.ControlPoint.x - _containmentBuffer && targetX < _curve.EndPoint.ControlPoint.x + _containmentBuffer);
    }
    private EdgeCollider2D AddCollider(float resolution = 10)
    {
        var firstPoint = GroundSegmentUtility.LastColliderPoint(this);
        _collider = CurveCollider.GenerateCollider(_curve, _collider, _colliderMaterial, firstPoint, resolution);
        return _collider;
    }

    public Vector2 EndPositionAsWorldPoint()
    {
        return transform.TransformPoint(_curve.EndPoint.ControlPoint);
    }

}
