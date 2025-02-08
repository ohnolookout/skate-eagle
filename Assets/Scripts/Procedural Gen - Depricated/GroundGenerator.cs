using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/*
public static class GroundGenerator
{
    #region Level Generation
    //public static void GenerateLevel(Level level, Ground terrain, Vector3 playerStartPosition, out Vector2 finishPoint)
    
    public static void GenerateLevel(Level level, GroundManager manager, Ground terrain)
    {
        if (level.LevelSections.Count < 1)
        {
            throw new Exception("Level must contain at least one section");
        }

        ResetTerrain(terrain);

        var lastSegment = GenerateStartSegment(terrain, manager);

        var endOfLastSegment = GenerateAllSequences(terrain, level.GenerateSequences(), lastSegment.Curve.EndPoint);

        terrain.PopulateMinMaxLists();

        GenerateFinishSegment(terrain, manager, endOfLastSegment);
    }

    private static CurvePoint GenerateAllSequences(Ground terrain, Dictionary<Grade, Sequence> curveSequences, CurvePoint endOfMostRecentSegment)
    {
        foreach (var sequence in curveSequences)
        {
            GenerateSegmentsFromSequence(terrain, sequence.Value, endOfMostRecentSegment, out endOfMostRecentSegment);
        }
        return endOfMostRecentSegment;
    }

    private static void GenerateSegmentsFromSequence(Ground terrain, Sequence sequence, CurvePoint startPoint, out CurvePoint endPoint)
    {
        foreach (ProceduralCurveDefinition curveDef in sequence.Curves)
        {
            terrain.AddSegment(curveDef);
        }
        endPoint = terrain.EndPoint;
    }

    private static void ResetTerrain(Ground terrain)
    {
        terrain.transform.position = new(0, 0);
        terrain.SegmentList = new();
    }
    #endregion

    #region Segment Generation
    
    #endregion

}
*/