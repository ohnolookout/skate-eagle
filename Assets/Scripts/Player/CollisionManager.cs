using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CollisionManager: MonoBehaviour, ICollisionManager
{
    private OrderedDictionary<string, TimedCollisionExit> _pendingExits = new();
    private Dictionary<ColliderCategory, List<string>> collidedCategories = new();
    private float uncollideTime = 0.15f;
    public Action<ColliderCategory, float> OnCollide { get; set; }
    public Action<ColliderCategory, float> OnUncollide { get; set; }
    public Action OnAirborne { get; set; }

    void OnEnable()
    {
        LevelManager.OnGameOver += _ => RemoveNonragdollColliders();
    }

    void Update()
    {
        CheckPendingExits();
    }

    public void CheckPendingExits()
    {
        //While there are pending collisions, continue checking the first pending collision
        //until one is found that has not exceeded the time limit
        while (_pendingExits.Count > 0)
        {
            TimedCollisionExit collision = _pendingExits.Value(0);
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
            _pendingExits.Remove(0);
            //If the current category has no more active collisions, remove it from the dictionary and send call to audio
            //If no categories are collided, send eagle into airborne mode
        }
        if (collidedCategories.Count == 0)
        {
            OnAirborne?.Invoke();
        }
    }
    public void AddCollision(Collision2D collision, float magnitudeDelta, ColliderCategory? inputCategory = null)
    {
        string colliderName = collision.otherCollider.name;
        ColliderCategory category = inputCategory ?? ParseCollider(collision);
        if (!collidedCategories.ContainsKey(category))
        {
            collidedCategories[category] = new();
            OnCollide?.Invoke(category, magnitudeDelta);
        }
        else if (collidedCategories[category].Contains(colliderName))
        {
            _pendingExits.Remove(colliderName);
            return;
        }
        //Send collision to player audio and add collision to list of colliders that are collided.
        collidedCategories[category].Add(colliderName);
    }
    public void SetCollisionTimer(float newTimer)
    {
        uncollideTime = newTimer;
    }
    public void RemoveCollision(Collision2D collision, float magnitudeAtCollisionExit, ColliderCategory? inputCategory = null)
    {
        string colliderName = collision.otherCollider.name;
        ColliderCategory category = inputCategory ?? ParseCollider(collision);
        //Instantly remove collision if it's not the last collider in the cateory to minimize the number of pending exits.
        if (collidedCategories.ContainsKey(category) && collidedCategories[category].Count > 1)
        {
            collidedCategories[category].Remove(colliderName);
        }
        else
        {
            _pendingExits.Add(colliderName, new(colliderName, category, Time.time, magnitudeAtCollisionExit));
        }
    }
    private void RemoveCategoryIfEmpty(ColliderCategory category, float magnitudeAtCollisionExit)
    {
        if (collidedCategories[category].Count == 0)
        {
            collidedCategories.Remove(category);
            OnUncollide?.Invoke(category, magnitudeAtCollisionExit);
        }
    }

    public void RemoveNonragdollColliders()
    {
        if (collidedCategories.ContainsKey(ColliderCategory.Body))
        {
            collidedCategories[ColliderCategory.Body].Remove("Player");
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
        foreach (var colliderName in _pendingExits.Dictionary.Keys.ToList())
        {
            if (colliderName == "Player" || colliderName == "LWheelCollider" || colliderName == "RWheelCollider" || colliderName == "BoardCollider")
            {
                _pendingExits.Remove(colliderName);
                continue;
            }
            ColliderCategory category = _pendingExits.Value(colliderName).Category;
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
    
    private static ColliderCategory ParseCollider(Collision2D collision)
    {
        switch (collision.otherCollider.name)
        {
            case "Player":
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
                return ColliderCategory.Body;
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
