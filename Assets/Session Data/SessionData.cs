using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;
//Single instance accessed by GameManager that maintains all data for players' current session
//Combines player records from SaveData with level list from ScriptableObject into single dictionary
public class SessionData
{
    private SerializedDictionary<string, PlayerRecord> recordDict = new();
    private Dictionary<Medal, int> medalCount = new()
    {
        { Medal.Red, 0 },
        { Medal.Blue, 0 },
        { Medal.Gold, 0 },
        { Medal.Silver, 0 },
        { Medal.Bronze, 0 },
        { Medal.Participant, 0}
    };
    private Dictionary<string, LevelNode> nodeDict = new();

    public SessionData(SaveData loadedGame)
    {
        LevelList levelList = Resources.Load<LevelList>("Level List");
        levelList.Build();
        nodeDict = levelList.levelNodeDict;
        BuildRecordsAndMedals(loadedGame, levelList.levelNodes[0]);
    }

    //Adds records to dictionary for any nodes that don't currently exist in recordDict
    //Builds medal count for each record
    //Ignores records that don't have nodes, which means they are editor levels
    public void BuildRecordsAndMedals(SaveData loadedGame, LevelNode firstNode)
    {
        LevelNode currentNode = firstNode;
        recordDict = loadedGame.PlayerRecords();
        //Set first node to incomplete if it's currently locked (only happens at new game).
        if(!recordDict.ContainsKey(firstNode.UID))
        {
            AddLevelToRecords(currentNode.level);
            Record(currentNode.UID).status = CompletionStatus.Incomplete;
            currentNode = currentNode.next;
        }
        CompletionStatus lastRecordStatus = CompletionStatus.Complete;
        //Advance through nodes using next
        while(currentNode != null)
        {
            PlayerRecord record;
            //Access record if it exists, create new record if not.
            if (recordDict.ContainsKey(currentNode.UID))
            {
                record = Record(currentNode.UID);
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
                medalCount[record.medal]++;
            }
            lastRecordStatus = record.status;
            currentNode = currentNode.next;
        }
    }

    public PlayerRecord AddLevelToRecords(Level level)
    {
        Debug.Log("Adding level to records by Level");
        recordDict[level.UID] = new PlayerRecord(level);
        return recordDict[level.UID];
    }


    //UID PROBLEM STARTS HERE
    public PlayerRecord AddLevelToRecords(string UID)
    {
        Debug.Log("Adding level to records by UID");
        recordDict[UID] = new PlayerRecord(UID);
        return recordDict[UID];
    }

    public void UpdateRecord(FinishScreenData finishData, Level level)
    {
        Record(level.UID).Update(finishData, this);
    }

    public void AdjustMedalCount(Medal medalToAdd, Medal medalToSubtract)
    {
        medalCount[medalToAdd]++;
        medalCount[medalToSubtract]--;
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


    public CompletionStatus RefreshRecordStatus(PlayerRecord record)
    {
        Debug.Log("Refreshing record status. UID:" + record.UID + " Level name:" + record.levelName);
        if (record.medal != Medal.Participant)
        {
            record.status = CompletionStatus.Complete;
            return record.status;
        }
        if (PreviousLevelNode(record.UID) == null)
        {
            record.status = CompletionStatus.Incomplete;
            return record.status;
        }
        if (PreviousLevelRecord(record.UID).status == CompletionStatus.Complete
            && Node(record.UID).goldRequired <= GoldPlusCount)
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
        if (Record(level.UID).status != CompletionStatus.Complete || NextLevelNode(level.UID) == null)
        {
            return false;
        }
        PlayerRecord nextLevelRecord = NextLevelRecord(level.UID);
        if (nextLevelRecord.status != CompletionStatus.Locked)
        {
            return true;
        }
        RefreshRecordStatus(nextLevelRecord);
        return nextLevelRecord.status != CompletionStatus.Locked;
    }

    public PlayerRecord Record(string UID)
    {
        if (recordDict.ContainsKey(UID))
        {
            return recordDict[UID];
        }
        return AddLevelToRecords(UID);
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
        if (previousNode == null)
        {
            return null;
        }
        return Record(previousNode.UID);
    }

    public PlayerRecord NextLevelRecord(string UID)
    {
        LevelNode nextNode = NextLevelNode(UID);
        if (nextNode == null)
        {
            return null;
        }
        return Record(nextNode.UID);
    }

    public LevelNode PreviousLevelNode(string UID)
    {
        LevelNode node = Node(UID);
        if(node == null)
        {
            return null;
        }
        return node.previous;
    }

    public LevelNode NextLevelNode(string UID)
    {
        LevelNode node = Node(UID);
        if (node == null)
        {
            return null;
        }
        return node.next;
    }

    public Dictionary<Medal, int> MedalCount
    {
        get
        {
            return medalCount;
        }
    }

    public SerializedDictionary<string, PlayerRecord> RecordDict
    {
        get
        {
            return recordDict;
        }
    }

    public Dictionary<string, LevelNode> NodeDict
    {
        get
        {
            return nodeDict;
        }
    }

    public void PrintDictionaries()
    {
        Debug.Log("recordDict:");
        foreach(var record in recordDict)
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
