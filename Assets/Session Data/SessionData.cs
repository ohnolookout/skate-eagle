using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class SessionData
{
    public Dictionary<string, PlayerRecord> playerRecordsDict = new();
    public Dictionary<Medal, int> medalCount = new();
    public bool dirty = false;

    public SessionData(SaveData loadedGame)
    {
        BuildLevelRecordDictionary(loadedGame);
        BuildMedalCount();
    }


    public void BuildLevelRecordDictionary(SaveData loadedGame)
    {
        foreach(PlayerRecord playerRecord in loadedGame.playerRecord)
        {
            playerRecordsDict[playerRecord.levelName] = playerRecord;
        }
    }

    public void AddLevel(Level level)
    {
        if (playerRecordsDict.ContainsKey(level.name))
        {
            return;
        }
        playerRecordsDict[level.name] = new PlayerRecord(level);
    }

    public void UpdateLevelRecords(FinishScreenData finishData, Level level)
    {
        if(finishData.finishType == FinishScreenType.Participant)
        {
            return;
        }
        if(finishData.finishType == FinishScreenType.NewMedal)
        {
            AdjustMedalCount(finishData.medal, playerRecordsDict[level.name].medal);
            playerRecordsDict[level.name].medal = finishData.medal;
        }
        playerRecordsDict[level.name].bestTime = finishData.attemptTime;
    }

    public PlayerRecord[] ExportLevelRecordList()
    {
        return playerRecordsDict.Values.ToArray();
    }

    public void BuildMedalCount()
    {
        medalCount = new();
        Medal[] medals = (Medal[])Enum.GetValues(typeof(Medal));
        foreach(Medal medal in medals)
        {
            medalCount[medal] = 0;
        }
        foreach(PlayerRecord levelTime in playerRecordsDict.Values)
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
        playerRecordsDict = null;
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
