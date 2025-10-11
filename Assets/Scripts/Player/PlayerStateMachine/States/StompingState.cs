using UnityEngine;
using System;
public class StompingState : PlayerState
{
    private float _originalRotationAccel;
    private float _timer = 0, _stallTime = 0.3f; //_stallTime was 0.075f
    private bool _stalling = true, _diving = false;
    private Action _boost;
    public StompingState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _originalRotationAccel = _player.Params.RotationAccel;
        _boost = () => _player.TriggerBoost(_player.Params.FlipBoost, 1.8f);
    }

    public override void EnterState()
    {
        _player.CollisionManager.OnCollide += OnLand; 
        _player.Params.JumpCount = 2;
        _player.Params.StompCharge = 0;
        _player.NormalBody.angularVelocity = 0;
        _player.Params.RotationAccel *= 1.5f;
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Stomp);
        _timer = 0;
        _stalling = true;
        _diving = false;
    }

    public override void ExitState()
    {
        _player.CollisionManager.OnCollide -= OnLand;
        _player.Params.RotationAccel = _originalRotationAccel;
    }

    public override void FixedUpdateState()
    {
        if (_stalling)
        {
            StallPhase();
            return;
        }
        if (_diving)
        {
            DivePhase();
        }
    }

    private void OnLand(ColliderCategory _, float __)
    {
        _diving = false;
        _stalling = false;
        PlayerAsyncUtility.DelayedFunc(_boost, 0.2f);
        ChangeState(_stateFactory.GetState(PlayerStateType.Grounded));
    }

    private void StallPhase()
    {
        if (_timer < _stallTime)
        {
            _player.NormalBody.linearVelocity -= new Vector2(_player.NormalBody.linearVelocity.x * 0.1f, _player.NormalBody.linearVelocity.y * 0.4f);
            _timer += Time.deltaTime;
        }
        else
        {
            _player.NormalBody.centerOfMass = new Vector2(0, -2f);
            _stalling = false;
            _diving = true;
        }
    }

    private void DivePhase()
    {
        if (_player.NormalBody.linearVelocity.y > _player.Params.StompSpeedLimit)
        {
            _player.NormalBody.linearVelocity -= new Vector2(0, 0.15f * Mathf.Abs(_player.NormalBody.linearVelocity.y));
            _player.NormalBody.linearVelocity = new Vector2(_player.NormalBody.linearVelocity.x, Mathf.Clamp(_player.NormalBody.linearVelocity.y, _player.Params.StompSpeedLimit, -64));
        }
        else
        {
            _diving = false;
        }
    }

}