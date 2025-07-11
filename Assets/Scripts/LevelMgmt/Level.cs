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
    [SerializeField][HideInInspector] private List<SerializedGround> _serializedGrounds;
    [SerializeReference][HideInInspector] private List<IDeserializable> _serializedObjects;
    [SerializeField] private string _leaderboardKey = "None";
    [SerializeField] private Vector2 _startPoint;
    [SerializeField] private float _killPlaneY;
    [SerializeField] private Vector2 _cameraStartPosition = new(-35, 15);
    [SerializeField] private SerializedFinishLine _serializedFinishLine;
    [SerializeField] private LinkedCameraTarget _rootCameraTarget;
    public string UID { get => _UID; set => _UID = value; }
    public string Name { get => _name; set => _name = value; }
    public MedalTimes MedalTimes { get => _medalTimes; set => _medalTimes = value; }
    public List<SerializedGround> SerializedGrounds => _serializedGrounds;
    public List<IDeserializable> SerializedObjects => _serializedObjects;
    public string LeaderboardKey { get => _leaderboardKey; set => _leaderboardKey = value; }
    public int GoldRequired => _goldRequired;
    public Vector2 StartPoint => _startPoint;
    public float KillPlaneY { get => _killPlaneY; set => _killPlaneY = value; }
    public Vector2 CameraStartPosition { get => _cameraStartPosition; set => _cameraStartPosition = value; }
    public SerializedFinishLine SerializedFinishLine {get => _serializedFinishLine; set => _serializedFinishLine = value; }
    public LinkedCameraTarget RootCameraTarget { get => _rootCameraTarget; set => _rootCameraTarget = value; }

    public Level(string name, MedalTimes medalTimes, Ground[] grounds, Vector2 startPoint = new(), Vector2 cameraStartPosition = new(), float killPlaneY = -100, FinishLine finishLine = null, LinkedCameraTarget rootCameraTarget = null, string UID = null)
    {
        _name = name;
        _medalTimes = medalTimes;
        _serializedGrounds = SerializeLevelUtility.SerializeGroundList(grounds);
        _rootCameraTarget = rootCameraTarget;
        _leaderboardKey = _name + "_leaderboard";
        _startPoint = startPoint;
        _cameraStartPosition = cameraStartPosition;
        _killPlaneY = killPlaneY;

        if (finishLine != null)
        {
            _serializedFinishLine = finishLine.Parameters;
        }
    }

    public Level(string name, MedalTimes medalTimes, GroundManager groundManager, Vector2 startPoint = new(), Vector2 cameraStartPosition = new(), float killPlaneY = -100, LinkedCameraTarget rootCameraTarget = null, string UID = null)
    {
        _name = name;
        _medalTimes = medalTimes;

        _serializedObjects = SerializeLevelUtility.SerializeGroundManager(groundManager);

        var grounds = groundManager.groundContainer.GetComponentsInChildren<Ground>();
        _serializedGrounds = SerializeLevelUtility.SerializeGroundList(grounds);

        _rootCameraTarget = rootCameraTarget;
        _leaderboardKey = _name + "_leaderboard";
        _startPoint = startPoint;
        _cameraStartPosition = cameraStartPosition;
        _killPlaneY = killPlaneY;

        if (groundManager.FinishLine != null)
        {
            _serializedFinishLine = groundManager.FinishLine.Parameters;
        }
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

    //Rewrite method to refresh serializedgrounds to include serializedobjectlists.
    public Level(Level level)
    {
        _UID = level.UID;
        _name = level.Name;
        _medalTimes = level.MedalTimes;
        _serializedGrounds = level.SerializedGrounds;        
        _serializedObjects = level.SerializedObjects;
        _leaderboardKey = level.LeaderboardKey;
        _startPoint = level.StartPoint;
        _cameraStartPosition = level.CameraStartPosition;
        _killPlaneY = level.KillPlaneY;
        _serializedFinishLine = level.SerializedFinishLine;
        _rootCameraTarget = level.RootCameraTarget;
    }
    public void Reserialize()
    {
        Debug.Log("Reserializing Level: " + _name);
        if (_serializedObjects != null)
        {
            Debug.Log("Serialized objects at start: " + _serializedObjects.Count);
        } else
        {
            Debug.Log("Serialized objects is null, initializing new list.");
        }
        _serializedObjects = new();

        foreach(var ground in _serializedGrounds)
        {
            if (ground.serializedObjectList == null)
            {
                ground.serializedObjectList = new();
            }

            foreach (var serializedSegment in ground.segmentList)
            {
                if (ground.serializedObjectList.Contains(serializedSegment))
                {
                    continue;
                }

                ground.serializedObjectList.Add(serializedSegment);
            }

            _serializedObjects.Add(ground);
        }

        Debug.Log("Adding serialized finish line to serialized objects.");
        Debug.Log("Flag position: " + _serializedFinishLine.flagPosition);
        Debug.Log("Backstop position: " + _serializedFinishLine.backstopPosition);

        _serializedObjects.Add(_serializedFinishLine);

        Debug.Log("Serialized objects after reserialize: " + _serializedObjects.Count);
    }
}
