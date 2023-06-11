using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Reflection;

public class GroundSegment : MonoBehaviour
{
    private Curve curve;
    private ShadowCaster2D shadowCaster;
    public SpriteShape spriteShape;
    public SpriteShapeController shapeController;
    private EdgeCollider2D edgeCollider;
    private Spline spline;
    private List<Vector2> unoffsetPoints;
    private int floorHeight = 100;


    void Awake()
    {
        spline = shapeController.spline;
        shapeController.spriteShape = spriteShape;
        edgeCollider = shapeController.edgeCollider;
        shadowCaster = GetComponent<ShadowCaster2D>();
        GroundUtility.FormatSpline(spline);
    }

    public void SetCurve(Curve curve, Vector3? overlapPoint = null)
    {
        this.curve = curve;
        GenerateSpline();
        CurveCollider.CreateCollider(this, out unoffsetPoints, overlapPoint);
        CollisionActive = false;
        ShadowCasterCreator.GenerateShadow(shadowCaster, this);
    }

    public bool StartsAfterX(float startX)
    {
        return curve.StartPoint.ControlPoint.x >= startX;
    }

    public bool EndsBeforeX(float endX)
    {
        return curve.EndPoint.ControlPoint.x <= endX;
    }

    private void GenerateSpline()
    {
        InsertCurveToSpline(curve, 1);
        UpdateCorners(floorHeight);
    }

    private void UpdateCorners(float lowerY)
    {
        UpdateRightCorners(lowerY);
        UpdateLeftCorners(lowerY);
    }

    private void UpdateRightCorners(float lowerY)
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
    private void UpdateLeftCorners(float lowerY)
    {

        spline.SetPosition(0, new Vector3(spline.GetPosition(1).x, spline.GetPosition(1).y - lowerY));
        spline.SetTangentMode(0, ShapeTangentMode.Linear);
        spline.SetLeftTangent(0, new Vector3(0, 1));
        spline.SetRightTangent(0, new Vector3(1, 0));
        spline.SetTangentMode(1, ShapeTangentMode.Broken);
        spline.SetLeftTangent(1, new Vector2(0, -1));
    }

    private void InsertCurveToSpline(Curve curve, int index) //Inserts curve into the spline beginning at the given index
    {
        for (int i = 0; i < curve.Count; i++)
        {
            InsertCurvePointToSpline(curve.GetPoint(i), index);
            index++;
        }
        spline.SetTangentMode(index, ShapeTangentMode.Broken);
        spline.SetRightTangent(index, new Vector3(0, -1));

    }

    private void InsertCurvePointToSpline(CurvePoint curvePoint, int index) //Inserts curvePoint at a given index
    {
        spline.InsertPointAt(index, curvePoint.ControlPoint);
        spline.SetTangentMode(index, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(index, curvePoint.LeftTangent);
        spline.SetRightTangent(index, curvePoint.RightTangent);
    }

    public bool CollisionActive
    {
        get
        {
            return edgeCollider.enabled;
        }
        set
        {
            edgeCollider.enabled = value;
        }
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
        return (targetX > curve.StartPoint.ControlPoint.x - 20 && targetX < curve.EndPoint.ControlPoint.x + 20);
    }

    public Vector3 LastColliderPoint
    {
        get
        {
            return edgeCollider.points[^1];
        }
    }

    public Vector2[] ColliderPoints
    {
        get
        {
            return edgeCollider.points;
        }
    }

    public List<Vector2> UnoffsetPoints
    {
        get
        {
            return unoffsetPoints;
        }
    }

    public CurveType Type
    {
        get
        {
            return curve.Type;
        }
    }

                                       

}
