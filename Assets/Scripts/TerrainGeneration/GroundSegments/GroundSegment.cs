using UnityEngine;
using UnityEngine.U2D;
using System;
using UnityEditor;

[ExecuteAlways]
[Serializable]
public class GroundSegment : MonoBehaviour, IGroundSegment
{
    #region Declarations
    public Curve curve;
    [SerializeField] private SpriteShapeController _fillShapeController, _edgeShapeController;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private EdgeCollider2D _collider;
    public int floorHeight = 100;
    private int _containmentBuffer = 20;
    public bool isStart = false;
    public bool isFinish = false;
    public Ground parentGround;
    [SerializeField] private GroundSegment _previousSegment;
    public GroundSegment PreviousSegment { get => _previousSegment; set => _previousSegment = value; }
    public Action<GroundSegment> OnActivate { get; set; }
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
    public bool IsFinish { get => isFinish; set => isFinish = value; }
    public bool IsStart { get => isStart; set => isStart = value; }
    public bool IsFirstSegment => parentGround.SegmentList[0] == this;
    public bool IsLastSegment => parentGround.SegmentList[^1] == this;
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; }
    #endregion


    void Awake()
    {
        _collider = gameObject.GetComponentInChildren<EdgeCollider2D>();
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

    public Vector3 FirstColliderPoint()
    {
        if (_previousSegment == null)
        {
            return curve.StartPoint.ControlPoint;
        }

        var worldPoint = _previousSegment.transform.TransformPoint(_previousSegment.Collider.points[^1]);
        return transform.InverseTransformPoint(worldPoint);
    }

}
