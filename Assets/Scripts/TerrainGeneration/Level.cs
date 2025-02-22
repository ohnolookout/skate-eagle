using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using UnityEngine;

[Serializable]
public class Level
{
    [SerializeField] private string _UID;
    [SerializeField] private string _name;
    [SerializeField] private MedalTimes _medalTimes;
    [SerializeField] private List<SerializedGround> _serializedGrounds;
    [SerializeField] private string _leaderboardKey = "None";
    public string UID { get => _UID; set => _UID = value; }
    public string Name { get => _name; set => _name = value; }
    public MedalTimes MedalTimes { get => _medalTimes; set => _medalTimes = value; }
    public List<SerializedGround> SerializedGrounds => _serializedGrounds;
    public string LeaderboardKey { get => _leaderboardKey; set => _leaderboardKey = value; }

    public Level(string name, MedalTimes medalTimes, Ground[] grounds)
    {
        _name = name;
        _medalTimes = medalTimes;
        _serializedGrounds = SerializeLevelUtility.SerializeGroundList(grounds);
        _leaderboardKey = _name + "_leaderboard";

        if (string.IsNullOrWhiteSpace(_UID))
        {
            Debug.Log("GUI created");
            _UID = Guid.NewGuid().ToString();
        }
    }
}
