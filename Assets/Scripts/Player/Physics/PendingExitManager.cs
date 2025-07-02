using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum CollisionType { Ground, OtherBody };
public class PendingExitManager
{
    private Dictionary<CollisionType, OrderedDictionary<ColliderCategory, TimedCollisionExit?>> _pendingExits = new()
    {
        { CollisionType.Ground, new() },
        { CollisionType.OtherBody, new() }
    };
    private Dictionary<CollisionType, float> _exitTimers = new();
    public Action<TimedCollisionExit> ExitCollision;
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

    public void AddCollision(ColliderCategory category)
    {
        var collisionType = CollisionType.Ground;
        if (category == ColliderCategory.BodyAndBoard)
        {
            collisionType = CollisionType.OtherBody;
        }

        if (_pendingExits[collisionType].Value(category) != null)
        {
            _pendingExits[collisionType].Add(category, null); // Move the collision to the end of the order list with a null value
        }
    }

    public void AddPendingExit(TimedCollisionExit collision)
    {
        CollisionType type = GetTypeFromCategory(collision.Category);
        _pendingExits[type].Insert(0, collision.Category, collision);
    }

    private CollisionType GetTypeFromCategory(ColliderCategory category)
    {
        if(category == ColliderCategory.BodyAndBoard)
        {
            return CollisionType.OtherBody;
        }
        return CollisionType.Ground;
    }

    private void CheckPendingExits(OrderedDictionary<ColliderCategory, TimedCollisionExit?> pendingExits, float uncollideTime)
    {
        //While there are pending exits, continue checking the first pending exit
        //until one is found that has not exceeded the time limit
        var exitsCompleted = 0;
        var i = 0;
        while(i < pendingExits.Count - exitsCompleted) {
            if (pendingExits.Value(i) == null)
            {
                break; // End loop at first null value
            }

            var category = pendingExits.Key(i);
            TimedCollisionExit pendingExit = (TimedCollisionExit)pendingExits.Value(category);

            if (Time.time - pendingExit.Time >= uncollideTime)
            {
                pendingExits.Add(category, null); // Move the collision to the end of the order list with a null value
                ExitCollision?.Invoke(pendingExit);
                exitsCompleted++;
            } else
            {
                i++;
            }
        }
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
    public float Time;
    public float MagnitudeAtCollisionExit;
    public float Count;

    public TimedCollisionExit(ColliderCategory category, float time, float magnitude)
    {
        Category = category;
        Time = time;
        MagnitudeAtCollisionExit = magnitude;
        Count = 1;
    }
}