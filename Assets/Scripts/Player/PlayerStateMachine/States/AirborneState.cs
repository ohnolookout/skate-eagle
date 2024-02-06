using UnityEngine;
using System;
using System.Threading.Tasks;
public class AirborneState : PlayerState
{
    private const float _jumpReleaseLimit = 0.2f;
    private bool _undoJumpDampen = false;
    private int _downPressCount = 0;
    private Action _decreaseDownPressCount;
    private JumpAdjuster _jumpAdjuster;
    public AirborneState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _decreaseDownPressCount = () => Mathf.Clamp(_downPressCount - 1, 0, 3);
        _jumpAdjuster = new(_player);

    }

    public override void EnterState()
    {
        Debug.Log("Entering airborne");
        _downPressCount = 0;
        _player.Rigidbody.centerOfMass = new Vector2(0, 0f);
        _player.CollisionManager.OnCollide += OnLand;
        _params.RotationStart = _player.Rigidbody.rotation;
        _animator.SetBool("Airborne", true);
        
        if (_player.Params.StompCharge >= _player.Params.StompThreshold)
        {
            _player.InputEvents.OnDownPress += DownPress;
        }

        if (_player.CheckForJumpRelease)
        {
            _jumpAdjuster.AddReleaseCheck();
            _player.InputEvents.OnJumpRelease += JumpRelease;
        } else
        {
            CheckEnableJump(false);
        }
    }

    public override void ExitState()
    {
        _player.CollisionManager.OnCollide -= OnLand;
        _player.InputEvents.OnJumpRelease -= JumpRelease;
        _player.InputEvents.OnJumpPress -= SecondJump;
        _player.InputEvents.OnDownPress -= DownPress;
    }

    public override void FixedUpdateState()
    {
        _animator.SetBool("AirborneUp", _body.velocity.y >= 0);
        _animator.SetFloat("YSpeed", _body.velocity.y);
    }

    private void SecondJump()
    {
        _player.InputEvents.OnJumpPress -= SecondJump;
        _jumpAdjuster.ScheduleSecondJump( () => ChangeState(_stateFactory.GetState(PlayerStateType.Jumping)) );
    }

    private void JumpRelease()
    {
        _player.InputEvents.OnJumpRelease -= JumpRelease;
        _jumpAdjuster.ScheduleJumpRelease();
        _player.CheckForJumpRelease = false;
        CheckEnableJump(true);
    }

    private void OnLand(ColliderCategory _, float __)
    {
        ChangeState(_stateFactory.GetState(PlayerStateType.Grounded));
    }
    private void CheckEnableJump(bool firstJumpOccured)
    {
        if (_player.Params.JumpCount >= _player.Params.JumpLimit)
        {
            return;
        }
        _player.InputEvents.OnJumpPress += SecondJump;
        if (firstJumpOccured)
        {
            _jumpAdjuster.AddSecondJumpCheck();
        }
    }

    private void DownPress()
    {
        _downPressCount++;
        if (_downPressCount > 1)
        {
            ChangeState(_stateFactory.GetState(PlayerStateType.Stomping));
        }
        else
        {
            _player.DelayedFunc(_decreaseDownPressCount, 0.25f);
        }
    }
}