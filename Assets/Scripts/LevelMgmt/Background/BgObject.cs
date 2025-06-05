using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public enum BgObjectType { BuildingCenterBump, BuildingSquatWide}


public class BgObject : MonoBehaviour, IPosition
{
    public List<BgShadowSegment> ShadowSegments;
    public BgObjectType Type;
    public BgShadowData InterceptProjection;
    public Transform LightSource;
    public Transform CastPointTransform;
    public Transform ProjectionAnchor;
    private Vector2 _outboundCastPoint;
    private Vector2 _inboundCastPoint;
    public float YMin = -20;
    public float YMax = 0;
    public float XDistance = 10;
    public float YDistance = 0;

    private bool _shadowIsSet = false;
    private int _shadowIndex = -1;
    private float _shadowT;

    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public Vector2 OutboundCastPoint => CastPointTransform.position;

    /*
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        foreach(var y in InterceptProjection.YList)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new(ProjectionAnchor.localPosition.x, y)), 1);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(_inboundCastPoint), 2);
    }
    */

    void OnEnable()
    {
        if (!_shadowIsSet && _shadowIndex != -1)
        {
            ShadowSegments[_shadowIndex].SetShadowLength(_shadowT);
            _shadowIsSet = true;
        }
    }


    public void BuildShadow(Vector2 shadowCastPoint, float lightSlope)
    {
        shadowCastPoint = transform.InverseTransformPoint(shadowCastPoint);
        _inboundCastPoint = shadowCastPoint;
        float deltaX = shadowCastPoint.x - ProjectionAnchor.localPosition.x;
        float yIntercept = shadowCastPoint.y - (lightSlope * deltaX);

        if(yIntercept >= InterceptProjection.ProjectionYList[^1])
        {
            return;
        }

        bool hasFoundIntercept = InterceptProjection.ProjectionYList[0] > yIntercept;

        for(int i = 0; i < ShadowSegments.Count; i++)
        {
            if (hasFoundIntercept)
            {
                ShadowSegments[i].gameObject.SetActive(false);
                continue;
            }

            if(InterceptProjection.ProjectionYList[i + 1] > yIntercept)
            {
                hasFoundIntercept = true;
                float segmentYLength = InterceptProjection.ProjectionYList[i + 1] - InterceptProjection.ProjectionYList[i];
                float interceptYLength = yIntercept - InterceptProjection.ProjectionYList[i];
                _shadowIndex = i;
                _shadowT = interceptYLength / segmentYLength;
                continue;
            }
        }

        if (!hasFoundIntercept)
        {
            _shadowIsSet = true;
        }
    }

    public BgShadowData BuildInterceptProjection(float lightSlope)
    {
        var isActive = gameObject.activeInHierarchy;
        gameObject.SetActive(true);
        BgShadowData projection = new(ProjectionAnchor.localPosition.x);

        projection.ProjectionYList = new()
        {
            LocalizedSplinePoint(0, BgShadowSegment.FirstStartIndex).y,
            LocalizedSplinePoint(0, BgShadowSegment.FirstEndIndex).y
        };

        for (int i = 1; i < ShadowSegments.Count; i++)
        {
            var splinePosition = LocalizedSplinePoint(i, BgShadowSegment.FirstEndIndex);
            float deltaX = projection.ProjectionX - splinePosition.x;
            projection.ProjectionYList.Add(splinePosition.y + (deltaX * lightSlope));
        }
        gameObject.SetActive(isActive);
        InterceptProjection = projection;
        return projection;
    }

    private Vector3 LocalizedSplinePoint(int shadowSegmentIndex, int splinePointIndex)
    {
        var point = ShadowSegments[shadowSegmentIndex].ShapeController.spline.GetPosition(splinePointIndex);
        point = ShadowSegments[shadowSegmentIndex].transform.parent.transform.TransformPoint(point);
        return transform.InverseTransformPoint(point);
    }
}
