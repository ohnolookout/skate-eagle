using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TerrainGenerator
{

    public static Vector2 GenerateLevel(Level level, LevelTerrain terrain, Vector3 playerStartPosition)
    {
        terrain.transform.position = new(0, 0);
        if (level.LevelSections.Count < 1)
        {
            throw new Exception("Level must contain at least one section");
        }
        terrain.SegmentList = new();
        terrain.ColliderList = new();
        //Create startline at location of player
        GroundSegment newSegment = GenerateCompleteSegment(terrain, CurveFactory.StartLine(new(playerStartPosition)));
        CurvePoint endOfLastSegment = newSegment.Curve.EndPoint;
        //Create dictionary of sequences with corresponding grades
        Dictionary<Grade, Sequence> curveSequences = level.GenerateSequence();
        foreach (var sequence in curveSequences)
        {
            GenerateSegmentsFromSequence(terrain, sequence.Key, sequence.Value, endOfLastSegment, out endOfLastSegment);
        }
        //Add finishline at end of final segment
        GenerateCompleteSegment(terrain, CurveFactory.FinishLine(endOfLastSegment), terrain.ColliderList[^1].points[^1]);
        Vector2 finishPoint = AddFinishObjects(terrain, terrain.SegmentList[^1].Curve.GetPoint(1).ControlPoint, terrain.SegmentList[^1].EndPoint);
        return finishPoint;
    }

    private static void GenerateSegmentsFromSequence(LevelTerrain terrain, Grade grade, Sequence sequence, CurvePoint startPoint, out CurvePoint endPoint)
    {
        //Instantiate segments for each curve from sequence using sequence's grade.
        foreach (CurveDefinition curveDef in sequence.Curves)
        {
            //First create curve values, then add segment with corresponding gameobject
            Curve nextCurve = CurveFactory.CurveFromDefinition(curveDef, startPoint, grade.MinClimb, grade.MaxClimb);
            GroundSegment newSegment = GenerateCompleteSegment(terrain, nextCurve, terrain.LastColliderPoint());
            startPoint = newSegment.Curve.EndPoint;
        }
        endPoint = startPoint;
    }

    private static GroundSegment GenerateCompleteSegment(LevelTerrain terrain, Curve curve, Vector3? colliderStartPoint = null)
    {
        GroundSegment newSegment = CreateSegment(terrain, curve);
        terrain.SegmentList.Add(newSegment);
        EdgeCollider2D newCollider = CreateCollider(terrain, curve, terrain.ColliderMaterial, out List<Vector2> shadowPoints, colliderStartPoint);
        terrain.ColliderList.Add(newCollider);
        ShadowCasterCreator.GenerateShadow(newSegment, shadowPoints);
        return newSegment;

    }
    private static GroundSegment CreateSegment(LevelTerrain terrain, Curve curve)
    {
        //Instantiate segment object and add its script to segmentList
        GroundSegment newSegment = terrain.InstantiateSegment().GetComponent<GroundSegment>();
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

    private static EdgeCollider2D CreateCollider(LevelTerrain terrain, Curve curve, PhysicsMaterial2D colliderMaterial, out List<Vector2> shadowPoints, Vector3? firstPoint = null, float resolutionMult = 10)
    {
        GameObject colliderObject = new("Collider");
        colliderObject.transform.parent = terrain.transform;
        EdgeCollider2D newCollider = CurveCollider.GenerateCollider(curve, colliderObject, colliderMaterial, out shadowPoints, firstPoint, resolutionMult);        
        colliderObject.SetActive(false);
        return newCollider;
    }
    private static Vector2 AddFinishObjects(LevelTerrain terrain, Vector3 finishLineBound, Vector3 backstopBound)
    {
        Vector3 finishPoint = finishLineBound + new Vector3(50, 1);
        //Assign locations finishPoint, backstop, and finishflag
        terrain.InstantiateFinish(finishPoint, backstopBound);
        return finishPoint;
    }

}
