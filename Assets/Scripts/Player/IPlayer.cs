using UnityEngine;
using System;

public interface IPlayer
{
    bool Collided { get; }
    bool FacingForward { get; set; }
    bool IsRagdoll { get; set; }
    bool DoLanding { get; set; }
    ICollisionManager CollisionManager {get;}
    Rigidbody2D RagdollBoard { get; }
    Rigidbody2D RagdollBody { get; }

    Rigidbody2D NormalBody { get; }
    MomentumTracker MomentumTracker { get; }
    JumpManager JumpManager { get; }
    PlayerParameters Params { get; }
    bool Stomping { get; set; }
    Animator Animator { get; }
    InputEventController InputEvents { get; set; }
    Action<Collision2D, MomentumTracker, ColliderCategory, TrackingType> AddCollision { get; set; }
    public Action<Collision2D, ColliderCategory> RemoveCollision { get; set; }
    Action FinishStop { get; set; }
    Action OnStomp { get; set; }
    Action OnDismount { get; set; }
    Action OnStartAttempt { get; set; }
    Action<IPlayer, double> OnFlip { get; set; }
    Action<IPlayer> OnJump { get; set; }
    Action<IPlayer> OnSlowToStop { get; set; }
    Action<IPlayer> OnStartWithStomp { get; set; }
    Action OnDie { get; set; }

    void Die();
    void DismountSound();
    void SlowToStop();
    void TriggerBoost(float boostValue, float boostMultiplier);
    
}