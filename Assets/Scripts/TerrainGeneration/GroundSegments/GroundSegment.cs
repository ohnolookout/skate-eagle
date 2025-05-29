using UnityEngine;
using UnityEngine.U2D;
using System;
using UnityEditor;
using UnityEditor.Build;
using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.HableCurve;

//[ExecuteAlways]
[Serializable]
public class GroundSegment : MonoBehaviour, IGroundSegment, ICameraTargetable
{
    #region Declarations
    public List<GameObject> leftTargetObjects;
    public List<GameObject> rightTargetObjects;
    public Curve curve;
    [SerializeField] private SpriteShapeController _fillShapeController, _edgeShapeController;
    [SerializeField] private ShadowCaster2D _shadowCaster;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private EdgeCollider2D _collider;
    [SerializeField] private EdgeCollider2D _bottomCollider;
    [SerializeField] private GameObject _highPoint;
    [SerializeField] private GameObject _lowPoint;
    [SerializeField] private int _leftFloorHeight = 100;
    [SerializeField] private int _leftFloorAngle = 0;
    [SerializeField] private int _rightFloorHeight = 100;
    [SerializeField] private int _rightFloorAngle = 0;
    private int _containmentBuffer = 20;
    [SerializeField] private bool _isStart = false;
    [SerializeField] private bool _isFinish = false;
    [SerializeField] private bool _isFloating = false;
    [SerializeField] private bool _isInverted = false;
    [SerializeField] private bool _hasShadow = true;
    [SerializeField] private bool _doTarget = true;
    [SerializeField] private bool _useDefaultHighLowPoints = true;
    public Ground parentGround;
    [SerializeField] private GroundSegment _nextLeftSegment = null;
    [SerializeField] private GroundSegment _nextRightSegment = null;
    [SerializeField] private LinkedCameraTarget _linkedCameraTarget;
    public GroundSegment NextLeftSegment { get => _nextLeftSegment; set => _nextLeftSegment = value; }
    public GroundSegment NextRightSegment { get => _nextRightSegment; set => _nextRightSegment = value; }
    public static Action<GroundSegment> OnSegmentBecomeVisible { get; set; }
    public static Action<GroundSegment> OnSegmentBecomeInvisible { get; set; }
    public Curve Curve { get => curve; set => curve = value; }
    public Spline Spline { get => _fillShapeController.spline; }
    public Spline EdgeSpline { get => _edgeShapeController.spline; }
    public SpriteShapeController EdgeShapeController { get => _edgeShapeController; }
    public SpriteShapeController FillShapeController { get => _fillShapeController; }
    public Vector3 StartPosition => transform.TransformPoint(curve.StartPoint.Position);
    public Vector3 EndPosition => transform.TransformPoint(curve.EndPoint.Position);
    public Vector3 PrevTangent => _nextLeftSegment != null ? _nextLeftSegment.Curve.EndPoint.LeftTangent : new(1, 1);
    public Vector3 Position { get => transform.TransformPoint(curve.StartPoint.Position); set => transform.position = value; }
    public int LeftFloorHeight { get => _leftFloorHeight; set => _leftFloorHeight = value; }
    public int RightFloorHeight { get => _rightFloorHeight; set => _rightFloorHeight = value; }
    public int LeftFloorAngle { get => _leftFloorAngle; set => _leftFloorAngle = value; }
    public int RightFloorAngle { get => _rightFloorAngle; set => _rightFloorAngle = value; }
    public EdgeCollider2D Collider { get => _collider; set => _collider = value; }
    public EdgeCollider2D BottomCollider { get => _bottomCollider; set => _bottomCollider = value; }
    public new GameObject gameObject { get => transform.gameObject; }
    public bool IsFinish { get => _isFinish; set => _isFinish = value; }
    public bool IsStart { get => _isStart; set => _isStart = value; }
    public bool DoTarget { get => _doTarget; set => _doTarget = value; }
    public bool IsFirstSegment => parentGround.SegmentList[0] == this;
    public bool IsLastSegment => parentGround.SegmentList[^1] == this;
    public bool IsFloating { get => _isFloating; set => _isFloating = value; }
    public bool IsInverted { get => _isInverted; set => _isInverted = value; }
    public bool UseDefaultHighLowPoints { get => _useDefaultHighLowPoints; set => _useDefaultHighLowPoints = value; }
    public bool HasShadow { get => _hasShadow; set => _hasShadow = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; }
    public Transform HighPoint => _highPoint.transform;
    public Transform LowPoint => _lowPoint.transform;
    public List<GameObject> LeftTargetObjects { get => leftTargetObjects; set => leftTargetObjects = value; }
    public List<GameObject> RightTargetObjects { get => rightTargetObjects; set => rightTargetObjects = value; }
    public LinkedCameraTarget LinkedCameraTarget { get => _linkedCameraTarget; set => _linkedCameraTarget = value; }
    #endregion

    #region Monobehaviors
    void Awake()
    {
        _collider = gameObject.GetComponentInChildren<EdgeCollider2D>();
    }

    void Start()
    {
        UpdateShadow();
    }

    void OnBecameVisible()
    {
        OnSegmentBecomeVisible?.Invoke(this);
    }

    void OnBecameInvisible()
    {
        OnSegmentBecomeInvisible?.Invoke(this);
    }

    void OnDrawGizmosSelected()
    {
        var startPos = transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(curve.StartPoint.Position + startPos, 2f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(curve.EndPoint.Position + startPos, 2f);

        Gizmos.color = Color.blue;
        for(int i = 1; i < curve.CurvePoints.Count - 1; i++)
        {
            Gizmos.DrawSphere(curve.CurvePoints[i].Position + startPos, 2f);
        }

        Gizmos.color = Color.yellow;
        foreach(var point in curve.CurvePoints)
        {
            Gizmos.DrawSphere(point.LeftTangentPosition + startPos, 1f);
            Gizmos.DrawSphere(point.RightTangentPosition + startPos, 1f);
        }

        Gizmos.color = Color.white;
        foreach (var point in curve.CurvePoints)
        {
            Gizmos.DrawLine(point.Position + startPos, point.LeftTangentPosition + startPos);
            Gizmos.DrawLine(point.Position + startPos, point.RightTangentPosition + startPos);
        }

        LinkedCameraTarget.DrawTargets();

    }

    public void UpdateShadow()
    {
        _shadowCaster.enabled = _hasShadow;
    }

    #endregion

    #region Positional List Utilities
    public bool StartsAfterX(float startX)
    {
        return curve.StartPoint.Position.x >= startX;
    }

    public bool EndsBeforeX(float endX)
    {
        return curve.EndPoint.Position.x <= endX;
    }
    public bool ContainsX(float targetX)
    {
        return (targetX > curve.StartPoint.Position.x - _containmentBuffer && targetX < curve.EndPoint.Position.x + _containmentBuffer);
    }
    #endregion

    #region Collider Utilities
    public Vector3 FirstColliderPoint()
    {
        if (_nextLeftSegment == null)
        {
            return curve.StartPoint.Position;
        }

        var worldPoint = _nextLeftSegment.transform.TransformPoint(_nextLeftSegment.Collider.points[^1]);
        var colliderPoint = transform.InverseTransformPoint(worldPoint);

        return colliderPoint;
    }
    #endregion

    #region High/LowPoints
    public void PopulateDefaultTargets()
    {
        if (UseDefaultHighLowPoints)
        {
            curve.DoDefaultHighLowPoints();
            UpdateHighLowTransforms();
        }

        _linkedCameraTarget.LowTarget = CameraTargetUtility.GetTarget(CameraTargetType.GroundSegmentLowPoint, LowPoint.transform); 
        _linkedCameraTarget.HighTarget = CameraTargetUtility.GetTarget(CameraTargetType.GroundSegmentHighPoint, HighPoint.transform);

        if (_nextLeftSegment != null && !LeftTargetObjects.Contains(_nextLeftSegment.gameObject))
        {
            LeftTargetObjects.Add(_nextLeftSegment.gameObject);
        }

        if (_nextRightSegment != null && !RightTargetObjects.Contains(_nextRightSegment.gameObject))
        {
            RightTargetObjects.Add(_nextRightSegment.gameObject);
        }
    }

    public void UpdateHighLowTransforms()
    {
        _highPoint.transform.position = curve.HighPoint + transform.position;
        _lowPoint.transform.position = curve.LowPoint + transform.position;
    }

    public void SetLowPoint(int index)
    {
        curve.LowPoint = curve.CurvePoints[index].Position;
        _lowPoint.transform.position = curve.LowPoint + transform.position;
    }

    public void SetHighPoint(int index)
    {
        if(index >= curve.CurvePoints.Count)
        {
            index = curve.CurvePoints.Count - 1;
        }

        curve.HighPoint = curve.CurvePoints[index].Position;
        _highPoint.transform.position = curve.HighPoint + transform.position;
    }
    #endregion
}
