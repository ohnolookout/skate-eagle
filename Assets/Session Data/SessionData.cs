using System.Collections.Generic;
using System.Linq;
using System;

public class SessionData
{
    public Dictionary<string, LevelRecords> levelTimeDict = new();
    public Dictionary<Medal, int> medalCount = new();
    public bool dirty = false;

    public SessionData(SaveData loadedGame)
    {
        BuildLevelRecordDictionary(loadedGame);
        BuildMedalCount();
    }


    public void BuildLevelRecordDictionary(SaveData loadedGame)
    {
        foreach(LevelRecords levelRecords in loadedGame.levelTimes)
        {
            levelTimeDict[levelRecords.levelName] = levelRecords;
        }
    }

    public void AddLevel(Level level)
    {
        if (levelTimeDict.ContainsKey(level.name))
        {
            return;
        }
        levelTimeDict[level.name] = new LevelRecords(level);
    }

    public void UpdateLevelRecords(FinishScreenData finishData, Level level)
    {
        if(finishData.finishType == FinishScreenType.Participant)
        {
            return;
        }
        if(finishData.finishType == FinishScreenType.NewMedal)
        {
            AdjustMedalCount(finishData.medal, levelTimeDict[level.name].medal);
            levelTimeDict[level.name].medal = finishData.medal;
        }
        levelTimeDict[level.name].bestTime = finishData.attemptTime;
    }

    public LevelRecords[] ExportLevelRecordList()
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
        foreach(LevelRecords levelTime in levelTimeDict.Values)
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
        levelTimeDict = null;
        medalCount = null;
    }
}
