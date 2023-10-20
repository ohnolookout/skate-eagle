using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class SessionData
{
    public Dictionary<string, LevelRecords> levelRecordsDict = new();
    public Dictionary<Medal, int> medalCount = new();
    public bool dirty = false;

    public SessionData(SaveData loadedGame)
    {
        BuildLevelRecordDictionary(loadedGame);
        BuildMedalCount();
    }


    public void BuildLevelRecordDictionary(SaveData loadedGame)
    {
        foreach(LevelRecords levelRecords in loadedGame.levelRecords)
        {
            levelRecordsDict[levelRecords.levelName] = levelRecords;
        }
    }

    public void AddLevel(Level level)
    {
        if (levelRecordsDict.ContainsKey(level.name))
        {
            return;
        }
        levelRecordsDict[level.name] = new LevelRecords(level);
    }

    public void UpdateLevelRecords(FinishScreenData finishData, Level level)
    {
        if(finishData.finishType == FinishScreenType.Participant)
        {
            return;
        }
        if(finishData.finishType == FinishScreenType.NewMedal)
        {
            AdjustMedalCount(finishData.medal, levelRecordsDict[level.name].medal);
            levelRecordsDict[level.name].medal = finishData.medal;
        }
        levelRecordsDict[level.name].bestTime = finishData.attemptTime;
    }

    public LevelRecords[] ExportLevelRecordList()
    {
        return levelRecordsDict.Values.ToArray();
    }

    public void BuildMedalCount()
    {
        medalCount = new();
        Medal[] medals = (Medal[])Enum.GetValues(typeof(Medal));
        foreach(Medal medal in medals)
        {
            medalCount[medal] = 0;
        }
        foreach(LevelRecords levelTime in levelRecordsDict.Values)
        {
            if (Single.IsPositiveInfinity(levelTime.bestTime))
            {
                break;
            }
            medalCount[levelTime.medal]++;
        }
    }

    public void AdjustMedalCount(Medal medalToAdd, Medal medalToSubtract)
    {
        medalCount[medalToAdd]++;
        medalCount[medalToSubtract]--;
    }

    public void Clear()
    {
        levelRecordsDict = null;
        medalCount = null;
    }

    public void PrintMedalCount()
    {
        Medal[] medals = (Medal[])Enum.GetValues(typeof(Medal));
        foreach (Medal medal in medals)
        {
            Debug.Log($"{medal} medals: {medalCount[medal]}");
        }
    }

    public int GoldPlusCount
    {
        get
        {
            return medalCount[Medal.Gold] + medalCount[Medal.Blue] + medalCount[Medal.Red];
        }
    }
}
