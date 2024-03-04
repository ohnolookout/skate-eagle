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
    private static Vector3[] shadowPointArray;
    public static void GenerateShadow(GroundSegment groundSegment, List<Vector2> colliderPoints)
    {
        ShadowCaster2D shadow = groundSegment.ShadowCaster;
        /*
        shadowPointArray = ShadowPoints(groundSegment, colliderPoints);
        shapePathField.SetValue(shadow, shadowPointArray);
        meshField.SetValue(shadow, null);
        onEnableMethod.Invoke(shadow, new object[0]);
        */
    }

    private static Vector3[] ShadowPoints(GroundSegment groundSegment, List<Vector2> colliderPoints)
    {
        List<Vector3> shadowPointList = new();
        shadowPointList.Add(colliderPoints[^1]);
        shadowPointList.Add(groundSegment.Spline.GetPosition(groundSegment.Spline.GetPointCount() - 1));
        shadowPointList.Add(groundSegment.Spline.GetPosition(0));
        int increment = (int)Mathf.Min(5, Mathf.Ceil(colliderPoints.Count / 20));
        for (int i = 0; i < colliderPoints.Count - 2; i += increment)
        {
            shadowPointList.Add(colliderPoints[i]);
        }
        return shadowPointList.ToArray();
    }
}
