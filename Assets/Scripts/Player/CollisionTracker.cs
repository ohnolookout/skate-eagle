using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CollisionTracker : MonoBehaviour, ICollisionManager
{
    private OrderedDictionary<string, TimedCollisionExit> pendingUncollisions = new();
    private Dictionary<ColliderCategory, List<string>> collidedCategories = new();
    private float uncollideTime = 0.15f;
    public Action<ColliderCategory, float> OnCollide { get; set; }
    public Action<ColliderCategory, float> OnUncollide { get; set; }
    public Action OnAirborne { get; set; }
    private Action<LiveRunManager> onGameOver;


    private void OnEnable()
    {
        onGameOver += _ => RemoveNonragdollColliders();
        LiveRunManager.OnGameOver += onGameOver;
    }
    //Set float in collisionTimers to startTime instead of current timer. Then look at currentTime-startTime.
    void Update()
    {
        CheckPendingExits();
    }
    public void CheckPendingExits()
    {
        //While there are pending collisions, continue checking the first pending collision
        //until one is found that has not exceeded the time limit
        while (pendingUncollisions.Count > 0)
        {
            TimedCollisionExit collision = pendingUncollisions.Value(0);
            if (Time.time - collision.Time < uncollideTime)
            {
                //If first collision in list does not exceed time limit, break loop
                break;
            }
            //If collision does exceed time limit, remove it from the list of colliders that are currently collided in its category
            //And remove it from the list of pending collisions.
            if (collidedCategories.ContainsKey(collision.Category))
            {
                collidedCategories[collision.Category].Remove(collision.ColliderName);
                RemoveCategoryIfEmpty(collision.Category, collision.MagnitudeAtCollisionExit);
            }
            pendingUncollisions.Remove(0);
            //If the current category has no more active collisions, remove it from the dictionary and send call to audio
            //If no categories are collided, send eagle into airborne mode
            if (collidedCategories.Count == 0)
            {
                OnAirborne?.Invoke();
            }
        }
    }

    public void SetCollisionTimer(float newTimer)
    {
        uncollideTime = newTimer;
    }

    public void AddCollision(Collision2D collision, float velocityDelta, ColliderCategory? input = null)
    {
        string colliderName = collision.otherCollider.name;
        ColliderCategory category = ParseCollider(collision);
        if (!collidedCategories.ContainsKey(category))
        {
            collidedCategories[category] = new();
            OnCollide?.Invoke(category, velocityDelta);
        }
        else if (collidedCategories[category].Contains(colliderName))
        {
            pendingUncollisions.Remove(colliderName);
            return;
        }
        Debug.Log("Adding collision for " + colliderName);
        //Send collision to player audio and add collision to list of colliders that are collided.
        collidedCategories[category].Add(colliderName);
    }

    public void RemoveCollision(Collision2D collision, float magnitudeAtCollisionExit, ColliderCategory? input = null)
    {
        string colliderName = collision.otherCollider.name;
        ColliderCategory category = ParseCollider(collision);
        pendingUncollisions.Add(colliderName, new(colliderName, category, Time.time, magnitudeAtCollisionExit));
    }

    private void RemoveCategoryIfEmpty(ColliderCategory category, float magnitudeAtCollisionExit)
    {
        if (collidedCategories[category].Count == 0)
        {
            collidedCategories.Remove(category);
            Debug.Log("Removing collider category " + category);
            OnUncollide?.Invoke(category, magnitudeAtCollisionExit);
        }
    }
    private static ColliderCategory ParseCollider(Collision2D collision)
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
        if (collidedCategories.ContainsKey(ColliderCategory.Body))
        {
            collidedCategories[ColliderCategory.Body].Remove("Skate Eagle");
            RemoveCategoryIfEmpty(ColliderCategory.Body, 0);
        }
        if (collidedCategories.ContainsKey(ColliderCategory.LWheel))
        {
            collidedCategories[ColliderCategory.LWheel].Remove("LWheelCollider");
            RemoveCategoryIfEmpty(ColliderCategory.LWheel, 0);
        }
        if (collidedCategories.ContainsKey(ColliderCategory.RWheel))
        {
            collidedCategories[ColliderCategory.RWheel].Remove("RWheelCollider");
            RemoveCategoryIfEmpty(ColliderCategory.RWheel, 0);
        }
        if (collidedCategories.ContainsKey(ColliderCategory.Board))
        {
            collidedCategories[ColliderCategory.Board].Remove("BoardCollider");
            RemoveCategoryIfEmpty(ColliderCategory.Board, 0);
        }
        foreach (var colliderName in pendingUncollisions.Dictionary.Keys.ToList())
        {
            if (colliderName == "Skate Eagle" || colliderName == "LWheelCollider" || colliderName == "RWheelCollider" || colliderName == "BoardCollider")
            {
                pendingUncollisions.Remove(colliderName);
                continue;
            }
            ColliderCategory category = pendingUncollisions.Value(colliderName).Category;
            if (!collidedCategories.ContainsKey(category))
            {
                collidedCategories[category] = new() { colliderName };
            }
            else if (!collidedCategories[category].Contains(colliderName))
            {
                collidedCategories[category].Add(colliderName);
            }
        }
    }

    public bool WheelsCollided
    {
        get => collidedCategories.ContainsKey(ColliderCategory.LWheel)
|| collidedCategories.ContainsKey(ColliderCategory.RWheel);
    }
    public bool BothWheelsCollided
    {
        get => collidedCategories.ContainsKey(ColliderCategory.LWheel)
&& collidedCategories.ContainsKey(ColliderCategory.RWheel);
    }
    public bool LWheel { get => collidedCategories.ContainsKey(ColliderCategory.LWheel); }
    public bool RWheel { get => collidedCategories.ContainsKey(ColliderCategory.RWheel); }
    public bool Body { get => collidedCategories.ContainsKey(ColliderCategory.Body); }
    public bool Board { get => collidedCategories.ContainsKey(ColliderCategory.Board); }
    public bool Collided { get => collidedCategories.Count > 0; }
    private struct TimedCollisionExit
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
}
