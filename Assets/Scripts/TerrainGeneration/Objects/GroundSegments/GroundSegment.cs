using UnityEngine;
using UnityEngine.U2D;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

//[ExecuteAlways]
[Serializable]
public class GroundSegment : MonoBehaviour
{
    #region Declarations
    //public Curve curve;
    [SerializeField] private SpriteShapeController _fillShapeController, _edgeShapeController;
    [SerializeField] private ShadowCaster2D _shadowCaster;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private EdgeCollider2D _collider;
    [SerializeField] private EdgeCollider2D _bottomCollider;
    [SerializeField] private int _leftFloorHeight = 100;
    [SerializeField] private int _leftFloorAngle = 0;
    [SerializeField] private int _rightFloorHeight = 100;
    [SerializeField] private int _rightFloorAngle = 0;
    private int _startPointIndex;
    private int _endPointIndex;
    private int _containmentBuffer = 20;
    private LinkedCameraTarget _startTarget = null;
    public Ground parentGround;

    public static Action<GroundSegment> OnSegmentBecomeVisible { get; set; }
    public static Action<GroundSegment> OnSegmentBecomeInvisible { get; set; }
    public Spline Spline { get => _fillShapeController.spline; }
    public Spline EdgeSpline { get => _edgeShapeController.spline; }
    public SpriteShapeController EdgeShapeController { get => _edgeShapeController; }
    public SpriteShapeController FillShapeController { get => _fillShapeController; }
    public Vector3 StartPosition => transform.TransformPoint(EdgeSpline.GetPosition(0));
    public Vector3 EndPosition => transform.TransformPoint(EdgeSpline.GetPosition(EdgeSpline.GetPointCount()-1));
    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public int LeftFloorHeight { get => _leftFloorHeight; set => _leftFloorHeight = value; }
    public int RightFloorHeight { get => _rightFloorHeight; set => _rightFloorHeight = value; }
    public int LeftFloorAngle { get => _leftFloorAngle; set => _leftFloorAngle = value; }
    public int RightFloorAngle { get => _rightFloorAngle; set => _rightFloorAngle = value; }
    public EdgeCollider2D Collider { get => _collider; set => _collider = value; }
    public EdgeCollider2D BottomCollider { get => _bottomCollider; set => _bottomCollider = value; }
    public new GameObject gameObject { get => transform.gameObject; }
    public int StartPointIndex { get => _startPointIndex; set => _startPointIndex = value; }
    public int EndPointIndex { get => _endPointIndex; set => _endPointIndex = value; }
    public LinkedCameraTarget StartTarget { get => _startTarget; set => _startTarget = value; }
    #endregion

    #region Monobehaviors
    void Awake()
    {
        _collider = gameObject.GetComponentInChildren<EdgeCollider2D>();
    }

    void Start()
    {
        ActivateShadow(parentGround.HasShadow);
    }

    void OnBecameVisible()
    {
        OnSegmentBecomeVisible?.Invoke(this);
    }

    void OnBecameInvisible()
    {
        OnSegmentBecomeInvisible?.Invoke(this);
    }

    public void ActivateShadow(bool doActivate)
    {
        _shadowCaster.enabled = doActivate;
    }

    #endregion

    #region Positional List Utilities
    public bool StartsAfterX(float startX)
    {
        return StartPosition.x >= startX;
    }

    public bool EndsBeforeX(float endX)
    {
        return EndPosition.x <= endX;
    }
    public bool ContainsX(float targetX)
    {
        return (targetX > StartPosition.x - _containmentBuffer && targetX < EndPosition.x + _containmentBuffer);
    }
    #endregion

}
