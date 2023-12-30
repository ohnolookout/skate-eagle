using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class GroundSpawner : MonoBehaviour
{
    public GameObject groundSegment;
    private List<GroundSegment> segmentList;
    private List<EdgeCollider2D> colliderList;
    private Vector3 leadingActivePoint, trailingActivePoint, lowestPoint;
    private float trailingCameraBound, leadingCameraBound;
    private int leadingSegmentIndex = 3, trailingSegmentIndex = 0, birdIndex = 0;
    private float cameraBuffer = 25;
    private List<Vector3> activeLowPoints = new();
    private LiveRunManager logic;
    private GroundColliderTracker colliderTracker;
    [SerializeField] PhysicsMaterial2D colliderMaterial;
    [HideInInspector] public GameObject finishFlag; 
    public GameObject finishFlagPrefab;
    [SerializeField] GameObject backstop;
    public bool testMode = false;
    private Action<LiveRunManager> onGameOver;
    private enum SegmentPosition { Leading, Trailing };
    private enum CacheStatus { New, Removed, Added };

    private void OnEnable()
    {
        onGameOver += _ => SwitchToRagdoll();
        LiveRunManager.OnGameOver += onGameOver;
    }
    void Awake()
    {
        AssignComponents();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
        //Delete children if in Unity Editor due to the potential of creating children within level editor.
        DeleteChildren();
#endif
        GenerateLevel(GameManager.Instance.CurrentLevel);
        colliderTracker = new(logic.Player.Rigidbody, colliderList, segmentList, backstop, birdIndex);
        ActivateInitialSegments(3);
    }

    private void Start()
    {
    }
    void Update()
    {
        UpdateActiveSegments();
        //Exit update if run hasn't activated
        if ((int)logic.runState < 2)
        {
            return;
        }
        colliderTracker.UpdateColliders();
    }

    private void AssignComponents()
    {
        transform.position = new Vector2(0, 0);
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        segmentList = new();
    }

    public void SwitchToRagdoll()
    {
        colliderTracker.SwapBodies(new Rigidbody2D[] { logic.Player.rigidEagle }, 
            new Rigidbody2D[] { logic.Player.Rigidbody, logic.Player.RagdollBoard });
    }
    public void GenerateLevel(Level level)
    {
        List<LevelSection> levelSections = level.LevelSections;
        if (levelSections.Count < 1)
        {
            throw new Exception("Level must contain at least one section");
        }
        //If called outside of play mode in unity editor (i.e., in level editor), exit generate script and call Awake();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            AssignComponents();
            DeleteChildren();
        }
#endif
        segmentList = new();
        colliderList = new();
        //Create startline at location of player
        CurvePoint endOfLastSegment = AddSegment(CurveFactory.StartLine(new (logic.Player.Rigidbody.position)));
        //Create dictionary of sequences with corresponding grades
        Dictionary<Grade, Sequence> curveSequences = level.GenerateSequence();
        foreach(var sequence in curveSequences)
        {
            Grade grade = sequence.Key;
            //Instantiate segments for each curve from sequence using sequence's grade.
            foreach(CurveDefinition curveDef in sequence.Value.Curves)
            {
                //First create curve values, then add segment with corresponding gameobject
                Curve nextCurve = CurveFactory.CurveFromDefinition(curveDef, endOfLastSegment, grade.MinClimb, grade.MaxClimb);
                endOfLastSegment = AddSegment(nextCurve);
            }
        }
        //Add finishline at end of final segment
        AddSegment(CurveFactory.FinishLine(endOfLastSegment));
        AddFinishObjects(segmentList[^1].Curve.GetPoint(1).ControlPoint, segmentList[^1].EndPoint);
    }
    public void GenerateLevel(LiveRunManager runManager)
    {
        Debug.Log("Generating level from runManager");
        GenerateLevel(runManager.CurrentLevel);
    }

    //Instantiate a new deactivated segment, add it to the segment list, and update the current point and length
    private CurvePoint AddSegment(Curve curve)
    {
        Vector3? overlapPoint = null;
        //Instantiate segment object and add its script to segmentList
        GroundSegment newSegment = Instantiate(groundSegment, transform, true).GetComponent<GroundSegment>();
        segmentList.Add(newSegment);
        //If segment is not the first segment, get the last point from the preceding segment
        //to use as the first point in the new segment's collider
        if(colliderList.Count > 0)
        {
            overlapPoint = colliderList[^1].points[^1];
        }
        //Set the new segment's curve and overlap point, update the currentpoint to be the end of the new segment
        segmentList[^1].SetCurve(curve, colliderList, this, colliderMaterial, overlapPoint);
        //Exit here in unity editor if in test mode so that all segments remain active
        //Return endpoint of added segment
#if UNITY_EDITOR
        if (testMode)
        {
            return segmentList[^1].Curve.EndPoint; ;
        }
#endif
        //Otherwise deactivate segment on creation.
        newSegment.gameObject.SetActive(false);
        return segmentList[^1].Curve.EndPoint;
    }

    
    private void UpdateActiveSegments()
    {
        UpdateCameraBounds();
        //If current leadingSegmentIndex starts after the leading edge of the camera + buffer,
        //deactivate it and decrease leadingSegmentIndex
        if (segmentList[leadingSegmentIndex].StartsAfterX(leadingCameraBound))
        {
            DeactivateSegment(SegmentPosition.Leading);
        }
        else if (leadingSegmentIndex < segmentList.Count - 1)
        {
            //If the segment after the current leading segment starts before the leading edge of the camera + buffer,
            //Activate it and increase leadingSegmentIndex
            if (!segmentList[leadingSegmentIndex + 1].StartsAfterX(leadingCameraBound))
            {
                ActivateSegment(SegmentPosition.Leading);
            }
        }
        //Exit if trailingSegment index is outside the bounds of the segment array
        //Because player is on finishline segment.
        if(trailingSegmentIndex >= segmentList.Count)
        {
            return;
        }
        //If the trailingSegment ends before the trailing edge of the camera + buffer,
        //Deactivate it and increment the trailing index.
        if (segmentList[trailingSegmentIndex].EndsBeforeX(trailingCameraBound))
        {
            DeactivateSegment(SegmentPosition.Trailing);
        }
        else if (trailingSegmentIndex > 0)
        {
            //If the segment before the trailing segment index ends after the trailing edge of the camera + buffer,
            //Activate it and decrement the trailing index.
            if (!segmentList[trailingSegmentIndex - 1].EndsBeforeX(trailingCameraBound))
            {
                ActivateSegment(SegmentPosition.Trailing);
            }
        }
    }

    private void AddFinishObjects(Vector3 finishLineBound, Vector3 backstopBound)
    {
        //Assign locations finishPoint, backstop, and finishflag
        logic.FinishPoint = finishLineBound + new Vector3(50, 1);
        finishFlag = Instantiate(finishFlagPrefab, logic.FinishPoint, transform.rotation, transform);
        finishFlag.SetActive(false);
        backstop.transform.position = backstopBound - new Vector3(75, 0);
    }
    
    //Activate the given number of segments from the start of the level.
    //Only activate colliders for the first two segments.
    private void ActivateInitialSegments(int segmentCount)
    {
        leadingSegmentIndex = -1;
        for (int i = 0; i < segmentCount; i++)
        {
            ActivateSegment(SegmentPosition.Leading);
            if (i < 2)
            {
                colliderList[i].gameObject.SetActive(true);
            }
        }
    }

    //Update camera bounds based on lower corners of camera view and camera buffer.
    private void UpdateCameraBounds()
    {
        leadingCameraBound = logic.CameraScript.LeadingCorner.x + cameraBuffer;
        trailingCameraBound = logic.CameraScript.TrailingCorner.x - cameraBuffer;
    }


    //Activate trailing or leading segment, update leading or trailing segment index
    private void ActivateSegment(SegmentPosition position)
    {
        Vector3 addedPoint = transform.position;
        int index = -1;
        if(position == SegmentPosition.Leading && leadingSegmentIndex < segmentList.Count - 1)
        {
            index = leadingSegmentIndex + 1;
            activeLowPoints.Add(segmentList[index].Curve.LowPoint);
            addedPoint = activeLowPoints[activeLowPoints.Count - 1];
            leadingSegmentIndex++;
            if(leadingSegmentIndex >= segmentList.Count - 1)
            {
                finishFlag.SetActive(true);
            }
        } else if(position == SegmentPosition.Trailing && trailingSegmentIndex > 0)
        {
            index = trailingSegmentIndex - 1;
            activeLowPoints.Insert(0, segmentList[index].Curve.LowPoint);
            addedPoint = activeLowPoints[0];
            trailingSegmentIndex--;
        }
        if(index >= 0 && index <= segmentList.Count - 1)
        {
            segmentList[index].gameObject.SetActive(true);
            CacheLowestPoint(CacheStatus.Added, addedPoint);
        }
    }

    private void DeactivateSegment(SegmentPosition position)
    {
        int index = -1;
        if(position == SegmentPosition.Leading)
        {
            if (!testMode)
            {
                segmentList[leadingSegmentIndex].gameObject.SetActive(false);
            }
            leadingSegmentIndex--;
            index = activeLowPoints.Count - 1;
        } else if (position == SegmentPosition.Trailing)
        {
            if (!testMode)
            {
                segmentList[trailingSegmentIndex].gameObject.SetActive(false);
            }
            trailingSegmentIndex++;
            index = Mathf.Min(0, activeLowPoints.Count - 1);
        }
        if (index >= 0)
        {
            Vector3 removedPoint = activeLowPoints[index];
            activeLowPoints.RemoveAt(index);
            CacheLowestPoint(CacheStatus.Removed, removedPoint);
        }
    }

    private void CacheLowestPoint(CacheStatus status, Vector3 point)
    {
        if (activeLowPoints.Count == 0)
        {
            lowestPoint = activeLowPoints[0];
            return;
        }
        switch (status)
        {
            case CacheStatus.New:
                int startIndex = Mathf.Clamp(birdIndex - 2, 0, activeLowPoints.Count - 1);
                int endIndex = Mathf.Clamp(birdIndex + 4, startIndex, activeLowPoints.Count - 1);
                lowestPoint = activeLowPoints[startIndex];
                for (int i = startIndex; i <= endIndex; i++)
                {
                    if (activeLowPoints[i].y < lowestPoint.y) lowestPoint = activeLowPoints[i];
                }
                break;
            case CacheStatus.Removed:
                if (point == lowestPoint) CacheLowestPoint(CacheStatus.New, point);
                break;
            case CacheStatus.Added:
                if (point.y <= lowestPoint.y) lowestPoint = point;
                break;
        }
        
    }


    public void DeleteChildren()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        segmentList = new();
    }


    public Vector3 LowestPoint
    {
        get
        {
            return lowestPoint;
        }
    }
}
