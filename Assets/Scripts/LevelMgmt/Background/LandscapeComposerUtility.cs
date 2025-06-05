using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscapeComposerUtility : MonoBehaviour
{
    private static float LightSlope = 0.38f;
    [SerializeField] private int DrawDistance = 500;
    [SerializeField] private bool doDrawGuides = false;
    [SerializeField] private bool isGround = false;
    private static bool DoDrawAllGuides = false;
    [SerializeField] private Transform CastPoint;

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        if (doDrawGuides || DoDrawAllGuides)
        {
            var yChange = isGround ? 1 : (DrawDistance * LightSlope);
            Gizmos.DrawLine(CastPoint.position, new(CastPoint.position.x - DrawDistance, CastPoint.position.y - yChange));
        }
    }
}
