using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;

public class JumpManager
{
    private IPlayer _player;
    private bool _doJumpRelease = false, _undoJumpDampen = false;
    private Action _turnOffUndoDampen, _turnOffReleaseCheck;
    private const float _jumpReleaseLimit = 0.2f;
    private CancellationTokenSource _tokenSource;
    private CancellationToken _releaseCheckToken;

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

    public void Jump()
    {
        _player.Params.JumpMultiplier = 1 - (_player.Params.JumpCount * 0.25f);
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Jump);
        if (_player.NormalBody.velocity.y < 0)
        {
            _player.NormalBody.velocity = new Vector2(_player.NormalBody.velocity.x, 0);
        }
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

    //Turns on doJumpRelease and sets a timer to turn it back off after the max jump duration
    public void AddReleaseCheck()
    {
        _doJumpRelease = true;
        PlayerAsyncUtility.DelayedFunc(_turnOffReleaseCheck, _player.Params.FullJumpDuration);
    }

    //Turns on undoJumpDampen and sets timer to turn it back off after the max release duration
    public void AddSecondJumpCheck()
    {
        _undoJumpDampen = true;
        PlayerAsyncUtility.DelayedFunc(_turnOffUndoDampen, _jumpReleaseLimit);
    }
    public void ScheduleSecondJump()
    {
        //If undoJumpDampen has not expired, adds speed to offset the dampen from the first jump due to rapid second jump
        if (_undoJumpDampen)
        {
            UndoDampen(_player.Params.JumpMultiplier);
            _undoJumpDampen = false;
        }
        float timeSinceLastJump = Time.time - _player.Params.JumpStartTime;

        //If second jump has been pressed faster than the minimum jump duration, triggers the second jump after a delay
        if (timeSinceLastJump < _player.Params.FullJumpDuration)
        {
            PlayerAsyncUtility.DelayedFunc(Jump, _player.Params.FullJumpDuration - timeSinceLastJump);
        }
        else
        {
            Jump();
        }
    }
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

        //If jump has been released after the minimum jump duration, dampen jump immediately.
        if (timeElapsed >= scaledMinDuration)
        {
            dampenJump();
        }
        //If released before min jump duration, dampen on a delay up to the min jump duration.
        else
        {
            //float scaledJumpDuration = _player.Params.FullJumpDuration * _player.Params.JumpMultiplier;
            PlayerAsyncUtility.DelayedFunc(dampenJump, scaledMinDuration - timeElapsed);
        }

        //If player has jumps remaining, wait for the second jump to cancel dampening if it happens fast enough.
        if (_player.Params.JumpCount < _player.Params.JumpLimit)
        {
            AddSecondJumpCheck();
        }


    }

    private void UndoDampen(float jumpMultiplier)
    {
        _player.NormalBody.AddForce(new Vector2(0, _player.Params.JumpForce * 300 * jumpMultiplier));
        _undoJumpDampen = false;
    }

    public void CancelReleaseCheck()
    {
        _tokenSource.Cancel();
    }


}
