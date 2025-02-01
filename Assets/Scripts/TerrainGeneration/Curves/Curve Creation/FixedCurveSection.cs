using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedCurveSection : CurveSection
{
    public SectionType sectionType;
    public float length;
    public float shape;
    public float pitch;
    public float climb;

    public FixedCurveSection(SectionType sectionType, float length, float shape, float pitch)
    {
        this.sectionType = sectionType;
        this.length = length;
        this.shape = shape;
        this.pitch = pitch;
    }

    public FixedCurveSection(SectionType sectionType)
    {
        this.sectionType = sectionType;
        length = 45;
        shape = 0.5f;
        pitch = 1.25f;
        climb = 0;

    }

    public override CurveSectionParameters GetSectionParameters(Vector2 prevTangent)
    {
        //Set modifier for curve to be concave or convex
        int peakValleyModifier = sectionType == SectionType.Valley ? -1 : 1;
        
        var grade = climb / length;
        var adjustedPitch = pitch + grade * peakValleyModifier;

        return new CurveSectionParameters(length, shape, adjustedPitch, climb);
    }

}
