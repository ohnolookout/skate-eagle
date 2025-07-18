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
    [SerializeField] private Vector2 _startPoint;
    [SerializeField] private float _killPlaneY;
    [SerializeField] private Vector2 _cameraStartPosition = new(-35, 15);
    [SerializeField] private LinkedCameraTarget _rootCameraTarget;
    public string UID { get => _UID; set => _UID = value; }
    public string Name { get => _name; set => _name = value; }
    public MedalTimes MedalTimes { get => _medalTimes; set => _medalTimes = value; }
    public List<IDeserializable> SerializedObjects => _serializedObjects;
    public string LeaderboardKey { get => _leaderboardKey; set => _leaderboardKey = value; }
    public int GoldRequired => _goldRequired;
    public Vector2 StartPoint => _startPoint;
    public float KillPlaneY { get => _killPlaneY; set => _killPlaneY = value; }
    public Vector2 CameraStartPosition { get => _cameraStartPosition; set => _cameraStartPosition = value; }
    public LinkedCameraTarget RootCameraTarget { get => _rootCameraTarget; set => _rootCameraTarget = value; }

    public Level(string name, MedalTimes medalTimes, GroundManager groundManager, Vector2 startPoint = new(), Vector2 cameraStartPosition = new(), float killPlaneY = -100, LinkedCameraTarget rootCameraTarget = null, string UID = null)
    {
        _name = name;
        _medalTimes = medalTimes;

        _serializedObjects = SerializeLevelUtility.SerializeGroundManager(groundManager);

        _rootCameraTarget = rootCameraTarget;
        _leaderboardKey = _name + "_leaderboard";
        _startPoint = startPoint;
        _cameraStartPosition = cameraStartPosition;
        _killPlaneY = killPlaneY;
    }
    public Level(string name, MedalTimes medalTimes, List<IDeserializable> serializedObjects, Vector2 startPoint = new(), Vector2 cameraStartPosition = new(), float killPlaneY = -100, LinkedCameraTarget rootCameraTarget = null, string UID = null)
    {
        _name = name;
        _medalTimes = medalTimes;
        _serializedObjects = serializedObjects;
        _rootCameraTarget = rootCameraTarget;
        _leaderboardKey = _name + "_leaderboard";
        _startPoint = startPoint;
        _cameraStartPosition = cameraStartPosition;
        _killPlaneY = killPlaneY;
    }

    public Level(Level level)
    {
        _UID = level.UID;
        _name = level.Name;
        _medalTimes = level.MedalTimes; 
        _serializedObjects = level.SerializedObjects;
        _leaderboardKey = level.LeaderboardKey;
        _startPoint = level.StartPoint;
        _cameraStartPosition = level.CameraStartPosition;
        _killPlaneY = level.KillPlaneY;
        _rootCameraTarget = level.RootCameraTarget;
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

    public void PopulateGroundCurvePoints()
    {
        var count = 0;
        foreach(var serializable in _serializedObjects)
        {
            if (serializable is SerializedGround serializedGround)
            {
                serializedGround.curvePointList = SerializeLevelUtility.GenerateCurvePointListFromGround(serializedGround);
                count++;
            }
        }

        Debug.Log($"Level {Name}: Populated curve points for {count} serialized grounds.");
    }
}
