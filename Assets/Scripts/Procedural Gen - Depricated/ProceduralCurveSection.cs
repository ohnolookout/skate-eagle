
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using JetBrains.Annotations;
using UnityEngine.UIElements;

/*
#region Enums
public enum LengthType { Short = 0, Medium = 1, Long = 2, Jumbo = 3};
public enum ShapeType { Roller, SoftPeak, HardPeak, SoftTable, HardTable }
public enum PitchType { Flat, Gentle, Normal, Steep };
public enum SectionType { Peak, Valley};
#endregion

[Serializable]
public class ProceduralCurveSection : ICurveSection
{
    #region Declarations
    public ShapeType _shapeType;
    public LengthType _lengthType;
    public PitchType _pitchType;
    public float ClimbMin = 0;
    public float ClimbMax = 0;
    public SectionType _sectionType;
    public LengthType LengthType => _lengthType;
    public ShapeType ShapeType => _shapeType;
    public PitchType PitchType => _pitchType;
    public SectionType SectionType => _sectionType;
    public float LengthMin => LengthMinMax().x;
    public float LengthMax => LengthMinMax().y;
    public float ShapeMin => ShapeMinMax().x;
    public float ShapeMax => ShapeMinMax().y;
    public float PitchMin => PitchMinMax().x;
    public float PitchMax => PitchMinMax().y;

    #endregion

    public ProceduralCurveSection(LengthType lengthType, ShapeType shapeType, PitchType slopeType, SectionType sectionType)
    {
        _lengthType = lengthType;
        _shapeType = shapeType;
        _pitchType = slopeType;
        _sectionType = sectionType;
    }

    public override CurveSectionParameters GetSectionParameters(Vector2 prevTangent)
    {   
        //Set modifier for curve to be concave or convex
        int peakValleyModifier = _sectionType == SectionType.Valley ? -1 : 1;

        //Create a limiter 6on parameters based on the right tangent of the previous curve section
        float prevSlope = Mathf.Abs(prevTangent.y / prevTangent.x);
        float prevTangSpacer = prevTangent.x + Mathf.Abs(prevTangent.y) / 3;

        //Generate length, climb, and shape from min and max values
        float length = UnityEngine.Random.Range(LengthMin + prevTangSpacer, LengthMax + prevTangSpacer);
        float climb = UnityEngine.Random.Range(ClimbMin, ClimbMax);
        float shape = UnityEngine.Random.Range(ShapeMin, ShapeMax);

        //Generate pitch based on length, climb, and previous tangent
        float grade = climb / length;
        float adjustedPitchMin = Mathf.Max(PitchMin, Mathf.Abs(prevSlope * 0.4f)) + (grade * peakValleyModifier);
        float adjustedPitchMax = Mathf.Min(PitchMax, Mathf.Abs(prevSlope * 2f)) + (grade * peakValleyModifier);
        float pitch = UnityEngine.Random.Range(adjustedPitchMin, adjustedPitchMax) * peakValleyModifier;

        //Return results as parameter object
        return new CurveSectionParameters(length, shape, pitch, climb);
    }

    public void AddGrade(Grade grade)
    {
        ClimbMin = grade.ClimbMin;
        ClimbMax = grade.ClimbMax;
    }

    #region Parameter Type Conversions
    //Converts enums into vectors of min/max values
    public Vector2 LengthMinMax()
    {
        return _lengthType switch
        {
            LengthType.Short => new Vector2(25, 40),
            LengthType.Medium => new Vector2(35, 55),
            LengthType.Long => new Vector2(55, 75),
            LengthType.Jumbo => new Vector2(75, 90),
            _ => new Vector2(35, 60)
        };
    }
    public Vector2 ShapeMinMax()
    {
        return _shapeType switch
        {
            ShapeType.HardTable => new Vector2(0.1f, 0.3f),
            ShapeType.SoftTable => new Vector2(0.3f, 0.45f),
            ShapeType.Roller => new Vector2(0.45f, 0.55f),
            ShapeType.SoftPeak => new Vector2(0.55f, 0.75f),
            ShapeType.HardPeak => new Vector2(0.75f, 0.9f),
            _ => new Vector2(0.45f, 0.55f)
        };
    }
    public Vector2 PitchMinMax()
    {
        return _pitchType switch
        {
            PitchType.Flat => new Vector2(0.4f, 0.7f),
            PitchType.Gentle => new Vector2(0.6f, 1.1f),
            PitchType.Normal => new Vector2(1.0f, 1.5f),
            PitchType.Steep => new Vector2(1.5f, 2.2f),
            _ => new Vector2(0.8f, 1.4f)
        };
    }

    #endregion


}
*/