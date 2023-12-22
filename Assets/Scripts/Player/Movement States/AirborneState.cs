using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneState : PlayerBaseState {
    private bool _goingUp = true;
    
    public AirborneState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
        _isRootMovement = true;
    }
    public override void EnterState()
    {
        _context.PlayerControls.DoubleTap += ValidateStomp;
        _context.CollisionTracker.OnCollide += _ => SwitchState(_factory.Grounded());
        _context.OnJump += _ => SwitchState(_factory.Jump());
    }
    public override void UpdateState()
    {

    }
    public override void FixedUpdateState()
    {

    }
    public override void ExitState()
    {
        _context.PlayerControls.DoubleTap -= ValidateStomp;
        _context.CollisionTracker.OnCollide -= _ => SwitchState(_factory.Grounded());
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

    public void ValidateStomp() 
    { 
        if(_context.StompCharge >= _context.StompThreshold)
        {
            _context.StompCharge = 0;
            SwitchState(_factory.Stomp());
        }
    }
}
