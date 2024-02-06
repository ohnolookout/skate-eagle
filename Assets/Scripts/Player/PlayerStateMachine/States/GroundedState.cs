using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class GroundedState : PlayerState
{
    private Action OnJump, OnAirborne;
    private bool _doDampen;
    private const int _dampenThreshold = 180;
    public GroundedState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        OnJump += () => ChangeState(_stateFactory.GetState(PlayerStateType.Jumping));
        OnAirborne += () => ChangeState(_stateFactory.GetState(PlayerStateType.Airborne));
        OnAirborne += () => _player.JumpCount = 1;
    }

    public override void EnterState()
    {
        _doDampen = true;
        DampenLanding();
        _player.Rigidbody.centerOfMass = new Vector2(0, -2f);
        Debug.Log("Entering grounded");
        _animator.SetBool("Airborne", false);
        _animator.SetFloat("forceDelta", _player.MomentumTracker.ReboundMagnitude(TrackingType.PlayerNormal));     
        _animator.SetTrigger("Land");
        _player.InputEvents.OnJumpPress += OnJump;
        _player.CollisionManager.OnAirborne += OnAirborne;
        _player.JumpCount = 0;
        FlipCheck();
    }

    public override void ExitState()
    {
        _doDampen = false;
        _player.InputEvents.OnJumpPress -= OnJump;
        _player.CollisionManager.OnAirborne -= OnAirborne;
    }

    public override void FixedUpdateState()
    {
        _animator.SetFloat("Speed", _body.velocity.magnitude);
    }

    private void FlipCheck()
    {
        double spins = Math.Round(Math.Abs(_player.Params.RotationStart - _body.rotation) / 360);
        _player.Params.RotationStart = _body.rotation;
        if (spins >= 1)
        {
            if (_player.StompCharge < _player.StompThreshold)
            {
                _player.StompCharge = Mathf.Min((int)spins + _player.StompCharge, _player.StompThreshold);
            }
            _player.OnFlip?.Invoke(_player, spins);
        }
    }

    //Try getting rid of _dampenThresholdCount.

    private async void DampenLanding()
    {
        _player.Rigidbody.angularVelocity = Mathf.Clamp(_player.Rigidbody.angularVelocity, -300, 300);
        float dampenTimer = 0;
        int dampenThresholdCount = 0;
        while (dampenTimer < 0.2f && dampenThresholdCount < 2 && _doDampen)
        {
            if (Mathf.Abs(_player.Rigidbody.angularVelocity) > 60)
            {
                _player.Rigidbody.angularVelocity *= 0.3f;
            }
            if (_player.Rigidbody.angularVelocity < _dampenThreshold)
            {
                dampenThresholdCount++;
            }
            else
            {
                dampenThresholdCount = 0;
            }

            await Task.Yield();
        }
        _doDampen = false;
    }
}
