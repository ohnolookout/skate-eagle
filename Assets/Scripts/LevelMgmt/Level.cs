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
    [SerializeField] private SerializedStartLine _serializedStartLine;
    public string UID { get => _UID; set => _UID = value; }
    public string Name { get => _name; set => _name = value; }
    public MedalTimes MedalTimes { get => _medalTimes; set => _medalTimes = value; }
    public List<IDeserializable> SerializedObjects { get => _serializedObjects; set => _serializedObjects = value; }
    public string LeaderboardKey { get => _leaderboardKey; set => _leaderboardKey = value; }
    public int GoldRequired => _goldRequired;
    public float KillPlaneY { get => _killPlaneY; set => _killPlaneY = value; }
    public SerializedStartLine SerializedStartLine => _serializedStartLine;

    public Level(string name, MedalTimes medalTimes, GroundManager groundManager)
    {
        _name = name;
        _medalTimes = medalTimes;
        _serializedObjects = SerializeLevelUtility.SerializeGroundManager(groundManager, out _serializedStartLine);
        _leaderboardKey = _name + "_leaderboard";
        _killPlaneY = GetKillPlaneY(groundManager);
    }
    public Level(string name)
    {
        _name = name;
        _medalTimes = new();
        _serializedObjects = new();
        _leaderboardKey = _name + "_leaderboard";
        _killPlaneY = 0;
    }

    private float GetKillPlaneY(GroundManager groundManager)
    {
        Ground[] grounds = groundManager.groundContainer.GetComponentsInChildren<Ground>();
        float lowY = float.PositiveInfinity;
        foreach (var ground in grounds)
        {
            foreach (var curvePointObj in ground.CurvePointObjects)
            {
                var newY = curvePointObj.transform.position.y;
                if (newY < lowY)
                {
                    lowY = newY;
                }
            }
        }

        return lowY - 10;

    }

}
