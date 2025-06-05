using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class BgShadowSegment : MonoBehaviour
{
    public Spline ShadowSpline;
    public SpriteShapeController ShapeController;

    public const int FirstStartIndex = 0;
    public const int FirstEndIndex = 1;
    public const int SecondStartIndex = 3;
    public const int SecondEndIndex = 2;

    public Vector2 FirstStartPosition;
    public Vector2 SecondStartPosition;
    public Vector2 FirstEndPosition;

    private Vector2 _totalChangeVector;


    void Awake()
    {
        _totalChangeVector = FirstEndPosition - FirstStartPosition;
        ShadowSpline = ShapeController.spline;
    }

    public void SetShadowLength(float t)
    {
        _totalChangeVector = FirstEndPosition - FirstStartPosition;
        if(t > 0.9)
        {
            gameObject.SetActive(true);
            return;
        } else if (t < 0.1)
        {
            gameObject.SetActive(false);
            return;
        }
        
        var newChangeVector = _totalChangeVector * t;

        ShapeController.spline.SetPosition(FirstEndIndex, FirstStartPosition + newChangeVector);
        ShapeController.spline.SetPosition(SecondEndIndex, SecondStartPosition + newChangeVector);
    }
}
