using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : PlayerBaseState
{
    private const float SecondJumpMultiplier = 0.75f;
    private float _jumpStartTime = 0;
    public JumpState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
        _isRootMovement = true;
        _context.PlayerControls.Inputs.Player.Down.canceled += _ => OnJumpRelease();
    }

    public override void EnterState()
    {
        _context.Animator.SetTrigger("Jump");
        float jumpMultiplier = 1;
        _context.LastJumpTime = Time.time;
        if (_context.RigidEagle.velocity.y < 0)
        {
            _context.RigidEagle.velocity = new Vector2(_context.RigidEagle.velocity.x, 0);
        }
        if (_context.JumpCount == 0)
        {
            _context.RigidEagle.angularVelocity *= 0.1f;
            _context.RigidEagle.centerOfMass = new Vector2(0, 0.0f);
        }
        else
        {
            jumpMultiplier = SecondJumpMultiplier;
        }
        _context.RigidEagle.AddForce(new Vector2(0, _context.JumpForce * 1000 * jumpMultiplier));
    }
    public override void UpdateState()
    {

    }
    public override void FixedUpdateState()
    {

    }
    public override void ExitState()
    {
    }
    public override void CheckSwitchStates()
    {
    }
    public override void InitializeSubState()
    {
    }
    public override void CollisionEnter(Collision2D collision)
    {
    }
    public override void CollisionExit(Collision2D collision)
    {
    }

    private void OnJumpRelease()
    {
        throw new NotImplementedException();
    }
}
