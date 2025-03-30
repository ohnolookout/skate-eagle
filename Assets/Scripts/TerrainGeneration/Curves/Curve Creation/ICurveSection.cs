using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CurveSectionType
{
    Straight,
    Custom,
    Standard
}
public interface ICurveSection
{
    CurveSectionType CurveType { get; }
    public abstract List<CurvePoint> GetCurvePoints();
}
