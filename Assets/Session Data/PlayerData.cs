using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PlayerData
{
    public Dictionary<string, LevelTimeData> levelTimeDict = new();
    public Dictionary<Medal, int> medalCount = new();
    public bool dirty = false;

    public PlayerData(SaveData loadedGame)
    {
        BuildLevelTimeDictionary(loadedGame);
        BuildMedalCount();
    }


    public void BuildLevelTimeDictionary(SaveData loadedGame)
    {
        foreach(LevelTimeData levelTime in loadedGame.levelTimes)
        {
            levelTimeDict[levelTime.levelName] = levelTime;
        }
    }

    public void AddLevel(Level level)
    {
        if (levelTimeDict.ContainsKey(level.Name))
        {
            return;
        }
        levelTimeDict[level.Name] = new LevelTimeData(level);
    }

    public void AddLevelTime(Level level, float timeInSeconds)
    {
        if (levelTimeDict.ContainsKey(level.Name)){
            levelTimeDict[level.Name].UpdateTime(timeInSeconds, out Medal? newMedal, out Medal? oldMedal);
            if(newMedal != oldMedal)
            {
                AdjustMedalCount((Medal)newMedal, (Medal)oldMedal);
            }
        }
        else
        {
            levelTimeDict[level.Name] = new LevelTimeData(level, timeInSeconds);
        }
    }

    public LevelTimeData[] ExportLevelTimeList()
    {
        return levelTimeDict.Values.ToArray();
    }

    public void BuildMedalCount()
    {
        medalCount = new();
        Medal[] medals = (Medal[])Enum.GetValues(typeof(Medal));
        foreach(Medal medal in medals)
        {
            medalCount[medal] = 0;
        }
        foreach(LevelTimeData levelTime in levelTimeDict.Values)
        {
            if(levelTime.bestTime is null)
            {
                break;
            }
            Medal levelMedal = levelTime.level.MedalFromTime((float)levelTime.bestTime);
            medalCount[levelMedal]++;
        }
    }

    public void AdjustMedalCount(Medal medalToAdd, Medal medalToSubtract)
    {
        medalCount[medalToAdd]++;
        medalCount[medalToSubtract]--;
    }

    public void Clear()
    {
        levelTimeDict = null;
        medalCount = null;
    }
}
