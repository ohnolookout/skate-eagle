using UnityEngine;
using UnityEngine.U2D;
using System;
using UnityEditor;
using UnityEditor.Build;
using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections.Generic;

//[ExecuteAlways]
[Serializable]
public class GroundSegment : MonoBehaviour, IGroundSegment, ICameraTargetable
{
    #region Declarations
    public Curve curve;
    [SerializeField] private SpriteShapeController _fillShapeController, _edgeShapeController;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private EdgeCollider2D _collider;
    [SerializeField] private GameObject _highPoint;
    [SerializeField] private GameObject _lowPoint;
    [SerializeField] private List<CameraTarget> _cameraTargets;
    public int floorHeight = 100;
    private int _containmentBuffer = 20;
    private bool _isStart = false;
    private bool _isFinish = false;
    public bool doTarget = true;
    public Ground parentGround;
    [SerializeField] private GroundSegment _previousSegment;
    public GroundSegment PreviousSegment { get => _previousSegment; set => _previousSegment = value; }
    public static Action<bool> OnActivateFinish { get; set; }
    public static Action<GroundSegment> OnSegmentBecomeVisible { get; set; }
    public static Action<GroundSegment> OnSegmentBecomeInvisible { get; set; }
    public Curve Curve { get => curve; set => curve = value; }
    public Spline Spline { get => _fillShapeController.spline; }
    public Spline EdgeSpline { get => _edgeShapeController.spline; }
    public SpriteShapeController EdgeShapeController { get => _edgeShapeController; }
    public SpriteShapeController FillShapeController { get => _fillShapeController; }
    public Vector3 StartPosition => transform.TransformPoint(curve.StartPoint.ControlPoint);
    public Vector3 EndPosition => transform.TransformPoint(curve.EndPoint.ControlPoint);
    public Vector3 PrevTangent => _previousSegment != null ? -_previousSegment.Curve.EndPoint.LeftTangent : Vector3.zero;
    public Vector3 Position { get => transform.TransformPoint(curve.StartPoint.ControlPoint); set => transform.position = value; }
    public EdgeCollider2D Collider { get => _collider; set => _collider = value; }
    public new GameObject gameObject { get => transform.gameObject; }
    public bool IsFinish { get => _isFinish; set => _isFinish = value; }
    public bool IsStart { get => _isStart; set => _isStart = value; }
    public bool DoTarget { get => doTarget; }
    public bool IsFirstSegment => parentGround.SegmentList[0] == this;
    public bool IsLastSegment => parentGround.SegmentList[^1] == this;
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; }
    public Transform HighPoint => _highPoint.transform;
    public Transform LowPoint => _lowPoint.transform;
    public CameraTarget LowPointTarget => _cameraTargets[0];
    public CameraTarget HighPointTarget => _cameraTargets[1];
    public List<CameraTarget> CameraTargets => _cameraTargets;
    #endregion

    #region Monobehaviors
    void Awake()
    {
        _collider = gameObject.GetComponentInChildren<EdgeCollider2D>();
        var lowPointTarget = CameraTargetUtility.GetTarget(CameraTargetType.GroundSegmentLowPoint, _lowPoint.transform);
        var highPointTarget = CameraTargetUtility.GetTarget(CameraTargetType.GroundSegmentHighPoint, _highPoint.transform);
        _cameraTargets = new List<CameraTarget> { lowPointTarget, highPointTarget };
    }

    void Start()
    {
    }

    void OnBecameVisible()
    {
        if (_isFinish)
        {
            OnActivateFinish?.Invoke(true);
        }
        OnSegmentBecomeVisible?.Invoke(this);
    }

    void OnBecameInvisible()
    {
        if (_isFinish)
        {
            OnActivateFinish?.Invoke(false);
        }
        OnSegmentBecomeInvisible?.Invoke(this);
    }

    #endregion

    #region Positional List Utilities
    public bool StartsAfterX(float startX)
    {
        return curve.StartPoint.ControlPoint.x >= startX;
    }

    public bool EndsBeforeX(float endX)
    {
        return curve.EndPoint.ControlPoint.x <= endX;
    }
    public bool ContainsX(float targetX)
    {
        return (targetX > curve.StartPoint.ControlPoint.x - _containmentBuffer && targetX < curve.EndPoint.ControlPoint.x + _containmentBuffer);
    }
    #endregion

    #region Collider Utilities
    public Vector3 FirstColliderPoint()
    {
        if (_previousSegment == null)
        {
            return curve.StartPoint.ControlPoint;
        }

        var worldPoint = _previousSegment.transform.TransformPoint(_previousSegment.Collider.points[^1]);
        return transform.InverseTransformPoint(worldPoint);
    }
    #endregion

    #region High/LowPoints
    public void UpdateHighLowTransforms()
    {
        _highPoint.transform.position = curve.HighPoint + transform.position;
        _lowPoint.transform.position = curve.LowPoint + transform.position;
    }

    public void SetLowPoint(int index)
    {
        curve.LowPoint = curve.CurvePoints[index].ControlPoint;
        _lowPoint.transform.position = curve.LowPoint + transform.position;
    }

    public void SetHighPoint(int index)
    {
        if(index >= curve.CurvePoints.Count)
        {
            index = curve.CurvePoints.Count - 1;
        }

        curve.HighPoint = curve.CurvePoints[index].ControlPoint;
        _highPoint.transform.position = curve.HighPoint + transform.position;
    }

    public void RecalculateDefaultHighLowPoints()
    {
        curve.RecalculateDefaultHighLowPoints();
        UpdateHighLowTransforms();
    }
    #endregion
}
