using UnityEngine;
using System;

public interface IPlayer
{
    bool Collided { get; }
    bool FacingForward { get; set; }
    bool IsRagdoll { get; set; }
    bool DoLanding { get; set; }
    ICollisionManager CollisionManager { get; }
    Rigidbody2D RagdollBoard { get; }
    Rigidbody2D RagdollBody { get; }
    Rigidbody2D NormalBody { get; }
    MomentumTracker MomentumTracker { get; }
    JumpManager JumpManager { get; }
    PlayerParameters Params { get; }
    PlayerEventAnnouncer EventAnnouncer { get; }
    PlayerAnimationManager AnimationManager { get; }
    bool Stomping { get; set; }
    InputEventController InputEvents { get; set; }
    TrailRenderer Trail { get; }

    void SwitchDirection();
    void DismountSound();
    void TriggerBoost(float boostValue, float boostMultiplier);
    void CancelAsyncTokens();
    
}