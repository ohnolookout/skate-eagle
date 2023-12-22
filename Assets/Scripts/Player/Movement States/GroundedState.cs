using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedState : PlayerBaseState
{
    public GroundedState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
        _isRootMovement = true;
    }
    public override void EnterState()
    {
        //Figure out landing intensity.
        _context.PlayerControls.Inputs.Player.Jump.started += _ => SwitchState(_factory.Jump());
        _context.CollisionTracker.OnUncollide += _ => CheckForAirborne();
        _context.OnJump += _ => SwitchState(_factory.Jump());
        _context.DoLanding();
    }
    public override void UpdateState()
    {

    }
    public override void FixedUpdateState()
    {

    }
    public override void ExitState()
    {
        _context.PlayerControls.Inputs.Player.Jump.started -= _ => SwitchState(_factory.Jump());
    }

    private void CheckForAirborne()
    {
        if (!_context.CollisionTracker.Collided)
        {
            _context.JumpCount = Mathf.Max(1, _context.JumpCount);
            SwitchState(_factory.Airborne());
        }
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
}
