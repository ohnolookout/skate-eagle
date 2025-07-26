using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using UnityEngine;
using System.Linq;

[Serializable]
public class Level
{
    [SerializeField] private string _UID = "";
    [SerializeField] private string _name;
    [SerializeField] private int _goldRequired = 0;
    [SerializeField] private MedalTimes _medalTimes;
    [SerializeReference][HideInInspector] private List<IDeserializable> _serializedObjects;
    [SerializeField] private string _leaderboardKey = "None";
    [SerializeField] private float _killPlaneY;
    [SerializeField] private Vector2 _cameraStartPosition = new(116, 6);
    [SerializeField] private LinkedCameraTarget _rootCameraTarget;
    [SerializeField] private LinkedCameraTarget _startTarget;
    [SerializeField] private SerializedStartLine _serializedStartLine;
    public string UID { get => _UID; set => _UID = value; }
    public string Name { get => _name; set => _name = value; }
    public MedalTimes MedalTimes { get => _medalTimes; set => _medalTimes = value; }
    public List<IDeserializable> SerializedObjects { get => _serializedObjects; set => _serializedObjects = value; }
    public string LeaderboardKey { get => _leaderboardKey; set => _leaderboardKey = value; }
    public int GoldRequired => _goldRequired;
    public float KillPlaneY { get => _killPlaneY; set => _killPlaneY = value; }
    public Vector2 CameraStartPosition { get => _cameraStartPosition; set => _cameraStartPosition = value; }
    public LinkedCameraTarget RootCameraTarget { get => _rootCameraTarget; set => _rootCameraTarget = value; }
    public LinkedCameraTarget StartTarget { get => _startTarget; set => _startTarget = value; }
    public SerializedStartLine SerializedStartLine => _serializedStartLine;

    public Level(string name, MedalTimes medalTimes, GroundManager groundManager, Vector2 cameraStartPosition = new())
    {
        _name = name;
        _medalTimes = medalTimes;
        _rootCameraTarget = CameraTargetBuilder.BuildKdTree(groundManager.CameraTargetables);
        _serializedObjects = SerializeLevelUtility.SerializeGroundManager(groundManager, out _serializedStartLine);
        _startTarget = groundManager.StartLine.StartPoint.LinkedCameraTarget;
        _leaderboardKey = _name + "_leaderboard";
        if (cameraStartPosition == new Vector2(0,0))
        {
            Debug.Log("Setting camera start position to serialized start line position.");
            cameraStartPosition = _serializedStartLine.StartPositionWithOffset;
        }
        else
        {
            _cameraStartPosition = cameraStartPosition;
        }
        _killPlaneY = GetKillPlaneY(groundManager.Grounds);
    }
    public Level(string name)
    {
        _name = name;
        _medalTimes = new();
        _serializedObjects = new();
        _rootCameraTarget = null;
        _startTarget = null;
        _leaderboardKey = _name + "_leaderboard";
        _cameraStartPosition = new();
        _killPlaneY = 0;
    }

    //Reserialization utility for old level system

    //public void Reserialize()
    //{
    //    Debug.Log("Reserializing Level: " + _name);
    //    if (_serializedObjects != null)
    //    {
    //        Debug.Log("Serialized objects at start: " + _serializedObjects.Count);
    //    } else
    //    {
    //        Debug.Log("Serialized objects is null, initializing new list.");
    //    }
    //    _serializedObjects = new();

    //    foreach(var ground in _serializedGrounds)
    //    {
    //        if (ground.serializedObjectList == null)
    //        {
    //            ground.serializedObjectList = new();
    //        }

    //        foreach (var serializedSegment in ground.segmentList)
    //        {
    //            if (ground.serializedObjectList.Contains(serializedSegment))
    //            {
    //                continue;
    //            }

    //            ground.serializedObjectList.Add(serializedSegment);
    //        }

    //        _serializedObjects.Add(ground);
    //    }

    //    _serializedObjects.Add(_serializedFinishLine);

    //    Debug.Log("Serialized objects after reserialize: " + _serializedObjects.Count);
    //}

    //Uncomment this method to populate curve points for serialized grounds
    public void PopulateGroundCurvePoints()
    {
        var count = 0;
        var curvePointCount = 0;
        foreach (var serializable in _serializedObjects)
        {
            if (serializable is SerializedGround serializedGround)
            {
                serializedGround.curvePoints = SerializeLevelUtility.GenerateCurvePointListFromGround(serializedGround);
                curvePointCount += serializedGround.curvePoints.Count;
                count++;
            }
        }

        Debug.Log($"Level {Name}: Populated {curvePointCount} curve points for {count} serialized grounds.");
    }

    public void PopulateSegmentCurvePoints()
    {
        Debug.Log($"Populating segment curve points for level: {Name}");
        var groundCount = 0;
        foreach (var serializable in _serializedObjects)
        {
            if (serializable is SerializedGround serializedGround)
            {
                Debug.Log($"Populating segments for serialized ground: {serializedGround.name}");

                if(serializedGround.curvePoints == null || serializedGround.curvePoints.Count == 0)
                {
                    Debug.LogWarning($"Serialized ground {serializedGround.name} has no curve points. Skipping segment population.");
                    continue;
                }

                SerializeLevelUtility.SerializeGroundSegments(serializedGround);
                groundCount++;

                Debug.Log($"Populated {serializedGround.segmentList.Count} runtime segments.");
            }
        }
    }

    private float GetKillPlaneY(List<Ground> grounds)
    {
        float lowY = float.PositiveInfinity;
        foreach (var ground in grounds)
        {
            foreach (var segment in ground.SegmentList)
            {
                var newY = segment.transform.TransformPoint(segment.LowPoint.position).y;
                if (newY < lowY)
                {
                    lowY = newY;
                }
            }
        }

        return lowY - 10;

    }
}
