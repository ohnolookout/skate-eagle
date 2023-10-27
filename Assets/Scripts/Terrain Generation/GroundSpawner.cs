using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class GroundSpawner : MonoBehaviour
{
    public GameObject groundSegment;
    public List<GroundSegment> segmentList;
    private CurvePoint currentPoint;
    private Vector3 leadingActivePoint, trailingActivePoint, lowestPoint;
    private float trailingCameraBound, leadingCameraBound, length = 0;
    private int leadingSegmentIndex = 3, trailingSegmentIndex = 0, birdIndex = 0;
    private CameraScript cameraScript;
    private float cameraBuffer = 25;
    private List<Vector3> activeLowPoints = new();
    private LiveRunManager logic;
    public GameObject finishFlag;
    private GameManager gameManager;
    public bool testMode = false;
    private enum SegmentPosition { Leading, Trailing };
    private enum CacheStatus { New, Removed, Added };

    void Awake()
    {
        AssignComponents();
        currentPoint = new(logic.BirdPosition);
        if (Application.isPlaying)
        {
            DeleteChildren();
            GenerateLevel(gameManager.CurrentLevel);
            logic.FinishPoint = segmentList[segmentList.Count - 1].Curve.GetPoint(1).ControlPoint + new Vector3(50, 1);

            Instantiate(finishFlag, logic.FinishPoint, transform.rotation, transform);
            FindBirdIndex();
        }

    }
    void Start()
    {
        TrimLevel();

    }
    void Update()
    {
        UpdateActiveSegments();
        UpdateCollision();
    }

    private void AssignComponents()
    {
        gameManager = GameManager.Instance;
        cameraScript = Camera.main.GetComponent<CameraScript>();
        transform.position = new Vector2(0, 0);
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        segmentList = new();
    }

    public void GenerateLevel(Level level)
    {
        List<LevelSection> levelSections = level.LevelSections;
        if (levelSections.Count < 1)
        {
            throw new Exception("Level must contain at least one section");
        }
        if (!Application.isPlaying)
        {
            Awake();
        }
        DeleteChildren();
        segmentList = new();
        currentPoint = new(logic.BirdPosition);
        AddSegment(CurveFactory.StartLine(currentPoint));
        Dictionary<Grade, Sequence> curveSequences = level.GenerateSequence();
        foreach(KeyValuePair<Grade, Sequence> sequence in curveSequences)
        {
            Grade grade = sequence.Key;
            foreach(CurveDefinition curve in sequence.Value.Curves)
            {
                Curve nextCurve = CurveFactory.CompoundCurve(curve, currentPoint, grade.MinClimb, grade.MaxClimb);
                AddSegment(nextCurve);
            }
        }
        AddSegment(CurveFactory.FinishLine(currentPoint));
        leadingSegmentIndex = -1;
        for (int i = 0; i < 2; i++)
        {
            ActivateSegment(SegmentPosition.Leading);
        }
    }

    private void AddSegment(Curve curve)
    {
        Vector3? overlapPoint = null;
        GameObject newSegment = Instantiate(groundSegment, transform, true);
        segmentList.Add(newSegment.GetComponent<GroundSegment>());
        if(segmentList.Count > 1)
        {
            overlapPoint = segmentList[^2].LastColliderPoint;
        }
        segmentList[^1].SetCurve(curve, overlapPoint);
        currentPoint = segmentList[^1].Curve.EndPoint;
        if (!testMode)
        {
            newSegment.SetActive(false);
        }
        length += segmentList[^1].Curve.Length;
    }

    private void UpdateCollision()
    {
        if(logic.BirdDirectionForward && birdIndex >= segmentList.Count - 1 || (!logic.BirdDirectionForward && birdIndex == 0))
        {
            return;
        }
        if ((logic.BirdDirectionForward && !segmentList[birdIndex + 1].CollisionActive) ||
            (!logic.BirdDirectionForward && !segmentList[birdIndex - 1].CollisionActive))
        {
            ChangeCollisionDirection(logic.BirdDirectionForward);
        }

        CheckCurrentSegment();


    }

    private void UpdateActiveSegments()
    {
        UpdateCameraBounds();
        if (segmentList[trailingSegmentIndex].EndsBeforeX(trailingCameraBound))
        {
            DeactivateSegment(SegmentPosition.Trailing);
        }
        else if (trailingSegmentIndex > 0)
        {
            if (!segmentList[trailingSegmentIndex - 1].EndsBeforeX(trailingCameraBound))
            {
                ActivateSegment(SegmentPosition.Trailing);
            }
        }
        if (segmentList[leadingSegmentIndex].StartsAfterX(leadingCameraBound))
        {
            DeactivateSegment(SegmentPosition.Leading);
        } 
        else if (leadingSegmentIndex < segmentList.Count - 1)
        {
            if (!segmentList[leadingSegmentIndex + 1].StartsAfterX(leadingCameraBound))
            {
                ActivateSegment(SegmentPosition.Leading);
            }
        }
    }

    private void UpdateTrailingPoint()
    {
        trailingActivePoint = segmentList[trailingSegmentIndex].Curve.StartPoint.ControlPoint;
    }

    private void UpdateLeadingPoint()
    {
        leadingActivePoint = segmentList[leadingSegmentIndex].Curve.StartPoint.ControlPoint;
    }

    //Can speed this up by starting at birdIndex and finding leading and trailing indices by working out from there.
    private void TrimLevel()
    {
        while(segmentList[leadingSegmentIndex].StartsAfterX(leadingCameraBound) || segmentList[trailingSegmentIndex].EndsBeforeX(trailingCameraBound))
        {
            UpdateActiveSegments();
        }
        for(int i = trailingSegmentIndex; i <= leadingSegmentIndex; i++)
        {
            activeLowPoints.Add(segmentList[i].Curve.LowPoint);
        }
        CacheLowestPoint(CacheStatus.New, transform.position);            
    }

    private void FindBirdIndex()
    {
        while (!segmentList[birdIndex].ContainsX(logic.BirdPosition.x) && birdIndex < segmentList.Count - 1)
        {
            birdIndex++;
        }
        segmentList[birdIndex].CollisionActive = true;
        if (birdIndex < segmentList.Count - 1) segmentList[birdIndex + 1].CollisionActive = true;
    }

    private void UpdateCameraBounds()
    {
        leadingCameraBound = cameraScript.LeadingCorner.x + cameraBuffer;
        trailingCameraBound = cameraScript.TrailingCorner.x - cameraBuffer;
    }


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

    private void CheckCurrentSegment()
    {
        if (!segmentList[birdIndex].ContainsX(logic.BirdPosition.x))
        {
            if (logic.BirdDirectionForward)
            {
                birdIndex++;
            }
            else
            {
                birdIndex--;
            }
        }
    }

    private void ChangeCollisionDirection(bool forward)
    {
        //Checks to see if bird is contained by the segment at previous index.
        //Decrements birdIndex if true.
        int indexModifier = -1;
        if (!forward)
        {
            indexModifier = 1;
        }
        if (segmentList[birdIndex + indexModifier].ContainsX(logic.BirdPosition.x))
        {
            birdIndex+= indexModifier;
        }
        segmentList[birdIndex - indexModifier].CollisionActive = true;
        //If birdIndex isn't at 0, it deactives that preceding index.
        if ((forward && birdIndex >  0) || (!forward && birdIndex < segmentList.Count - 1))
        {
            segmentList[birdIndex + indexModifier].CollisionActive = false;
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

    public CurvePoint CurrentEndPoint
    {
        get
        {
            return currentPoint;
        }
    }
}
