using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Projection Library", menuName = "ScriptableObjects/ProjectionLibrary")]
[Serializable]
public class BgShadowDataLibrary : ScriptableObject
{
    public Dictionary<BgObjectType, BgShadowData> ProjectionDict;

    public BgShadowDataLibrary
        ()
    {
        ProjectionDict = new();
    }
}

public class BgShadowData
{
    public float ProjectionX;
    public List<float> ProjectionYList;
    public List<BgShadowSegmentPoints> SegmentPointsList;

    public BgShadowData(float x)
    {
        ProjectionX = x;
    }

    public BgShadowData(float x, List<float> yList)
    {
        ProjectionX = x;
        ProjectionYList = yList;
    }

    public void BuildSegmentDataList(List<BgShadowSegment> shadowSegments)
    {
        foreach(var segment in shadowSegments)
        {
            BgShadowSegmentPoints newSetup = new();
            newSetup.AssignVectorIndices(segment);
            SegmentPointsList.Add(newSetup);
        }
    }
}

public class BgShadowSegmentPoints
{
    public int UpperLeftIndex = 0;
    public int LowerLeftIndex = 0;
    public int UpperRightIndex = 0;
    public int LowerRightIndex = 0;

    public void AssignVectorIndices(BgShadowSegment segment)
    {
        List<Vector2> vectors = new();
        for (int i = 0; i < segment.ShapeController.spline.GetPointCount(); i++)
        {
            vectors.Add(segment.ShapeController.spline.GetPosition(i));
        }
        Vector2[] counts = { new(0, 0), new(0, 0), new(0, 0), new(0, 0) };
        for (int i = 0; i < vectors.Count; i++) 
        {
            for(int j = i + 1; j < vectors.Count - (i + 1); j++)
            {
                if(vectors[i].x > vectors[j].x)
                {
                    counts[i] += new Vector2(1, 0);
                    counts[j] -= new Vector2(1, 0);
                } else if (vectors[i].x < vectors[j].x)
                {
                    counts[i] -= new Vector2(1, 0);
                    counts[j] += new Vector2(1, 0);
                }
            }
        }

        for(int i = 0; i < vectors.Count; i++)
        {
            if(vectors[i].x >= 0)
            {
                if(vectors[i].y >= 0)
                {
                    UpperRightIndex = i;
                }
                else
                {
                    LowerRightIndex = i;
                }
            } else
            {
                if (vectors[i].y >= 0)
                {
                    UpperLeftIndex = i;
                }
                else
                {
                    LowerLeftIndex = i;
                }
            }
        }
    }


}