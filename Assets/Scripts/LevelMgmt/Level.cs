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
    [SerializeField][HideInInspector] private List<IDeserializable> _serializedObjects;
    [SerializeField] private string _leaderboardKey = "None";
    [SerializeField] private Vector2 _startPoint;
    [SerializeField] private float _killPlaneY;
    [SerializeField] private Vector2 _cameraStartPosition = new(-35, 15);
    [SerializeField] private SerializedFinishLine _finishLineParameters;
    [SerializeField] private LinkedCameraTarget _rootCameraTarget;
    public string UID { get => _UID; set => _UID = value; }
    public string Name { get => _name; set => _name = value; }
    public MedalTimes MedalTimes { get => _medalTimes; set => _medalTimes = value; }
    public List<SerializedGround> SerializedGrounds => _serializedGrounds;
    public List<IDeserializable> SerializedObjects => _serializedObjects ??= new List<IDeserializable>();
    public string LeaderboardKey { get => _leaderboardKey; set => _leaderboardKey = value; }
    public int GoldRequired => _goldRequired;
    public Vector2 StartPoint => _startPoint;
    public float KillPlaneY { get => _killPlaneY; set => _killPlaneY = value; }
    public Vector2 CameraStartPosition { get => _cameraStartPosition; set => _cameraStartPosition = value; }
    public SerializedFinishLine SerializedFinishLine {get => _finishLineParameters; set => _finishLineParameters = value; }
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
            _finishLineParameters = finishLine.Parameters;
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
            _finishLineParameters = groundManager.FinishLine.Parameters;
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

    public Level(Level level)
    {
        _UID = level.UID;
        _name = level.Name;
        _medalTimes = level.MedalTimes;
        _serializedGrounds = new List<SerializedGround>(level.SerializedGrounds);
        _leaderboardKey = level.LeaderboardKey;
        _startPoint = level.StartPoint;
        _cameraStartPosition = level.CameraStartPosition;
        _killPlaneY = level.KillPlaneY;
        _finishLineParameters = level.SerializedFinishLine;
        _rootCameraTarget = level.RootCameraTarget;
    }
}
