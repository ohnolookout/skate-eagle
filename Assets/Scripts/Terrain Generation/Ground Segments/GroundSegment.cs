using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Reflection;

[ExecuteAlways]
public class GroundSegment : MonoBehaviour
{
    private Curve curve;
    private ShadowCaster2D _shadowCaster;
    public SpriteShape spriteShape;
    public SpriteShapeController shapeController;
    private Spline spline;
    private List<Vector2> _unoffsetPoints;
    private int floorHeight = 100;
    private int containmentBuffer = 20;

    
    void Awake()
    {
        spline = shapeController.spline;
        shapeController.spriteShape = spriteShape;
        _shadowCaster = GetComponent<ShadowCaster2D>();
        FormatSpline(spline);
    }

    public void SetCurve(Curve curve, List<EdgeCollider2D> colliderList, PhysicsMaterial2D material, Vector3? overlapPoint = null)
    {
        this.curve = curve;
        GenerateSpline(spline, this.curve, floorHeight);
        CurveCollider.CreateCollider(colliderList, this, transform.parent, material, out _unoffsetPoints, overlapPoint);
        ShadowCasterCreator.GenerateShadow(this, _unoffsetPoints);
    }

    public void ApplyCurve(Curve curve)
    {
        this.curve = curve;
        GenerateSpline(spline, curve, floorHeight);
    }

    public bool StartsAfterX(float startX)
    {
        return curve.StartPoint.ControlPoint.x >= startX;
    }

    public bool EndsBeforeX(float endX)
    {
        return curve.EndPoint.ControlPoint.x <= endX;
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


    public Curve Curve
    {
        get
        {
            return curve;
        }
    }

    public Spline Spline
    {
        get
        {
            return spline;
        }
    }

    public bool ContainsX(float targetX)
    {
        return (targetX > curve.StartPoint.ControlPoint.x - containmentBuffer && targetX < curve.EndPoint.ControlPoint.x + containmentBuffer);
    }

    public Vector2 StartPoint
    {
        get
        {
            return curve.StartPoint.ControlPoint;
        }
    }

    public Vector2 EndPoint
    {
        get
        {
            return curve.EndPoint.ControlPoint;
        }
    }

    public List<Vector2> UnoffsetPoints
    {
        get
        {
            return _unoffsetPoints;
        }
    }

    public CurveType Type
    {
        get
        {
            return curve.Type;
        }
    }

    public ShadowCaster2D ShadowCaster { get => _shadowCaster; set => _shadowCaster = value; }

    public static void FormatSpline(Spline spline)
    {
        spline.isOpenEnded = false;
        while (spline.GetPointCount() > 2)
        {
            spline.RemovePointAt(2);
        }
    }

    

}
