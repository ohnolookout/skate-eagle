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
    [SerializeField] private bool _doPublish = false;
    [SerializeField] private int _goldRequired = 0;
    [SerializeField] private MedalTimes _medalTimes;
    [SerializeField] private List<SerializedGround> _serializedGrounds;
    [SerializeField] private string _leaderboardKey = "None";
    [SerializeField] private Vector2 _startPoint;
    [SerializeField] private float _killPlaneY;
    [SerializeField] private Vector2 _cameraStartPosition = new(-35, 15);
    [SerializeField] private Vector2[] _finishLineParameters;
    [SerializeField] private bool _backstopIsActive = true;
    [SerializeField] private LinkedCameraTarget _rootCameraTarget;
    public string UID { get => _UID; set => _UID = value; }
    public string Name { get => _name; set => _name = value; }
    public MedalTimes MedalTimes { get => _medalTimes; set => _medalTimes = value; }
    public List<SerializedGround> SerializedGrounds => _serializedGrounds;
    public string LeaderboardKey { get => _leaderboardKey; set => _leaderboardKey = value; }
    public bool DoPublish => _doPublish;
    public int GoldRequired => _goldRequired;
    public Vector2 StartPoint => _startPoint;
    public Vector2 FinishPoint => _finishLineParameters != null && _finishLineParameters.Length > 0 ? _finishLineParameters[0]: new Vector2(2000, 0);
    public float KillPlaneY { get => _killPlaneY; set => _killPlaneY = value; }
    public Vector2 CameraStartPosition { get => _cameraStartPosition; set => _cameraStartPosition = value; }
    public Vector2[] FinishLineParameters => _finishLineParameters;
    public bool BackstopIsActive => _backstopIsActive;
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
        _finishLineParameters = SerializeLevelUtility.SerializeFinishLine(finishLine);
        if (finishLine != null)
        {
            _backstopIsActive = finishLine.Backstop.activeInHierarchy;
        }
    }
}
