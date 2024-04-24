using UnityEngine;
using UnityEngine.U2D;
using System;

[ExecuteAlways]
public class GroundSegment : MonoBehaviour, IGroundSegment
{
    #region Declarations
    private Curve _curve;
    [SerializeField] private SpriteShapeController _fillShapeController, _edgeShapeController;
    private Spline _masterSpline;
    private int _floorHeight = 100;
    private int _containmentBuffer = 20;
    private bool _isFinish = false;
    public Action<IGroundSegment> OnActivate { get; set; }
    public Curve Curve { get => _curve; }
    public Spline Spline { get => _masterSpline; }
    public Vector2 StartPoint { get => _curve.StartPoint.ControlPoint; }
    public Vector2 EndPoint { get => _curve.EndPoint.ControlPoint; }
    public Vector3 StartPosition => _curve.StartPoint.ControlPoint;
    public Vector3 EndPosition => _curve.EndPoint.ControlPoint;
    public Vector3 Position => _curve.StartPoint.ControlPoint;
    public CurveType Type { get => _curve.Type; }
    public new GameObject gameObject { get => transform.gameObject; }
    public bool IsFinish { get => _isFinish; set => _isFinish = value; }
    #endregion


    void Awake()
    {
        _masterSpline = _fillShapeController.spline;
        FormatSpline(_masterSpline);
    }

    void OnEnable()
    {
        if (_isFinish)
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

    public void ApplyCurve(Curve curve)
    {
        _curve = curve;
        GenerateSpline(_masterSpline, curve, _floorHeight);
        InsertCurveToOpenSpline(_edgeShapeController.spline, curve);
    }

    public bool StartsAfterX(float startX)
    {
        return _curve.StartPoint.ControlPoint.x >= startX;
    }

    public bool EndsBeforeX(float endX)
    {
        return _curve.EndPoint.ControlPoint.x <= endX;
    }

    private static void GenerateSpline(Spline spline, Curve curve, int floorHeight)
    {
        InsertCurveToSpline(spline, curve, 1);
        UpdateCorners(spline, floorHeight);
    }

    private static void UpdateCorners(Spline spline, float lowerY)
    {
        UpdateRightCorners(spline, lowerY);
        UpdateLeftCorners(spline, lowerY);
    }

    private static void UpdateRightCorners(Spline spline, float lowerY)
    {
        //Reassigns the lower right corner (last index on the spline) to the same x as the preceding point and the y of the preceding point - the lowerBoundY buffer.
        int lastIndex = spline.GetPointCount() - 1;
        spline.SetPosition(lastIndex, new Vector3(spline.GetPosition(lastIndex - 1).x, spline.GetPosition(lastIndex - 1).y - lowerY));
        spline.SetTangentMode(lastIndex, ShapeTangentMode.Linear);
        spline.SetLeftTangent(lastIndex, new Vector3(-1, 0));
        spline.SetRightTangent(lastIndex, new Vector3(0, 1));
        //Resets the corner point's tangent mode in case it was changed.
        spline.SetTangentMode(lastIndex - 1, ShapeTangentMode.Broken);
        spline.SetRightTangent(lastIndex - 1, new Vector2(0, -1));
    }
    private static void UpdateLeftCorners(Spline spline, float lowerY)
    {

        spline.SetPosition(0, new Vector3(spline.GetPosition(1).x, spline.GetPosition(1).y - lowerY));
        spline.SetTangentMode(0, ShapeTangentMode.Linear);
        spline.SetLeftTangent(0, new Vector3(0, 1));
        spline.SetRightTangent(0, new Vector3(1, 0));
        spline.SetTangentMode(1, ShapeTangentMode.Broken);
        spline.SetLeftTangent(1, new Vector2(0, -1));
    }


    private static void InsertCurveToOpenSpline(Spline spline, Curve curve)
    {
        CopyCurvePointToSpline(spline, curve.GetPoint(0), 0);
        CopyCurvePointToSpline(spline, curve.GetPoint(1), 1);
        for (int i = 2; i < curve.Count; i++)
        {
            InsertCurvePointToSpline(spline, curve.GetPoint(i), i);
        }
    }

    private static void InsertCurveToSpline(Spline spline, Curve curve, int index) //Inserts curve into the spline beginning at the given index
    {
        for (int i = 0; i < curve.Count; i++)
        {
            InsertCurvePointToSpline(spline, curve.GetPoint(i), index);
            index++;
        }
        spline.SetTangentMode(index, ShapeTangentMode.Broken);
        spline.SetRightTangent(index, new Vector3(0, -1));

    }
    private static void InsertCurvePointToSpline(Spline spline, CurvePoint curvePoint, int index) //Inserts curvePoint at a given index
    {
        spline.InsertPointAt(index, curvePoint.ControlPoint);
        spline.SetTangentMode(index, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(index, curvePoint.LeftTangent);
        spline.SetRightTangent(index, curvePoint.RightTangent);
    }

    private static void CopyCurvePointToSpline(Spline spline, CurvePoint curvePoint, int index) //Inserts curvePoint at a given index
    {
        spline.SetPosition(index, curvePoint.ControlPoint);
        spline.SetTangentMode(index, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(index, curvePoint.LeftTangent);
        spline.SetRightTangent(index, curvePoint.RightTangent);
    }
    public bool ContainsX(float targetX)
    {
        return (targetX > _curve.StartPoint.ControlPoint.x - _containmentBuffer && targetX < _curve.EndPoint.ControlPoint.x + _containmentBuffer);
    }
    public static void FormatSpline(Spline spline)
    {
        spline.isOpenEnded = false;
        while (spline.GetPointCount() > 2)
        {
            spline.RemovePointAt(2);
        }
    }



}
