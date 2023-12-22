using UnityEngine;
using System;
using System.Collections;
public class PlayerActiveState : PlayerBaseState
{

    public PlayerActiveState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _context.RunManager.EnterGameOver += _ => SwitchState(_factory.Dead());
        _context.RunManager.EnterFinish += _ => SwitchState(_factory.Finished());
        _context.PlayerControls.Inputs.Player.Rotate.started += _ => StartRotation();
        _context.PlayerControls.Inputs.Player.Rotate.canceled += _ => StopRotation();
        _context.PlayerControls.Inputs.Player.Down.started += _ => StartCrouch();
        _context.PlayerControls.Inputs.Player.Down.started += _ => StopCrouch();

    }
    public override void UpdateState()
    {
        CheckSwitchStates();
        DirectionCheck();
        UpdateAnimatorParameters();
    }
    public override void FixedUpdateState()
    {
        if (_context.IsRotating)
        {
            _context.RigidEagle.AddTorque(-_context.RotationAccel * _context.PlayerControls.Rotation.x);
        }
        if(_context.Crouched && !_context.Stomping)
        {
            _context.RigidEagle.AddForce(new Vector2(0, -_context.DownForce * 20));
        }
    }
    public override void ExitState()
    {
        _context.RunManager.EnterGameOver -= _ => SwitchState(_factory.Dead());
        _context.RunManager.EnterFinish -= _ => SwitchState(_factory.Finished());
        _context.PlayerControls.Inputs.Player.Rotate.started -= _ => StartRotation();
        _context.PlayerControls.Inputs.Player.Rotate.canceled -= _ => StopRotation();
    }
    public override void CheckSwitchStates()
    {
        if (_context.RigidEagle.transform.position.x >= _context.RunManager.FinishPoint.x && _context.CollisionTracker.BothWheelsCollided)
        {
            _context.RunManager.Finish();
        }
    }
    public override void InitializeSubState()
    {
    }
    public override void CollisionEnter(Collision2D collision)
    {
        if (collision.otherCollider.name == "Skate Eagle")
        {
            _context.RunManager.GameOver();
        }
        _context.CollisionTracker.UpdateCollision(collision, true);
    }
    public override void CollisionExit(Collision2D collision)
    {
        _context.CollisionTracker.UpdateCollision(collision, false);
        _context.RotationStart = _context.RigidEagle.rotation;
    }
    private void DirectionCheck()
    {
        bool lastDirection = _context.FacingForward;
        _context.FacingForward = _context.RigidEagle.velocity.x >= 0;
        if (lastDirection != _context.FacingForward && !_context.IsRagdoll)
        {
            _context.transform.localScale = new Vector3(-_context.transform.localScale.x, _context.transform.localScale.y, _context.transform.localScale.z);
        }
    }
    

    private void StartRotation()
    {
        _context.IsRotating = true;
    }

    private void StopRotation()
    {
        _context.IsRotating = false;
    }
    private void StartCrouch()
    {
        _context.Crouched = true;
    }

    private void StopCrouch()
    {
        _context.Crouched = false;
    }

    private void UpdateAnimatorParameters()
    {
        _context.Animator.SetFloat("Speed", _context.RigidEagle.velocity.magnitude);
        _context.Animator.SetFloat("YSpeed", _context.RigidEagle.velocity.y);
        _context.Animator.SetBool("FacingForward", _context.FacingForward);
        _context.Animator.SetBool("Airborne", !_context.Collided);
        _context.Animator.SetBool("Crouched", _context.PlayerController.down);
        if (!_context.Collided)
        {
            _context.Animator.SetBool("AirborneUp", _context.RigidEagle.velocity.y >= 0);
        }
    }
}
