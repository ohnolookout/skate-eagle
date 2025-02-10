using UnityEngine;
using UnityEngine.U2D;
using System;
using UnityEditor;

[ExecuteAlways]
[Serializable]
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
    public Ground parentGround;
    [SerializeField] private GroundSegment _previousSegment;
    public GroundSegment PreviousSegment { get => _previousSegment; set => _previousSegment = value; }
    public Action<GroundSegment> OnActivate { get; set; }
    public Curve Curve { get => _curve; }
    public Spline Spline { get => _masterSpline; }
    public Vector3 StartPosition => transform.TransformPoint(_curve.StartPoint.ControlPoint);
    public Vector3 EndPosition => transform.TransformPoint(_curve.EndPoint.ControlPoint);

    public Vector3 PrevTangent => _previousSegment != null ? -_previousSegment.Curve.EndPoint.LeftTangent : Vector3.zero;
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
        Undo.RegisterFullObjectHierarchyUndo(this, "Generating segment");

        parentGround = parent;
        _previousSegment = previousSegment;

        _curve = new(curveDef, PrevTangent);

        ApplyCurve(_curve);
    }
#nullable disable

    public void RefreshCurve()
    {
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing segment");

        var prevTang = _previousSegment != null ? -_previousSegment.Curve.EndPoint.LeftTangent : Vector3.zero;
        _curve = new(_curve.curveDefinition, PrevTangent);

        ApplyCurve(_curve);

    }

    public void ApplyCurve(Curve curve)
    {
        //Set splines to default formatting
        GroundSegmentUtility.FormatSpline(_masterSpline, false);
        Undo.RegisterFullObjectHierarchyUndo(_edgeShapeController.gameObject, "Set edge");
        GroundSegmentUtility.FormatSpline(_edgeShapeController.spline, true);

        Undo.RegisterFullObjectHierarchyUndo(this, "Generating segment");
        GroundSegmentUtility.GenerateSpline(_masterSpline, curve, _floorHeight);

        Undo.RegisterFullObjectHierarchyUndo(_edgeShapeController.gameObject, "Set edge");
        GroundSegmentUtility.InsertCurveToOpenSpline(_edgeShapeController.spline, curve);

        AddCollider();

    }

    public void Reset()
    {
        _curve.curveDefinition = new CurveDefinition();
        RefreshCurve();
        parentGround.RecalculateSegments(this);
    }

    public void Delete()
    {
        parentGround.RemoveSegment(this);
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
        Undo.RegisterCompleteObjectUndo(_collider, "Add Collider");
        var firstPoint = GroundSegmentUtility.LastColliderPoint(this);
        _collider = CurveCollider.GenerateCollider(_curve, _collider, _colliderMaterial, firstPoint, resolution);
        return _collider;
    }

}
