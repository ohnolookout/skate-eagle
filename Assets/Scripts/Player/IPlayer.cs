using UnityEngine;
using System;
using System.Threading;
using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections.Generic;

public interface IPlayer
{
    bool FacingForward { get; set; }
    bool IsRagdoll { get; set; }
    bool DoLanding { get; set; }
    float KillPlaneY { get; set; }
    ICollisionManager CollisionManager { get; }
    Collision2D LastLandCollision { get; set; }
    Rigidbody2D RagdollBoard { get; }
    Rigidbody2D RagdollBody { get; }
    Rigidbody2D NormalBody { get; }
    MomentumTracker MomentumTracker { get; }
    JumpManager JumpManager { get; }
    PlayerParameters Params { get; }
    PlayerEventAnnouncer EventAnnouncer { get; }
    PlayerAnimationManager AnimationManager { get; }
    bool Airborne { get; }
    InputEventController InputEvents { get; set; }
    TrailRenderer Trail { get; }
    Transform Transform { get; }
    CancellationToken FreezeToken { get; }
    CancellationTokenSource BoostTokenSource { get; }
    CancellationTokenSource FreezeTokenSource { get; }
    List<CameraTarget> CameraTargets { get; }
    CameraTarget CameraTarget { get; }

    void SwitchDirection();
    void TriggerBoost(float boostValue, float boostMultiplier);
    void CancelAsyncTokens();
    void InvokeBodySound();
    void InvokeLandSound();
    
}