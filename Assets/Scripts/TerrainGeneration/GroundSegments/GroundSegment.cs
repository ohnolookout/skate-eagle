using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Reflection;

[ExecuteAlways]
public class GroundSegment : MonoBehaviour, IGroundSegment, IDoublePosition
{
    private Curve _curve;
    private ShadowCaster2D _shadowCaster;
    public SpriteShape spriteShape;
    public SpriteShapeController shapeController;
    private Spline _spline;
    private List<Vector2> _unoffsetPoints;
    private int _floorHeight = 100;
    private int _containmentBuffer = 20;


    void Awake()
    {
        _spline = shapeController.spline;
        _shadowCaster = GetComponent<ShadowCaster2D>();
        FormatSpline(_spline);
    }

    void Start()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_curve.Lowpoint, 1000);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_curve.Highpoint, 1);
    }

    public void ApplyCurve(Curve curve)
    {
        this._curve = curve;
        GenerateSpline(_spline, curve, _floorHeight);
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

    public Curve Curve { get => _curve; }
    public Spline Spline { get => _spline; }
    public Vector2 StartPoint { get => _curve.StartPoint.ControlPoint; }
    public Vector2 EndPoint { get => _curve.EndPoint.ControlPoint; }
    public Vector3 StartPosition { get => _curve.StartPoint.ControlPoint; }
    public Vector3 EndPosition { get => _curve.EndPoint.ControlPoint; }
    public Vector3 Position { get => transform.position; }
    public List<Vector2> UnoffsetPoints { get => _unoffsetPoints; }
    public CurveType Type { get => _curve.Type; }
    public ShadowCaster2D ShadowCaster { get => _shadowCaster; set => _shadowCaster = value; }
    public new GameObject gameObject { get => transform.gameObject; }



}
