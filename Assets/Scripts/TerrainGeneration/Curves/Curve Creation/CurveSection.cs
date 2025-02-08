using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SectionType { Peak, Valley };

[Serializable]
public class CurveSection
{
    public SectionType sectionType;
    public float length;
    public float shape;
    public float pitch;
    public float climb;

    public CurveSection(SectionType sectionType, float length, float shape, float pitch, float climb)
    {
        this.sectionType = sectionType;
        this.length = length;
        this.shape = shape;
        this.pitch = pitch;
        this.climb = climb;
    }

    public CurveSection(SectionType sectionType)
    {
        this.sectionType = sectionType;
        length = 45;
        shape = 0.5f;
        pitch = 1.25f;
        climb = 0;

    }

    public CurveSectionParameters GetSectionParameters(Vector2 prevTangent)
    {
        //Set modifier for curve to be concave or convex
        int peakValleyModifier = sectionType == SectionType.Valley ? -1 : 1;

        var grade = climb / length;
        var adjustedPitch = (pitch + grade) * peakValleyModifier;

        return new CurveSectionParameters(length, shape, adjustedPitch, climb);
    }

    public void DeepCopy(CurveSection sectionToCopy)
    {
        sectionType = sectionToCopy.sectionType;
        length = sectionToCopy.length;
        shape = sectionToCopy.shape;
        pitch = sectionToCopy.pitch;
        climb = sectionToCopy.climb;
    }

    public void Log()
    {
        Debug.Log("~~~Curve Section~~~");
        Debug.Log($"Section Type: {sectionType}");
        Debug.Log($"Length: {length}");
        Debug.Log($"Shape: {shape}");
        Debug.Log($"Pitch: {pitch}");
        Debug.Log($"Climb: {climb}");
    }
}


