using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class OldBezierColliderCreator : MonoBehaviour
{
    public Spline spline;
    public OldGroundSpline groundSpline;
    private List<Vector2[]> colliderPoints = new List<Vector2[]>();
    private GameObject bird;
    public SpriteShapeController controller;
    public float edgeOffset = 0.55f;
    private EdgeCollider2D edgeCollider;
    private Vector3 trailingPoint, leadingPoint;
    private int[] lengths = new int[3]; //the number of points contained by each of the three curves
    private int[] splineIndices; //After instantiation, this should always contain 4 points that are the 4 spline control points surrounding the bird.
    //Only use 4 spline points around bird


    public void Start()
    {
        bird = GameObject.FindWithTag("Player");
        spline = controller.spline;
        edgeCollider = controller.edgeCollider;
        int startIndex = GroundUtility.SplineIndexBeforeX(controller, bird.transform.position.x);
        splineIndices = new int[] { startIndex - 1, startIndex, startIndex + 1, startIndex + 2 };
        for (int i = 0; i < splineIndices.Length - 1; i++)
        {
            OldBezierCollider2D bezier = NewBezier(i);
            lengths[i] = bezier.Resolution;
            if(bezier.Resolution > 0)
            {
                edgeCollider.points = combineArrays(edgeCollider.points, bezier.calculate2DPoints(this, i));
            }
            UpdateBounds();
        }
    }

    //PROBLEM: When bird transitions from left third of edge to middle third (because bird is at the very left end of spline), the edge creates an additional straight edge between the third and fourth control points (i.e, the third edge).
    ////Possibly doing an unnecessary Add Right Point or moving leadingPoint farther back than it needs to.
    private void Update()
    {
        if (transform.InverseTransformPoint(bird.transform.position).x > leadingPoint.x)
        {
            AddRightPoint();
            
        }
        else if (transform.InverseTransformPoint(bird.transform.position).x < trailingPoint.x)
        {
            AddLeftPoint();
        }
    }

    public OldBezierCollider2D NewBezier(int i)
    {

        OldBezierCollider2D bezier = new OldBezierCollider2D();
        if (splineIndices[i] > 0)
        {
            bezier.firstPoint = spline.GetPosition(splineIndices[i]);
            bezier.secondPoint = spline.GetPosition(splineIndices[i + 1]);
            bezier.handlerFirstPoint = bezier.firstPoint + spline.GetRightTangent(splineIndices[i]);
            bezier.handlerSecondPoint = bezier.secondPoint + spline.GetLeftTangent(splineIndices[i + 1]);
            bezier.edgeOffset = edgeOffset;
        }
        return bezier;
    }

    public void AddLeftPoint()
    {
        ShiftIndices(-1);
        OldBezierCollider2D bezier = NewBezier(0);
        if (bezier.Resolution > 0 || splineIndices[0] == 0)
        {
            edgeCollider.points = combineArrays(bezier.calculate2DPoints(this, 0), edgeCollider.points).SkipLast(lengths[2]).ToArray();
        }
        lengths[2] = lengths[1];
        lengths[1] = lengths[0];
        lengths[0] = bezier.Resolution;
        UpdateBounds();
    }
    public void AddRightPoint()
    {
        ShiftIndices(1);
        OldBezierCollider2D bezier = NewBezier(splineIndices.Length - 2);
        if (bezier.Resolution > 0)
        {
            edgeCollider.points = combineArrays(edgeCollider.points, bezier.calculate2DPoints(this, 2)).Skip(lengths[0]).ToArray();
        }
        lengths[0] = lengths[1];
        lengths[1] = lengths[2];
        lengths[2] = bezier.Resolution;
        UpdateBounds();
    }

    public Vector2[] combineArrays(Vector2[] array1, Vector2[] array2)
    {
        Vector2[] combinedArray = new Vector2[array1.Length + array2.Length];
        array1.CopyTo(combinedArray, 0);
        array2.CopyTo(combinedArray, array1.Length);
        return combinedArray;
    }

    public void ShiftIndices(int delta)
    {
        for (int i = 0; i < splineIndices.Length; i++)
        {
            splineIndices[i] += delta;
        }
    }

    public void UpdateBounds()
    {
        leadingPoint = spline.GetPosition(splineIndices[2]);
        if (splineIndices[0] > 0)
        {
            trailingPoint = spline.GetPosition(splineIndices[1]);
        }
        else
        {
            trailingPoint = new Vector3(bird.transform.position.x - 10000, 0);
        }
        
    }
}
