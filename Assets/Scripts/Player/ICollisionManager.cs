using UnityEngine;
using System;

public enum ColliderCategory { LWheel, RWheel, Board, Body };
public interface ICollisionManager
{
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

    void CheckPendingExits();
    void AddCollision(Collision2D collision, float velocityDelta, ColliderCategory? inputCategory);
    void RemoveCollision(Collision2D collision, float magnitudeAtCollisionExit, ColliderCategory? inputCategory);
    void RemoveNonragdollColliders();
    void SetCollisionTimer(float newTimer);
}