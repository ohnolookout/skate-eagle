using UnityEngine;
using System;

public interface IPlayer
{
    bool Collided { get; }
    bool FacingForward { get; }
    bool IsRagdoll { get; }
    int JumpCount { get; set; }
    ICollisionManager CollisionManager {get;}
    Rigidbody2D RagdollBoard { get; }
    Rigidbody2D RagdollBody { get; }
    Rigidbody2D Rigidbody { get; }
    MomentumTracker MomentumTracker { get; }
    int StompCharge { get; set; }
    bool Stomping { get; set; }
    int StompThreshold { get; }
    Transform Transform { get; }
    Vector2 Velocity { get; }
    Animator Animator { get; }
    float RotationAccel { get; set; }
    float FlipBoost { get; }
    float StompSpeedLimit { get; }
    float JumpForce { get; }
    float JumpMultiplier { get; }
    float FlipDelay { get; }
    float DownForce { get; }
    InputEventController InputEvents { get; set; }
    Action<Collision2D, ColliderCategory?, TrackingType?> AddCollision { get; set; }
    public Action<Collision2D, ColliderCategory?> RemoveCollision { get; set; }
    static Action FinishStop { get; set; }
    static Action OnStomp { get; set; }
    static Action OnDismount { get; set; }
    static Action OnStartAttempt { get; set; }
    static Action<IPlayer, double> OnFlip { get; set; }
    static Action<IPlayer, double> FlipRoutine { get; set; }
    static Action<IPlayer> OnJump { get; set; }
    static Action<IPlayer> OnSlowToStop { get; set; }
    static Action<IPlayer> OnStartWithStomp { get; set; }


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
    Vector2 VectorChange(TrackingType body);
}