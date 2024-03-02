using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PlayerEvent { Stomp, Dismount, Jump, Die, StartAttempt, Land, Flip, Brake, 
    Finish, Ragdoll, StartWithStomp, Crouch, Stand, Airborne, Push, LandSound, BodySound}
public class PlayerEventAnnouncer
{
    private IPlayer _player;
    public Action<PlayerEvent, IPlayer> OnGenericEvent;
    private Action<IPlayer> OnStomp, OnDismount, OnJump, OnDie, OnStartAttempt, OnLand, OnFlip, 
        OnBrake, OnFinish, OnRagdoll, OnStartWithStomp, OnCrouch, OnStand, OnAirborne, OnPush, OnLandSound, OnBodySound;
    private Action<Collision2D, MomentumTracker, ColliderCategory, TrackingType> AddCollision;
    private Action<Collision2D, ColliderCategory> RemoveCollision;
    private Dictionary<PlayerEvent, Action<IPlayer>> _actionDict;

    public PlayerEventAnnouncer(IPlayer player)
    {
        _player = player;
        _actionDict = new()
        {
            { PlayerEvent.Stomp, OnStomp },
            { PlayerEvent.Dismount, OnDismount },
            { PlayerEvent.Jump, OnJump },
            { PlayerEvent.Die, OnDie },
            { PlayerEvent.StartAttempt, OnStartAttempt },
            { PlayerEvent.Land, OnLand },
            { PlayerEvent.Flip, OnFlip },
            { PlayerEvent.Brake, OnBrake },
            { PlayerEvent.Finish, OnFinish },
            { PlayerEvent.StartWithStomp, OnStartWithStomp },
            { PlayerEvent.Ragdoll, OnRagdoll },
            { PlayerEvent.Crouch, OnCrouch },
            { PlayerEvent.Airborne, OnAirborne },
            { PlayerEvent.Push, OnPush },
            { PlayerEvent.Stand, OnStand },
            { PlayerEvent.LandSound, OnLandSound },
            { PlayerEvent.BodySound, OnBodySound }
        };
    }

    public Action<IPlayer> GetAction(PlayerEvent action)
    {
        return _actionDict[action];
    }

    public void SubscribeToAddCollision(Action<Collision2D, MomentumTracker, ColliderCategory, TrackingType> action)
    {
        AddCollision += action;
    }

    public void SubscribeToRemoveCollision (Action<Collision2D, ColliderCategory> action)
    {
        RemoveCollision += action;
    }


    public void InvokeAction(PlayerEvent action)
    {
        _actionDict[action]?.Invoke(_player);
    }

    public void SubscribeToEvent(PlayerEvent eventName, Action<IPlayer> action)
    {
        _actionDict[eventName] += action;
    }

    public void EnterCollision(Collision2D collision)
    {
        ColliderCategory category = ParseColliderName(collision.otherCollider.name);
        AddCollision?.Invoke(collision, _player.MomentumTracker, category, TrackingType.PlayerNormal);
    }

    public void ExitCollision(Collision2D collision)
    {
        RemoveCollision?.Invoke(collision, ParseColliderName(collision.otherCollider.name));
    }

    public void ClearActions()
    {
        foreach(PlayerEvent action in Enum.GetValues(typeof(PlayerEvent)))
        {
            _actionDict[action] = null;
        }
        AddCollision = null;
        RemoveCollision = null;
    }

    private static ColliderCategory ParseColliderName(string colliderName)
    {
        switch (colliderName)
        {
            case "LWheelCollider":
                return ColliderCategory.LWheel;
            case "RWheelCollider":
                return ColliderCategory.RWheel;
            case "BoardCollider":
                return ColliderCategory.Board;
            default:
                return ColliderCategory.Body;
        }
    }


    public void UnsubscribeToAddCollision(Action<Collision2D, MomentumTracker, ColliderCategory, TrackingType> action)
    {
        AddCollision -= action;
    }

    public void UnsubscribeToRemoveCollision(Action<Collision2D, ColliderCategory> action)
    {
        RemoveCollision -= action;
    }

    public void UnsubscribeFromEvent(PlayerEvent eventName, Action<IPlayer> action)
    {
        _actionDict[eventName] -= action;
    }

}
