using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public enum LevelNodeStatus { Complete, Incomplete, Locked, Default}
[Serializable]
public class LevelNode
{
    public LevelNode previous, next;
    public LevelNodeStatus status = LevelNodeStatus.Default;
    public Level level;
    public int goldRequired;

    public LevelNode(Level level, int goldRequired = 0)
    {
        this.level = level;
        this.goldRequired = goldRequired;
        status = LevelNodeStatus.Default;
        previous = null;
        next = null;
    }
    public LevelNode(LevelNodeStatus status, Level level, int goldRequired = 0)
    {
        this.status = status;
        this.level = level;
        this.goldRequired = goldRequired;
        previous = null;
        next = null;
    }

    public bool IsNextNodeUnlocked()
    {
        if (next == null || status != LevelNodeStatus.Complete)
        {
            Debug.Log($"next node null: {next == null} Current node status: {status}");
            return false;
        }
        if (next.status != LevelNodeStatus.Locked)
        {
            return true;
        }
        next.GenerateStatus();
        return next.status != LevelNodeStatus.Locked;
    }

    public LevelNodeStatus GenerateStatus()
    {
        LevelRecords records = LevelDataManager.Instance.RecordFromLevel(level.Name);
        if (records != null)
        {
            if (!Single.IsPositiveInfinity(records.bestTime))
            {
                status = LevelNodeStatus.Complete;
            }
            else
            {
                status = LevelNodeStatus.Incomplete;
            }
        } else if (previous == null)
        {
            status = LevelNodeStatus.Incomplete;
        }
        else if (previous.status == LevelNodeStatus.Locked)
        {
            status = LevelNodeStatus.Locked;
        }
        else if (previous.status == LevelNodeStatus.Complete && LevelDataManager.Instance.sessionData.GoldPlusCount >= goldRequired)
        {
            status = LevelNodeStatus.Incomplete;
        }
        else
        {
            status = LevelNodeStatus.Locked;
        }
        return status;
    }
}
