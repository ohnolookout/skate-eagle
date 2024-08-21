using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class BgShadowSegment : MonoBehaviour
{
    public Spline ShadowSpline;
    public SpriteShapeController ShapeController;

    private const int FirstStartIndex = 0;
    private const int FirstEndIndex = 1;
    private const int SecondStartIndex = 3;
    private const int SecondEndIndex = 2;

    public Vector2 FirstStartPosition;
    public Vector2 SecondStartPosition;
    public Vector2 FirstEndPosition;

    private Vector2 _totalChangeVector;


    void Awake()
    {
        ShadowSpline = ShapeController.spline;
        FirstStartPosition = ShapeController.spline.GetPosition(FirstStartIndex);
        SecondStartPosition = ShapeController.spline.GetPosition(SecondStartIndex);
        FirstEndPosition = ShapeController.spline.GetPosition(FirstEndIndex);

        _totalChangeVector = FirstEndPosition - FirstStartPosition;
    }

    public void SetShadowLength(float t)
    {        
        t = Mathf.Clamp01(t);

        var newChangeVector = _totalChangeVector * t;

        ShapeController.spline.SetPosition(FirstEndIndex, FirstStartPosition + newChangeVector);
        ShapeController.spline.SetPosition(SecondEndIndex, SecondStartPosition + newChangeVector);
    }
}
