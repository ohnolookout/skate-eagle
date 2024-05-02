using System.Collections.Generic;
using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;
//Single instance accessed by GameManager that maintains all data for players' current session
//Combines player records from SaveData with level list from ScriptableObject into single dictionary
public class SessionData
{

    private SerializedDictionary<string, PlayerRecord> _recordDict = new();
    private List<PlayerRecord> _dirtyRecords = new();
    private Dictionary<Medal, int> _medalCount = new()
    {
        { Medal.Red, 0 },
        { Medal.Blue, 0 },
        { Medal.Gold, 0 },
        { Medal.Silver, 0 },
        { Medal.Bronze, 0 },
        { Medal.Participant, 0}
    };
    private Dictionary<string, LevelNode> nodeDict = new();
    public int GoldPlusCount => _medalCount[Medal.Gold] + _medalCount[Medal.Blue] + _medalCount[Medal.Red];
    public Dictionary<Medal, int> MedalCount => _medalCount;
    public SerializedDictionary<string, PlayerRecord> RecordDict => _recordDict;
    public List<PlayerRecord> DirtyRecords => _dirtyRecords;
    public Dictionary<string, LevelNode> NodeDict => nodeDict;

    public SessionData(SaveData loadedGame)
    {
        LevelList levelList = Resources.Load<LevelList>("Level List");
        levelList.Build();
        nodeDict = levelList.levelNodeDict;
        _dirtyRecords = new();
        BuildRecordsAndMedals(loadedGame, levelList.levelNodes[0]);
    }

    //Adds records to dictionary for any nodes that don't currently exist in recordDict
    //Builds medal count for each record
    //Ignores records that don't have nodes, which means they are editor levels
    public void BuildRecordsAndMedals(SaveData loadedGame, LevelNode firstNode)
    {
        LevelNode currentNode = firstNode;
        _recordDict = loadedGame.recordDict;
        _dirtyRecords = loadedGame.dirtyRecords;
        //Set first node to incomplete if it's currently locked (only happens at new game).
        if(!_recordDict.ContainsKey(firstNode.levelUID))
        {
            AddLevelToRecords(currentNode.level);
            Record(currentNode.levelUID).status = CompletionStatus.Incomplete;
            currentNode = currentNode.next;
        }
        CompletionStatus lastRecordStatus = CompletionStatus.Complete;
        //Advance through nodes using next
        while(currentNode != null)
        {
            PlayerRecord record;
            //Access record if it exists, create new record if not.
            if (_recordDict.ContainsKey(currentNode.levelUID))
            {
                record = Record(currentNode.levelUID);
            }
            else
            {
                record = AddLevelToRecords(currentNode.Level);
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
            currentNode = currentNode.next;
        }
    }

    public PlayerRecord AddLevelToRecords(Level level)
    {
        _recordDict[level.levelUID] = new PlayerRecord(level);
        return _recordDict[level.levelUID];
    }

    public bool UpdateRecord(FinishScreenData finishData, Level level)
    {
        return Record(level.levelUID).Update(finishData, this);
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
        if (record.medal != Medal.Participant)
        {
            record.status = CompletionStatus.Complete;
            return record.status;
        }
        if (PreviousLevelNode(record.levelUID) == null)
        {
            record.status = CompletionStatus.Incomplete;
            return record.status;
        }
        if (PreviousLevelRecord(record.levelUID).status == CompletionStatus.Complete
            && Node(record.levelUID).goldRequired <= GoldPlusCount)
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
        if (Record(level.levelUID).status != CompletionStatus.Complete || NextLevelNode(level.levelUID) == null)
        {
            return false;
        }
        PlayerRecord nextLevelRecord = NextLevelRecord(level.levelUID);
        if (nextLevelRecord.status != CompletionStatus.Locked)
        {
            return true;
        }
        RefreshRecordStatus(nextLevelRecord);
        return nextLevelRecord.status != CompletionStatus.Locked;
    }

    public PlayerRecord Record(string UID)
    {
        if (_recordDict.ContainsKey(UID))
        {
            return _recordDict[UID];
        }
        Debug.LogError("No record found. Call record using level to create new record.");
        return null;
    }

    public PlayerRecord Record(Level level)
    {
        if (_recordDict.ContainsKey(level.levelUID))
        {
            return _recordDict[level.levelUID];
        }
        return AddLevelToRecords(level);
    }

    public LevelNode Node(string UID)
    {
        if (nodeDict.ContainsKey(UID))
        {
            return nodeDict[UID];
        }
        return null;
    }

    public PlayerRecord PreviousLevelRecord(string UID)
    {
        LevelNode previousNode = PreviousLevelNode(UID);
        return previousNode != null ? Record(previousNode.levelUID) : null;
    }

    public PlayerRecord NextLevelRecord(string UID)
    {
        LevelNode nextNode = NextLevelNode(UID);
        return nextNode != null ? Record(nextNode.levelUID) : null;
    }

    public LevelNode PreviousLevelNode(string UID)
    {
        LevelNode node = Node(UID);
        return node != null ? node.previous : null;
    }

    public LevelNode NextLevelNode(string UID)
    {
        LevelNode node = Node(UID);
        return node != null ? node.next : null;
    }

    public void PrintDictionaries()
    {
        Debug.Log("recordDict:");
        foreach(var record in _recordDict)
        {
            Debug.Log($"Key: {record.Key} Value: {record.Value}");
        }

        Debug.Log("nodeDict:");
        foreach (var node in nodeDict)
        {
            Debug.Log($"Key: {node.Key} Value: {node.Value}");
        }
    }
}
