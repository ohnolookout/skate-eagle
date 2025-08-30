using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class GroundedState : PlayerState
{
    private Action OnAirborne;
    private bool _doDampen;
    private const int _dampenThreshold = 180;
    private const float _allowNewJumpDelay = 0.4f;

    // Constructor sets up the OnAirborne event to transition to Airborne state
    public GroundedState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        OnAirborne += () => ChangeState(_stateFactory.GetState(PlayerStateType.Airborne));
    }

    // Called when entering the grounded state
    public override void EnterState()
    {
        _doDampen = true;
        DampenLanding();

        //Lower center of mass to stabilize
        _player.NormalBody.centerOfMass = new Vector2(0, -2f);
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Land);

        //Add jump listener
        _player.InputEvents.OnJumpPress += FirstJump;

        //Add airborne listener
        _player.CollisionManager.OnAirborne += OnAirborne;

        //Reset jump count
        _player.Params.ResetJumpCount();
        _player.JumpManager.secondJumpScheduled = false;

        //Check for flips
        FlipCheck();
    }

    // Called when exiting the grounded state
    public override void ExitState()
    {
        //End jump dampen if still active
        _doDampen = false;

        //Remove jump and airborne listeners
        _player.InputEvents.OnJumpPress -= FirstJump;
        _player.CollisionManager.OnAirborne -= OnAirborne;
    }

    // Called every fixed update while in grounded state
    public override void FixedUpdateState()
    {
        _player.AnimationManager.UpdateSpeed();
    }

    // Checks if the player has completed a flip and applies flip logic
    private void FlipCheck()
    {
        double spins = Math.Round(Math.Abs(_player.Params.RotationStart - _body.rotation) / 360);
        _player.Params.RotationStart = _body.rotation;
        if (spins >= 1)
        {
            DoFlip(spins);
        }
    }

    // Dampen the player's angular velocity after landing to prevent excessive spinning
    // This method is called asynchronously when the player enters the grounded state
    // It clamps the initial angular velocity, then repeatedly reduces it if it's above a threshold
    // The loop runs for up to 0.2 seconds or until the angular velocity is sufficiently low for two consecutive checks
    // The dampening can be interrupted if the state is exited (_doDampen set to false)
    private async void DampenLanding()
    {
        // Clamp the initial angular velocity to prevent extreme spins
        _player.NormalBody.angularVelocity = Mathf.Clamp(_player.NormalBody.angularVelocity, -300, 300);
        float dampenTimer = 0;
        int dampenThresholdCount = 0;
        // Continue dampening while timer is less than 0.2s, threshold not reached twice, and dampening is allowed
        while (dampenTimer < 0.2f && dampenThresholdCount < 2 && _doDampen)
        {
            // If angular velocity is still high, reduce it sharply
            if (Mathf.Abs(_player.NormalBody.angularVelocity) > 60)
            {
                _player.NormalBody.angularVelocity *= 0.3f;
            }
            // If angular velocity is below the threshold, increment the threshold count
            if (_player.NormalBody.angularVelocity < _dampenThreshold)
            {
                dampenThresholdCount++;
            }
            else
            {
                dampenThresholdCount = 0;
            }

            // Wait for the next frame
            await Task.Yield();
            // Increment the timer by the time elapsed since the last frame
            dampenTimer += Time.fixedDeltaTime;
        }
        // End dampening
        _doDampen = false;
    }

    // Handles the first jump input while grounded
    private void FirstJump()
    {
        // Prevent jumping if within the new jump delay window
        if (Time.time - _player.Params.JumpStartTime < _allowNewJumpDelay)
        {
            return;
        }
        // Don't jump if player is on ground upside down
        var absRotation = Mathf.Abs(_player.NormalBody.rotation) % 360;
        if (absRotation > 90 && absRotation < 270)
        {
            return;
        }

        _player.JumpManager.Jump();
        ChangeState(_stateFactory.GetState(PlayerStateType.Airborne));
    }

    // Handles logic for when a flip is completed, including boosting and event invocation
    private async void DoFlip(double spins)
    {
        if (_player.Params.StompCharge < _player.Params.StompThreshold)
        {
            _player.Params.StompCharge = Mathf.Min((int)spins + _player.Params.StompCharge, _player.Params.StompThreshold);
        }
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Flip);
        await Task.Delay((int)(_player.Params.FlipDelay * 100));
        float boostMultiplier = 1 + ((-1 / (float)spins) + 1);
        _player.TriggerBoost(_player.Params.FlipBoost, boostMultiplier);
    }
}
