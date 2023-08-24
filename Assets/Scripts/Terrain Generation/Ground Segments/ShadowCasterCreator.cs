using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Reflection;

public static class ShadowCasterCreator
{
    private static BindingFlags accessFlagsPrivate = BindingFlags.NonPublic | BindingFlags.Instance;
    private static FieldInfo meshField = typeof(ShadowCaster2D).GetField("m_Mesh", accessFlagsPrivate);
    private static FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);
    private static MethodInfo onEnableMethod = typeof(ShadowCaster2D).GetMethod("OnEnable", accessFlagsPrivate);
    private static GroundSegment groundSegment;
    private static Vector3[] shadowPointArray;
    public static void GenerateShadow(ShadowCaster2D shadow, GroundSegment ground)
    {
        groundSegment = ground;
        shadowPointArray = ShadowPoints();
        shapePathField.SetValue(shadow, shadowPointArray);
        meshField.SetValue(shadow, null);
        onEnableMethod.Invoke(shadow, new object[0]);

    }

    private static Vector3[] ShadowPoints()
    {
        List<Vector3> shadowPointList = new();
        Vector2[] colliderPoints = groundSegment.UnoffsetPoints.ToArray();
        shadowPointList.Add(colliderPoints[^1]);
        shadowPointList.Add(groundSegment.Spline.GetPosition(groundSegment.Spline.GetPointCount() - 1));
        shadowPointList.Add(groundSegment.Spline.GetPosition(0));
        int increment = (int)Mathf.Min(5, Mathf.Ceil(colliderPoints.Length / 20));
        for (int i = 0; i < colliderPoints.Length - 2; i += increment)
        {
            shadowPointList.Add(colliderPoints[i]);
        }
        return shadowPointList.ToArray();
    }
}
