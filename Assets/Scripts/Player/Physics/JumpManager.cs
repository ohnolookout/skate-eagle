using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;

/// <summary>
/// Manages the player's jump logic, including jump force, jump release, and double jump dampening.
/// </summary>
public class JumpManager
{
    private IPlayer _player;
    // Indicates if jump release logic should be processed
    private bool _doJumpRelease = false, _undoJumpDampen = false;
    // Actions to turn off jump dampen and release check flags
    private Action _turnOffUndoDampen, _turnOffReleaseCheck;
    // Maximum time allowed for jump release dampening
    private const float _jumpReleaseLimit = 0.2f;
    // Used to cancel async release check
    private CancellationTokenSource _tokenSource;
    private CancellationToken _releaseCheckToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="JumpManager"/> class.
    /// </summary>
    /// <param name="player">The player instance to manage jumps for.</param>
    public JumpManager(IPlayer player)
    {
        _player = player;
        _tokenSource = new();
        _releaseCheckToken = _tokenSource.Token;

        _turnOffUndoDampen = () => _undoJumpDampen = false;

        _turnOffReleaseCheck = () =>
        {
            if (!_releaseCheckToken.IsCancellationRequested)
            {
                _doJumpRelease = false;
            }
        };
    }

    /// <summary>
    /// Performs a jump, applying force and updating jump state.
    /// </summary>
    public void Jump()
    {
        _player.Params.JumpMultiplier = 1 - (_player.Params.JumpCount * 0.25f);
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Jump);
        // Reset downward velocity if jumping while falling
        if (_player.NormalBody.linearVelocity.y < 0)
        {
            _player.NormalBody.linearVelocity = new Vector2(_player.NormalBody.linearVelocity.x, 0);
        }
        // Clears angular velo
        if (_player.Params.JumpCount == 0)
        {
            _player.NormalBody.angularVelocity *= 0.1f;
            _player.NormalBody.centerOfMass = new Vector2(0, 0.0f);
        }
        _player.NormalBody.AddForce(new Vector2(0, _player.Params.JumpForce * 1000 * _player.Params.JumpMultiplier));
        _player.Params.JumpCount++;
        _player.Params.JumpStartTime = Time.time;
        AddReleaseCheck();
    }

    /// <summary>
    /// Turns on jump release check and schedules it to turn off after the max jump duration.
    /// </summary>
    public void AddReleaseCheck()
    {
        _doJumpRelease = true;
        PlayerAsyncUtility.DelayedFunc(_turnOffReleaseCheck, _player.Params.FullJumpDuration);
    }

    /// <summary>
    /// Turns on undo jump dampen and schedules it to turn off after the max release duration.
    /// </summary>
    public void AddSecondJumpCheck()
    {
        _undoJumpDampen = true;
        PlayerAsyncUtility.DelayedFunc(_turnOffUndoDampen, _jumpReleaseLimit);
    }

    /// <summary>
    /// Schedules or performs a second jump, handling dampen undo if needed.
    /// </summary>
    public void ScheduleSecondJump()
    {
        // If undoJumpDampen has not expired, adds speed to offset the dampen from the first jump due to rapid second jump
        if (_undoJumpDampen)
        {
            UndoDampen(_player.Params.JumpMultiplier);
            _undoJumpDampen = false;
        }
        float timeSinceLastJump = Time.time - _player.Params.JumpStartTime;

        // If second jump has been pressed faster than the minimum jump duration, triggers the second jump after a delay
        if (timeSinceLastJump < _player.Params.FullJumpDuration)
        {
            PlayerAsyncUtility.DelayedFunc(Jump, _player.Params.FullJumpDuration - timeSinceLastJump);
        }
        else
        {
            Jump();
        }
    }

    /// <summary>
    /// Handles jump release, applying dampening if released early.
    /// </summary>
    public void JumpRelease()
    {
        if (!_doJumpRelease)
        {
            return;
        }
        CancelReleaseCheck();
        _doJumpRelease = false;
        float scaledMinDuration = _player.Params.MinJumpDuration * _player.Params.JumpMultiplier;
        float timeElapsed = Time.time - _player.Params.JumpStartTime;

        Action dampenJump = () => _player.NormalBody.AddForce(new Vector2(0, -_player.Params.JumpForce * 250 * _player.Params.JumpMultiplier));

        // If jump has been released after the minimum jump duration, dampen jump immediately.
        if (timeElapsed >= scaledMinDuration)
        {
            dampenJump();
        }
        // If released before min jump duration, dampen on a delay up to the min jump duration.
        else
        {
            PlayerAsyncUtility.DelayedFunc(dampenJump, scaledMinDuration - timeElapsed);
        }

        // If player has jumps remaining, wait for the second jump to cancel dampening if it happens fast enough.
        if (_player.Params.JumpCount < _player.Params.JumpLimit)
        {
            AddSecondJumpCheck();
        }
    }

    /// <summary>
    /// Applies force to undo jump dampening for rapid second jumps.
    /// </summary>
    /// <param name="jumpMultiplier">The current jump multiplier.</param>
    private void UndoDampen(float jumpMultiplier)
    {
        _player.NormalBody.AddForce(new Vector2(0, _player.Params.JumpForce * 300 * jumpMultiplier));
        _undoJumpDampen = false;
    }

    /// <summary>
    /// Cancels the release check, stopping any scheduled release logic.
    /// </summary>
    public void CancelReleaseCheck()
    {
        _tokenSource.Cancel();
    }
}
