using UnityEngine;
using System;
using System.Collections.Generic;

public enum ColliderCategory { LWheel, RWheel, Board, Body, BodyAndBoard };
public interface ICollisionManager
{
    bool BothWheelsCollided { get; }
    bool WheelsCollided { get; }

    Action<ColliderCategory, float> OnCollide { get; set; }
    Action<ColliderCategory, float> OnUncollide { get; set; }
    Action OnAirborne { get; set; }
    void AddCollision(Collision2D collision, MomentumTracker momentumTracker, ColliderCategory inputCategory, TrackingType trackingType);
    void RemoveCollision(Collision2D collision, ColliderCategory inputCategory);
}