using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class GroundedState : PlayerState
{
    private Action OnAirborne;
    private bool _doDampen;
    private const int _dampenThreshold = 180;
    public GroundedState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        OnAirborne += () => ChangeState(_stateFactory.GetState(PlayerStateType.Airborne));
    }

    public override void EnterState()
    {
        _doDampen = true;
        DampenLanding();
        _player.NormalBody.centerOfMass = new Vector2(0, -2f);
        _animator.SetBool("Airborne", false);
        _animator.SetFloat("forceDelta", _player.MomentumTracker.ReboundMagnitude(TrackingType.PlayerNormal));     
        _animator.SetTrigger("Land");
        _player.InputEvents.OnJumpPress += FirstJump;
        _player.CollisionManager.OnAirborne += OnAirborne;
        _player.Params.JumpCount = 0;
        FlipCheck();
    }

    public override void ExitState()
    {
        _doDampen = false;
        _player.InputEvents.OnJumpPress -= FirstJump;
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
            DoFlip(spins);
        }
    }

    //Try getting rid of _dampenThresholdCount.

    private async void DampenLanding()
    {
        _player.NormalBody.angularVelocity = Mathf.Clamp(_player.NormalBody.angularVelocity, -300, 300);
        float dampenTimer = 0;
        int dampenThresholdCount = 0;
        while (dampenTimer < 0.2f && dampenThresholdCount < 2 && _doDampen)
        {
            if (Mathf.Abs(_player.NormalBody.angularVelocity) > 60)
            {
                _player.NormalBody.angularVelocity *= 0.3f;
            }
            if (_player.NormalBody.angularVelocity < _dampenThreshold)
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

    private void FirstJump()
    {
        _player.JumpManager.Jump();
        ChangeState(_stateFactory.GetState(PlayerStateType.Airborne));
    }

    private async void DoFlip(double spins)
    {
        if (_player.Params.StompCharge < _player.Params.StompThreshold)
        {
            _player.Params.StompCharge = Mathf.Min((int)spins + _player.Params.StompCharge, _player.Params.StompThreshold);
        }
        _player.OnFlip?.Invoke(_player, spins);
        await Task.Delay((int)(_player.Params.FlipDelay * 100));
        float boostMultiplier = 1 + ((-1 / (float)spins) + 1);
        _player.TriggerBoost(_player.Params.FlipBoost, boostMultiplier);
    }

}
