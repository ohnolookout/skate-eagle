using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BgObjectType { BuildingCenterBump, BuildingSquatWide}


public class BgObject : MonoBehaviour, IPosition
{
    public List<BgShadowSegment> ShadowSegments;
    public BgObjectType Type;
    public Transform LightSource;
    public Transform CastPoint;
    public float YMin = -20;
    public float YMax = 0;
    public float XDistance = 10;
    public float YDistance = 0;

    private List<Vector2> _interceptProjectionPoints;

    public List<Vector2> InterceptProjectionPoints { get => _interceptProjectionPoints; set => _interceptProjectionPoints = value; }
    public Vector3 Position => transform.position;

    /*
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        Gizmos.color = Color.cyan;
        foreach(var point in _interceptProjectionPoints)
        {
            Gizmos.DrawSphere(transform.TransformPoint(point), 1);
        }
    }
    */

    public void SetInterceptPoints(List<Vector2> points)
    {
        _interceptProjectionPoints = points;
    }

    public void BuildShadow(Vector2 shadowCastPoint, float lightSlope)
    {
        shadowCastPoint = transform.InverseTransformPoint(shadowCastPoint);
        float deltaX = shadowCastPoint.x - _interceptProjectionPoints[0].x;
        float yIntercept = shadowCastPoint.y - (lightSlope * deltaX);
        if(yIntercept >= _interceptProjectionPoints[^1].y)
        {
            return;
        }

        bool hasFoundIntercept = _interceptProjectionPoints[0].y > yIntercept;

        for(int i = 0; i < ShadowSegments.Count; i++)
        {
            if (hasFoundIntercept)
            {
                ShadowSegments[i].gameObject.SetActive(false);
                continue;
            }

            if( _interceptProjectionPoints[i + 1].y > yIntercept)
            {
                hasFoundIntercept = true;
                float segmentYLength = _interceptProjectionPoints[i + 1].y - _interceptProjectionPoints[i].y;
                float interceptYLength = yIntercept - _interceptProjectionPoints[i].y;
                ShadowSegments[i].SetShadowLength(interceptYLength / segmentYLength);
                continue;
            }
        }
    }

    public List<Vector2> BuildShadowInterceptPoints(float lightSlope)
    {
        float startX = ShadowSegments[0].FirstStartPosition.x;
        _interceptProjectionPoints = new() { ShadowSegments[0].FirstStartPosition, ShadowSegments[0].FirstEndPosition };

        for(int i = 1; i < ShadowSegments.Count; i++)
        {
            float deltaX = startX - ShadowSegments[i].FirstEndPosition.x;
            _interceptProjectionPoints.Add(new(startX, ShadowSegments[i].FirstEndPosition.y + (deltaX * lightSlope)));
        }

        return _interceptProjectionPoints;
        
    }
}
