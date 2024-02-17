using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum CollisionType { Ground, OtherBody };
public class PendingExitManager
{
    private Dictionary<CollisionType, OrderedDictionary<string, TimedCollisionExit>> _pendingExits = new();
    private Dictionary<CollisionType, float> _exitTimers = new();
    public Action<TimedCollisionExit> RemoveCollision;
    private const float _groundUncollideTime = 0.15f, _bodyBoardUncollideTime = 0.5f;
    

    public PendingExitManager()
    {
        ResetAllExits();
        _exitTimers[CollisionType.Ground] = _groundUncollideTime;
        _exitTimers[CollisionType.OtherBody] = _bodyBoardUncollideTime;
    }

    public void ResetAllExits()
    {
        foreach (CollisionType type in Enum.GetValues(typeof(CollisionType)))
        {
            _pendingExits[type] = new();
        }
    }

    public void ResetExitsByType(CollisionType type)
    {
        _pendingExits[type] = new();
    }

    public void CheckAllExits()
    {
        foreach(var collisionType in _pendingExits.Keys.ToList())
        {
            CheckPendingExits(_pendingExits[collisionType], _exitTimers[collisionType]);
        }
    }

    public void RemovePendingExit(ColliderCategory category, string colliderName)
    {
        if (category == ColliderCategory.BodyAndBoard)
        {
            _pendingExits[CollisionType.OtherBody].Remove(colliderName);
        }
        else
        {
            _pendingExits[CollisionType.Ground].Remove(colliderName);
        }
    }

    public void AddPendingExit(CollisionType type, TimedCollisionExit collision)
    {
        _pendingExits[type].Add(collision.ColliderName, collision);
    }

    public void AddPendingExit(TimedCollisionExit collision)
    {
        CollisionType type = GetTypeFromCategory(collision.Category);
        _pendingExits[type].Add(collision.ColliderName, collision);
    }

    private CollisionType GetTypeFromCategory(ColliderCategory category)
    {
        if(category == ColliderCategory.BodyAndBoard)
        {
            return CollisionType.OtherBody;
        }
        return CollisionType.Ground;
    }

    private void CheckPendingExits(OrderedDictionary<string, TimedCollisionExit> pendingExits, float uncollideTime)
    {
        //While there are pending exits, continue checking the first pending exit
        //until one is found that has not exceeded the time limit
        while (pendingExits.Count > 0)
        {
            TimedCollisionExit collision = pendingExits.Value(0);
            bool earliestTimeRemoved = CheckExit(pendingExits, collision, uncollideTime);
            //If the first pending exit has not been removed, then later pending exits won't exceed the time limit either
            //so the function can stop cehcking.
            if (!earliestTimeRemoved)
            {
                break;
            }
        }
    }

    //CheckExit returns true if collision removed, false if not
    private bool CheckExit(OrderedDictionary<string, TimedCollisionExit> pendingExits, TimedCollisionExit collision, float uncollideTime)
    {
        if (Time.time - collision.Time < uncollideTime)
        {
            //If first collision in list does not exceed time limit, return false
            return false;
        }
        //If collision does exceed time limit, remove it from the list of colliders that are currently collided in its category
        //And remove it from the list of pending collisions.
        RemoveCollision?.Invoke(collision);
        pendingExits.Remove(0);
        return true;
        //If the current category has no more active collisions, remove it from the dictionary and send call to audio
        //If no categories are collided, send eagle into airborne mode
    }

    public void SetUncollideTimer(CollisionType type, float newTime)
    {
        _exitTimers[type] = newTime;
    }
    
    public float GetUncollideTimer(CollisionType type)
    {
        return _exitTimers[type];
    }

}
public struct TimedCollisionExit
{
    public ColliderCategory Category;
    public string ColliderName;
    public float Time;
    public float MagnitudeAtCollisionExit;

    public TimedCollisionExit(string colliderName, ColliderCategory category, float time, float magnitude)
    {
        ColliderName = colliderName;
        Category = category;
        Time = time;
        MagnitudeAtCollisionExit = magnitude;
    }
}