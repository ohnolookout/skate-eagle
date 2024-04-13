using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TerrainGenerator
{
    #region Level Generation
    public static void GenerateLevel(Level level, LevelTerrain terrain, Vector3 playerStartPosition, out Vector2 finishPoint)
    {
        if (level.LevelSections.Count < 1)
        {
            throw new Exception("Level must contain at least one section");
        }

        ResetTerrain(terrain);

        CurvePoint endOfLastSegment = GenerateStartSegment(terrain, playerStartPosition);

        endOfLastSegment = GenerateAllSequences(terrain, level.GenerateSequences(), endOfLastSegment);

        terrain.PopulateMinMaxLists();

        finishPoint = GenerateFinishSegment(terrain, endOfLastSegment);
    }

    private static CurvePoint GenerateAllSequences(LevelTerrain terrain, Dictionary<Grade, Sequence> curveSequences, CurvePoint endOfMostRecentSegment)
    {
        foreach (var sequence in curveSequences)
        {
            GenerateSegmentsFromSequence(terrain, sequence.Key, sequence.Value, endOfMostRecentSegment, out endOfMostRecentSegment);
        }
        return endOfMostRecentSegment;
    }

    private static void GenerateSegmentsFromSequence(LevelTerrain terrain, Grade grade, Sequence sequence, CurvePoint startPoint, out CurvePoint endPoint)
    {
        //Instantiate segments for each curve from sequence using sequence's grade.
        foreach (CurveDefinition curveDef in sequence.Curves)
        {
            //First create curve values, then add segment with corresponding gameobject
            Curve nextCurve = CurveFactory.CurveFromDefinition(curveDef, startPoint, grade.MinClimb, grade.MaxClimb);
            IGroundSegment newSegment = GenerateCompleteSegment(terrain, nextCurve, terrain.LastColliderPoint());
            startPoint = newSegment.Curve.EndPoint;
        }
        endPoint = startPoint;
    }

    private static void ResetTerrain(LevelTerrain terrain)
    {
        terrain.transform.position = new(0, 0);
        terrain.SegmentList = new();
        terrain.ColliderList = new();
        terrain.PositionalColliderList = new();
    }
    #endregion

    #region Segment Generation
    private static IGroundSegment GenerateCompleteSegment(LevelTerrain terrain, Curve curve, Vector3? colliderStartPoint = null)
    {
        IGroundSegment newSegment = CreateSegment(terrain, curve);
        terrain.SegmentList.Add(newSegment);
        EdgeCollider2D newCollider = CreateCollider(terrain, curve, terrain.ColliderMaterial, colliderStartPoint);
        terrain.ColliderList.Add(newCollider); //Delete when colliders fixed
        terrain.PositionalColliderList.Add(new(newCollider));
        return newSegment;

    }
    private static IGroundSegment CreateSegment(LevelTerrain terrain, Curve curve)
    {
        //Instantiate segment object and add its script to segmentList
        IGroundSegment newSegment = terrain.InstantiateSegment().GetComponent<IGroundSegment>();
        //Set the new segment's curve and deactivate the segment.
        newSegment.ApplyCurve(curve);
        newSegment.gameObject.SetActive(false);
#if UNITY_EDITOR
        //Set all segments active if in editor mode to show generated level.
        if (!Application.isPlaying)
        {
            newSegment.gameObject.SetActive(true);
        }
#endif
        //Return endpoint of added segment
        return newSegment;
    }

    private static EdgeCollider2D CreateCollider(LevelTerrain terrain, Curve curve, PhysicsMaterial2D colliderMaterial, Vector3? firstPoint = null, float resolutionMult = 10)
    {
        GameObject colliderObject = new("Collider");
        colliderObject.transform.parent = terrain.transform;
        EdgeCollider2D newCollider = CurveCollider.GenerateCollider(curve, colliderObject, colliderMaterial, firstPoint, resolutionMult);        
        colliderObject.SetActive(false);
        return newCollider;
    }

    private static CurvePoint GenerateStartSegment(LevelTerrain terrain, Vector3 startPosition)
    {
        //Create startline at location of player
        IGroundSegment newSegment = GenerateCompleteSegment(terrain, CurveFactory.StartLine(new(startPosition)));
        return newSegment.Curve.EndPoint;
    }

    private static Vector2 GenerateFinishSegment(LevelTerrain terrain, CurvePoint endOfLastSegment)
    {
        IGroundSegment finishSegment = GenerateCompleteSegment(terrain, CurveFactory.FinishLine(endOfLastSegment), terrain.ColliderList[^1].points[^1]);
        Vector2 finishPoint = AddFinishObjectsToSegment(terrain, finishSegment, terrain.SegmentList[^1].Curve.GetPoint(1).ControlPoint, terrain.SegmentList[^1].EndPoint);
        return finishPoint;
    }

    private static Vector2 AddFinishObjectsToSegment(LevelTerrain terrain, IGroundSegment segment, Vector3 finishPoint, Vector3 backstopPoint)
    {
        finishPoint += new Vector3(50, 1);
        GameObject.Instantiate(terrain.FinishFlagPrefab, finishPoint, segment.gameObject.transform.rotation, segment.gameObject.transform);        
        GameObject.Instantiate(terrain.BackstopPrefab, backstopPoint - new Vector3(75, 0), segment.gameObject.transform.rotation, segment.gameObject.transform);
        segment.IsFinish = true;
        return finishPoint;
    }
    #endregion

}
