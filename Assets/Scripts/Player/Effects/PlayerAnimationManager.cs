using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager
{
    private IPlayer _player;
    private Animator _animator;

    public PlayerAnimationManager(IPlayer player, Animator animator)
    {
        _player = player;
        _animator = animator;
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Land, Land);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Stomp, Stomp);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Brake, Brake);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Dismount, Dismount);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Stand, Stand);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Crouch, Crouch);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Airborne, Airborne);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Jump, Jump);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Push, Push);
    }

    private void Push(IPlayer obj)
    {

    }

    private void Stand(IPlayer obj)
    {
        _animator.SetBool("Crouched", false);
        _animator.SetTrigger("Stand Up");
    }

    private void Crouch(IPlayer obj)
    {
        _animator.SetBool("Crouched", true);
        _animator.SetTrigger("Crouch");
    }

    private void Jump(IPlayer obj)
    {
        _animator.SetTrigger("Jump");
    }

    private void Airborne(IPlayer obj)
    {
        _animator.SetBool("Airborne", true);
    }

    private void Dismount(IPlayer obj)
    {
        //throw new NotImplementedException();
    }

    private void Brake(IPlayer obj)
    {
        _animator.SetTrigger("Brake");
    }

    public void Land(IPlayer player)
    {
        _animator.SetBool("Airborne", false);
        _animator.SetFloat("forceDelta", _player.MomentumTracker.ReboundMagnitude(TrackingType.PlayerNormal));
        _animator.SetTrigger("Land");
    }

    public void UpdateSpeed()
    {
        _animator.SetFloat("Speed", _player.NormalBody.velocity.magnitude);
    }

    public void UpdateAirborneSpeed()
    {
        _animator.SetBool("AirborneUp", _player.NormalBody.velocity.y >= 0);
        _animator.SetFloat("YSpeed", _player.NormalBody.velocity.y);
    }

    public void SetOnBoard(bool onBoard)
    {
        _animator.SetBool("OnBoard", onBoard);
    }

    public void Stomp(IPlayer player)
    {

    }

}
