using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveState : PlayerState
{
    public bool doRotate = false;
    private Vector2 _rotationInput = new(0,0);
    public bool crouched = false;

    public ActiveState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {

    }

    public override void EnterState()
    {
        _playerMachine.player.InputEvents.OnDownPress += StartCrouch;
        _playerMachine.player.InputEvents.OnDownRelease += StopCrouch;
        _playerMachine.player.InputEvents.OnRotate += StartRotate;
        _playerMachine.player.InputEvents.OnRotateRelease += StopRotate;
    }
    public override void ExitState()
    {
        _playerMachine.player.InputEvents.OnDownPress -= StartCrouch;
        _playerMachine.player.InputEvents.OnDownRelease -= StopCrouch;
        _playerMachine.player.InputEvents.OnRotate -= StartRotate;
        _playerMachine.player.InputEvents.OnRotateRelease -= StopRotate;
    }

    public override void FixedUpdateState()
    {
        if (crouched && !PlayerCoroutines.Stomping)
        {
            _playerMachine.player.Rigidbody.AddForce(new Vector2(0, -_playerMachine.player.DownForce * 20));
        }
        if (doRotate)
        {
            _playerMachine.player.Rigidbody.AddTorque(-_playerMachine.player.RotationAccel * _rotationInput.x);
        }
    }

    private void StartRotate(Vector2 rotation)
    {
        doRotate = true;
        _rotationInput = rotation;
    }

    private void StopRotate()
    {
        doRotate = false;
        _rotationInput = new(0, 0);
    }

    private void StartCrouch()
    {
        crouched = true;
        _playerMachine.player.Animator.SetBool("Crouched", true);
        _playerMachine.player.Animator.SetTrigger("Crouch");
    }
    private void StopCrouch()
    {
        _playerMachine.player.Animator.SetBool("Crouched", false);
        _playerMachine.player.Animator.SetTrigger("Stand Up");
        crouched = false;
    }
}
