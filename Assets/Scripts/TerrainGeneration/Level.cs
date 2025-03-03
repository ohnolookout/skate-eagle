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
    [SerializeField] private Vector2 _finishPoint;
    public string UID { get => _UID; set => _UID = value; }
    public string Name { get => _name; set => _name = value; }
    public MedalTimes MedalTimes { get => _medalTimes; set => _medalTimes = value; }
    public List<SerializedGround> SerializedGrounds => _serializedGrounds;
    public string LeaderboardKey { get => _leaderboardKey; set => _leaderboardKey = value; }
    public bool DoPublish => _doPublish;
    public int GoldRequired => _goldRequired;
    public Vector2 StartPoint => _startPoint;
    public Vector2 FinishPoint => _finishPoint;

    public Level(string name, MedalTimes medalTimes, Ground[] grounds, Vector2 startPoint = new(), Vector2 finishPoint = new())
    {
        _name = name;
        _medalTimes = medalTimes;
        _serializedGrounds = SerializeLevelUtility.SerializeGroundList(grounds);
        _leaderboardKey = _name + "_leaderboard";
        _startPoint = startPoint;
        _finishPoint = finishPoint;

    }
}
