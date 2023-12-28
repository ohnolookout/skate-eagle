using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum ColliderCategory { LWheel, RWheel, Board, Body };
public class CollisionTracker : MonoBehaviour
{
    private OrderedDictionary<string, TimedCollision> pendingUncollisions = new();
    private Dictionary<ColliderCategory, List<string>> collidedCategories = new();
    private float uncollideTime = 0.15f;
    public Action<ColliderCategory> OnCollide, OnUncollide;
    public Action OnAirborne;
    private Action<LiveRunManager> onGameOver;


    void Awake()
    {
    }

    private void OnEnable()
    {
        onGameOver += _ => RemoveNonragdollColliders();
        LiveRunManager.OnGameOver += onGameOver;
    }
    private void OnDisable()
    {
        LiveRunManager.OnGameOver -= onGameOver;
    }

    //Set float in collisionTimers to startTime instead of current timer. Then look at currentTime-startTime.
    void Update()
    {
        //While there are pending collisions, continue checking the first pending collision
        //until one is found that has not exceeded the time limit
        while (pendingUncollisions.Count > 0)
        {
            TimedCollision collision = pendingUncollisions.Value(0);
            if (Time.time - collision.Time < uncollideTime)
            {
                //If first collision in list does not exceed time limit, break loop
                break;
            }
            //If collision does exceed time limit, remove it from the list of colliders that are currently collided in its category
            //And remove it from the list of pending collisions.
            if (collidedCategories.ContainsKey(collision.Category)){
                collidedCategories[collision.Category].Remove(collision.ColliderName);
                RemoveCategoryIfEmpty(collision.Category);
            }
            pendingUncollisions.Remove(0);
            //If the current category has no more active collisions, remove it from the dictionary and send call to audio
            //If no categories are collided, send eagle into airborne mode
            if(collidedCategories.Count == 0)
            {
                OnAirborne?.Invoke();
            }
        }
    }

    public void SetCollisionTimer(float newTimer)
    {
        uncollideTime = newTimer;
    }

    public void UpdateCollision(Collision2D collision, bool isCollided)
    {
        UpdateCollision(collision.otherCollider.name, ParseCollider(collision), isCollided);
    }

    private void RemoveCategoryIfEmpty(ColliderCategory category)
    {
        if (collidedCategories[category].Count == 0)
        {
            collidedCategories.Remove(category);
            OnUncollide?.Invoke(category);
        }
    }

    public void UpdateCollision(string name, ColliderCategory category, bool isCollided)
    {
        //If collision exit, add collider to timerDict to register collision exit after delay.
        if (!isCollided)
        {
            pendingUncollisions.Add(name, new(name, category, Time.time));
            return;
        }
        //If collision enter and collider is already on collider list,
        //remove it from timerDict of collisions pending removal so that timer stops
        if (!collidedCategories.ContainsKey(category))
        {
            collidedCategories[category] = new();
            OnCollide?.Invoke(category);
        } else if (collidedCategories[category].Contains(name))
        {
            pendingUncollisions.Remove(name);
            return;
        }
        //Send collision to player audio and add collision to list of colliders that are collided.
        collidedCategories[category].Add(name);

    }


    private ColliderCategory ParseCollider(Collision2D collision)
    {
        switch (collision.otherCollider.name)
        {
            case "Skate Eagle":
                return ColliderCategory.Body;
            case "LWheelCollider":
            case "LWheelRagdoll":
                return ColliderCategory.LWheel;
            case "RWheelCollider":
            case "RWheelRagdoll":
                return ColliderCategory.RWheel;
            case "BoardCollider":
            case "BoardRagdoll":
                return ColliderCategory.Board;
            default:
                return ColliderCategory.Board;
        }
    }

    public void RemoveNonragdollColliders()
    {
        if (collidedCategories.ContainsKey(ColliderCategory.Body)){
            collidedCategories[ColliderCategory.Body].Remove("Skate Eagle");
            RemoveCategoryIfEmpty(ColliderCategory.Body);
        }
        if (collidedCategories.ContainsKey(ColliderCategory.LWheel))
        {
            collidedCategories[ColliderCategory.LWheel].Remove("LWheelCollider");
            RemoveCategoryIfEmpty(ColliderCategory.LWheel);
        }
        if (collidedCategories.ContainsKey(ColliderCategory.RWheel))
        {
            collidedCategories[ColliderCategory.RWheel].Remove("RWheelCollider");
            RemoveCategoryIfEmpty(ColliderCategory.RWheel);
        }
        if (collidedCategories.ContainsKey(ColliderCategory.Board))
        {
            collidedCategories[ColliderCategory.Board].Remove("BoardCollider");
            RemoveCategoryIfEmpty(ColliderCategory.Board);
        }
        foreach(var colliderName in pendingUncollisions.Dictionary.Keys.ToList())
        {
            if(colliderName == "Skate Eagle" || colliderName == "LWheelCollider" || colliderName == "RWheelCollider" || colliderName == "BoardCollider")
            {
                pendingUncollisions.Remove(colliderName);
                continue;
            }
            ColliderCategory category = pendingUncollisions.Value(colliderName).Category;
            if (!collidedCategories.ContainsKey(category)){
                collidedCategories[category] = new() { colliderName };
            } else if (!collidedCategories[category].Contains(colliderName)){
                collidedCategories[category].Add(colliderName);
            }
        }
    }

    public bool WheelsCollided
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.LWheel) || collidedCategories.ContainsKey(ColliderCategory.RWheel);
        }
    }

    public bool BothWheelsCollided
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.LWheel) && collidedCategories.ContainsKey(ColliderCategory.RWheel);
        }
    }

    public bool LWheel
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.LWheel);
        }
    }

    public bool RWheel
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.RWheel);
        }
    }

    public bool Body
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.Body);
        }
    }

    public bool Board
    {
        get
        {
            return collidedCategories.ContainsKey(ColliderCategory.Board);
        }
    }

    public bool Collided
    {
        get
        {
            return collidedCategories.Count > 0;
        }
    }

    private struct TimedCollision
    {
        public ColliderCategory Category;
        public string ColliderName;
        public float Time;

        public TimedCollision(string colliderName, ColliderCategory category, float time)
        {
            ColliderName = colliderName;
            Category = category;
            Time = time;
        }
    }
}
