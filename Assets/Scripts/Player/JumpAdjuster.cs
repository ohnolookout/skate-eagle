using UnityEngine;
using System;
using System.Threading.Tasks;

public class JumpAdjuster
{
    private IPlayer _player;
    private bool _doJumpRelease = false, _undoJumpDampen = false;
    private Action _turnOffUndoDampen;
    private const float _jumpReleaseLimit = 0.2f;

    public bool UndoJumpDampen { get => _undoJumpDampen; set => _undoJumpDampen = value; }

    public JumpAdjuster(IPlayer player)
    {
        _player = player;
        _turnOffUndoDampen = () => _undoJumpDampen = false;
    }

    //Turns on doJumpRelease and sets a timer to turn it back off after the max jump duration
    public void AddReleaseCheck()
    {
        _doJumpRelease = true;
        CancelReleaseCheckOnDelay(_player.Params.FullJumpDuration);
    }

    //Turns on undoJumpDampen and sets timer to turn it back off after the max release duration
    public void AddSecondJumpCheck()
    {
        _undoJumpDampen = true;
        _player.DelayedFunc(_turnOffUndoDampen, _jumpReleaseLimit);
    }
    public void ScheduleSecondJump(Action doJump)
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
            Debug.Log($"Triggering jump on {_player.Params.FullJumpDuration - timeSinceLastJump} delay");
            _player.DelayedFunc(doJump, _player.Params.FullJumpDuration - timeSinceLastJump);
        }
        else
        {
            doJump();
        }
    }
    public void ScheduleJumpRelease()
    {
        if (!_doJumpRelease)
        {
            return;
        }
        _doJumpRelease = false;

        float scaledMinDuration = _player.Params.MinJumpDuration * _player.Params.JumpMultiplier;
        float timeElapsed = Time.time - _player.Params.JumpStartTime;

        Action dampenJump = () => _player.Rigidbody.AddForce(new Vector2(0, -_player.Params.JumpForce * 250 * _player.Params.JumpMultiplier));

        //If jump has been released after the minimum jump duration, dampen jump immediately.
        if (timeElapsed >= scaledMinDuration)
        {
            dampenJump();
        }
        //If released before min jump duration, dampen on a delay up to the min jump duration.
        else
        {
            //float scaledJumpDuration = _player.Params.FullJumpDuration * _player.Params.JumpMultiplier;
            _player.DelayedFunc(dampenJump, scaledMinDuration - timeElapsed);
        }
        //If player has jumps remaining, wait for the second jump to cancel dampening if it happens fast enough.


    }

    private void UndoDampen(float jumpMultiplier)
    {
        _player.Rigidbody.AddForce(new Vector2(0, _player.Params.JumpForce * 300 * jumpMultiplier));
        _undoJumpDampen = false;
    }


    private async void CancelReleaseCheckOnDelay(float delay)
    {
        float releaseTimer = 0;
        while (releaseTimer < delay)
        {
            releaseTimer += Time.deltaTime;
            //Ends cancel early if checkforjumprelease becomes false (when jump has been released).
            if (!_player.CheckForJumpRelease)
            {
                return;
            }
            await Task.Yield();
        }
        _doJumpRelease = false;
        _player.CheckForJumpRelease = false;
    }


}
