using UnityEngine;
using System;
using System.Collections.Generic;

public enum ColliderCategory { LWheel, RWheel, Board, Body, BodyAndBoard };
public interface ICollisionManager
{
    Dictionary<ColliderCategory, List<string>> CollidedCategories { get;}
    bool Board { get; }
    bool Body { get; }
    bool BothWheelsCollided { get; }
    bool Collided { get; }
    bool LWheel { get; }
    bool RWheel { get; }
    bool WheelsCollided { get; }

    Action<ColliderCategory, float> OnCollide { get; set; }
    Action<ColliderCategory, float> OnUncollide { get; set; }
    Action OnAirborne { get; set; }
    void AddCollision(Collision2D collision, ColliderCategory? inputCategory = null, TrackingType? trackingType = null);
    void RemoveCollision(Collision2D collision, ColliderCategory? inputCategory = null);
}