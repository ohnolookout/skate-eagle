using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using UnityEngine;

[Serializable]
public class Level
{
    [SerializeField] private string _UID = "";
    [SerializeField] private string _name;
    [SerializeField] private int _goldRequired = 0;
    [SerializeField] private MedalTimes _medalTimes;
    [SerializeField][HideInInspector] private List<SerializedGround> _serializedGrounds;
    [SerializeField] private string _leaderboardKey = "None";
    [SerializeField] private Vector2 _startPoint;
    [SerializeField] private float _killPlaneY;
    [SerializeField] private Vector2 _cameraStartPosition = new(-35, 15);
    [SerializeField] private FinishLineParameters _finishLineParameters;
    [SerializeField] private LinkedCameraTarget _rootCameraTarget;
    public string UID { get => _UID; set => _UID = value; }
    public string Name { get => _name; set => _name = value; }
    public MedalTimes MedalTimes { get => _medalTimes; set => _medalTimes = value; }
    public List<SerializedGround> SerializedGrounds => _serializedGrounds;
    public string LeaderboardKey { get => _leaderboardKey; set => _leaderboardKey = value; }
    public int GoldRequired => _goldRequired;
    public Vector2 StartPoint => _startPoint;
    public float KillPlaneY { get => _killPlaneY; set => _killPlaneY = value; }
    public Vector2 CameraStartPosition { get => _cameraStartPosition; set => _cameraStartPosition = value; }
    public FinishLineParameters FinishLineParameters {get => _finishLineParameters; set => _finishLineParameters = value; }
    public LinkedCameraTarget RootCameraTarget { get => _rootCameraTarget; set => _rootCameraTarget = value; }

    public Level(string name, MedalTimes medalTimes, Ground[] grounds, Vector2 startPoint = new(), Vector2 cameraStartPosition = new(), float killPlaneY = -100, FinishLine finishLine = null, LinkedCameraTarget rootCameraTarget = null)
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
}
