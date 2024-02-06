using UnityEngine;
using System;

public interface IPlayer
{
    bool Collided { get; }
    bool FacingForward { get; set; }
    bool IsRagdoll { get; }
    bool CheckForJumpRelease { get; set; }
    int JumpCount { get; set; }
    bool DoLanding { get; set; }
    ICollisionManager CollisionManager {get;}
    Rigidbody2D RagdollBoard { get; }
    Rigidbody2D RagdollBody { get; }
    Rigidbody2D Rigidbody { get; }
    MomentumTracker MomentumTracker { get; }

    PlayerParameters Params { get; }
    int StompCharge { get; set; }
    bool Stomping { get; set; }
    int StompThreshold { get; }
    Transform Transform { get; }
    Vector2 Velocity { get; }
    Animator Animator { get; }
    float RotationAccel { get; set; }
    int FlipBoost { get; }
    float StompSpeedLimit { get; }
    float JumpForce { get; }
    float JumpMultiplier { get; }
    float FlipDelay { get; }
    float DownForce { get; }
    InputEventController InputEvents { get; set; }
    Action<Collision2D, MomentumTracker, ColliderCategory, TrackingType> AddCollision { get; set; }
    public Action<Collision2D, ColliderCategory> RemoveCollision { get; set; }
    Action FinishStop { get; set; }
    Action OnStomp { get; set; }
    Action OnDismount { get; set; }
    Action OnStartAttempt { get; set; }
    Action<IPlayer, double> OnFlip { get; set; }
    static Action<IPlayer, double> FlipRoutine { get; set; }
    Action<IPlayer> OnJump { get; set; }
    Action<IPlayer> OnSlowToStop { get; set; }
    Action<IPlayer> OnStartWithStomp { get; set; }
    Action OnDie { get; set; }

    void Die();
    void Dismount();
    void DismountSound();
    void Fall();
    void GoAirborne();
    void Jump();
    void JumpRelease();
    void JumpValidation();
    void Ragdoll();
    void SlowToStop();
    void Stomp();
    void DoStart();
    void TriggerBoost(float boostValue, float boostMultiplier);
    void TriggerFinishStop();
    float MagnitudeDelta(TrackingType body);
    float MagnitudeDelta();

    void DelayedFunc(Action callback, float delayInSeconds);
    Vector2 VectorChange(TrackingType body);
}