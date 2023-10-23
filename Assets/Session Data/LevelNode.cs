using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LevelNode
{
    public LevelNode previous = null, next = null;
    public Level level;
    public int goldRequired, order;

    public LevelNode(Level level, int goldRequired = 0)
    {
        this.level = level;
        this.goldRequired = goldRequired;
        previous = null;
        next = null;
    }
    public LevelNode(CompletionStatus status, Level level, int goldRequired = 0)
    {
        this.level = level;
        this.goldRequired = goldRequired;
        previous = null;
        next = null;
    }


    public string Name
    {
        get
        {
            return level.Name;
        }
    }

    public string UID
    {
        get
        {
            return level.UID;
        }
    }

    public Level Level
    {
        get
        {
            return level;
        }
    }
}
