using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
//Single instance accessed by GameManager that maintains all data for players' current session
//Combines player records from SaveData with level list from ScriptableObject into single dictionary
public class SessionData
{
    private SaveData _saveData;
    private LevelDatabase _levelDB;
    private Dictionary<Medal, int> _medalCount = new()
    {
        { Medal.Red, 0 },
        { Medal.Blue, 0 },
        { Medal.Gold, 0 },
        { Medal.Silver, 0 },
        { Medal.Bronze, 0 },
        { Medal.Participant, 0}
    };
    //private Dictionary<string, LevelNode> nodeDict = new();
    public int GoldPlusCount => _medalCount[Medal.Gold] + _medalCount[Medal.Blue] + _medalCount[Medal.Red];
    public Dictionary<Medal, int> MedalCount => _medalCount;
    public SaveData SaveData => _saveData;    
    public LevelDatabase LevelDB => _levelDB;

    public SessionData(SaveData loadedGame)
    {
        _saveData = loadedGame;
        _levelDB = Resources.Load<LevelDatabase>("LevelDB"); ;
        //nodeDict = levelList.levelNodeDict;
        BuildRecordsAndMedals(loadedGame);
    }

    //Adds records to dictionary for any nodes that don't currently exist in recordDict
    //Builds medal count for each record
    //Ignores records that don't have nodes, which means they are editor levels
    public void BuildRecordsAndMedals(SaveData loadedGame)
    {
        Level currentLevel = _levelDB.GetLevelByIndex(0);

        if(currentLevel == null)
        {
            return;
        }

        //Set first node to incomplete if it's currently locked (only happens at new game).
        if (!_saveData.recordDict.ContainsKey(currentLevel.UID))
        {
            var newRecord = AddLevelToRecords(currentLevel);
            newRecord.status = CompletionStatus.Incomplete;
            currentLevel = _levelDB.GetNextLevel(currentLevel);
        }

        CompletionStatus lastRecordStatus = CompletionStatus.Complete;

        //Advance through nodes using next
        while(currentLevel != null)
        {
            PlayerRecord record;

            //Access record if it exists, create new record if not.
            if (_saveData.recordDict.ContainsKey(currentLevel.UID))
            {
                record = GetRecordByUID(currentLevel.UID);
            }
            else
            {
                record = AddLevelToRecords(currentLevel);
            }
            //Update record status only if last record is complete.
            if (lastRecordStatus == CompletionStatus.Complete && record.status == CompletionStatus.Locked)
            {
                RefreshRecordStatus(record);
            }
            //Add medal from record if complete.
            if (record.status == CompletionStatus.Complete)
            {
                _medalCount[record.medal]++;
            }
            lastRecordStatus = record.status;
            currentLevel = _levelDB.GetNextLevel(currentLevel);
        }
    }

    public PlayerRecord AddLevelToRecords(Level level)
    {
        _saveData.recordDict[level.UID] = new PlayerRecord(level);
        return _saveData.recordDict[level.UID];
    }

    public bool UpdateRecord(FinishData finishData, Level level)
    {
        return GetRecordByUID(level.UID).Update(finishData, this);
    }

    public void AdjustMedalCount(Medal medalToAdd, Medal medalToSubtract)
    {
        _medalCount[medalToAdd]++;
        _medalCount[medalToSubtract]--;
    }

    public void PrintMedalCount()
    {
        Medal[] medals = (Medal[])Enum.GetValues(typeof(Medal));
        foreach (Medal medal in medals)
        {
            Debug.Log($"{medal} medals: {_medalCount[medal]}");
        }
    }

    public CompletionStatus RefreshRecordStatus(PlayerRecord record)
    {
        //Set status to complete if medal is higher than participant
        if (record.medal != Medal.Participant)
        {
            record.status = CompletionStatus.Complete;
            return record.status;
        }

        //If there is no previous level, set to incomplete
        var previousLevel = _levelDB.GetPreviousLevel(record.levelName);
        if (previousLevel == null)
        {
            record.status = CompletionStatus.Incomplete;
            return record.status;
        }

        var previousRecord = GetRecordByUID(previousLevel.UID);
        if(previousRecord == null)
        {
            record.status = CompletionStatus.Incomplete;
            return record.status;
        }
        
        //If previous level is complete and player has enough gold, set to incomplete. Otherwise, set locked.
        if (previousRecord.status == CompletionStatus.Complete
            && _levelDB.GetLevelByUID(record.levelUID).GoldRequired <= GoldPlusCount)
        {
            record.status = CompletionStatus.Incomplete;
        } else
        {
            record.status = CompletionStatus.Locked;
        }

        return record.status;
    }

    public bool NextLevelUnlocked(Level level)
    {
        var nextLevel = _levelDB.GetNextLevel(level);
        if (GetRecordByUID(level.UID).status != CompletionStatus.Complete || nextLevel == null)
        {
            return false;
        }

        PlayerRecord nextLevelRecord = GetRecordByUID(nextLevel.UID);
        if (nextLevelRecord == null)
        {
            return false;
        }

        if (nextLevelRecord.status != CompletionStatus.Locked)
        {
            return true;
        }

        RefreshRecordStatus(nextLevelRecord);
        return nextLevelRecord.status != CompletionStatus.Locked;
    }

    public PlayerRecord GetRecordByUID(string UID)
    {
        if (_saveData.recordDict.ContainsKey(UID))
        {
            return _saveData.recordDict[UID];
        }
        Debug.LogError("No record found. Call record using level to create new record.");
        return null;
    }

    public PlayerRecord GetRecordByLevel(Level level)
    {
        if (_saveData.recordDict.ContainsKey(level.UID))
        {
            return _saveData.recordDict[level.UID];
        }
        return AddLevelToRecords(level);
    }

    public PlayerRecord PreviousLevelRecord(string UID)
    {
        var currentLevel = _levelDB.GetLevelByUID(UID);
        var previouslevel = _levelDB.GetPreviousLevel(currentLevel);
        return previouslevel != null ? GetRecordByUID(previouslevel.UID) : null;
    }

}
