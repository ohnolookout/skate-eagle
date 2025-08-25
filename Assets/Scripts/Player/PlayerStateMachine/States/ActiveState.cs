using UnityEngine;

public class ActiveState : PlayerState
{
    public bool doRotate = false;
    private Vector2 _rotationInput = new(0,0);
    public bool crouched = false;
    private Rigidbody2D _playerBody;

    public ActiveState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _playerBody = _player.NormalBody;
        LevelManager.OnFall += () => ChangeState(_stateFactory.GetState(PlayerStateType.Fallen));
        _isRootState = true;
    }


    public override void EnterState()
    {
        StartCrouch();
        _player.InputEvents.OnDownPress += StartCrouch;
        _player.InputEvents.OnDownRelease += StopCrouch;
        _player.InputEvents.OnRotate += StartRotate;
        _player.InputEvents.OnRotateRelease += StopRotate;
        _player.InputEvents.OnRagdoll += Die;
        _collisionManager.OnCollide += CheckForBodyCollision;
        LevelManager.OnCrossFinish += CrossFinish;
        InitializeSubstate(_stateFactory.GetState(PlayerStateType.Pushing));
    }
    public override void ExitState()
    {
        _player.InputEvents.OnDownPress -= StartCrouch;
        _player.InputEvents.OnDownRelease -= StopCrouch;
        _player.InputEvents.OnRotate -= StartRotate;
        _player.InputEvents.OnRotateRelease -= StopRotate;
        _collisionManager.OnCollide -= CheckForBodyCollision;
        LevelManager.OnCrossFinish -= CrossFinish;
    }

    public override void UpdateState()
    {
        DirectionCheck();
        if (_substate != null)
        {
            _substate.UpdateStates();
        }
        if(_player.Transform.position.y < _player.KillPlaneY)
        {
            Fall();
        }
    }

    public override void FixedUpdateState()
    {
        _player.MomentumTracker.Update();
        if (crouched && !_player.Stomping)
        {
            _playerBody.AddForce(new Vector2(0, -_player.Params.DownForce * 20));
        }
        if (doRotate)
        {
            _playerBody.AddTorque(-_player.Params.RotationAccel * _rotationInput.x);
        }
        if (_substate != null)
        {
            _substate.FixedUpdateStates();
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

    private float GetRotationMult(Vector2 rotation)
    {
        if(rotation.x > 0 && _playerBody.angularVelocity < 0 || rotation.x < 0 && _playerBody.angularVelocity > 0)
        {
            return 1;
        }

        var mult = 1 + Mathf.Abs(_playerBody.angularVelocity) / 100;
        Debug.Log("Rotation mult: " + mult);

        return mult;
    }

    private void StartCrouch()
    {
        crouched = true;
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Crouch);
    }
    private void StopCrouch()
    {
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Stand);
        crouched = false;
    }
    private void DirectionCheck()
    {
        bool lastDirection = _player.FacingForward;
        _player.FacingForward = _playerBody.linearVelocity.x >= 0;
        if (_player.FacingForward != lastDirection)
        {
            _player.SwitchDirection();
        }
    }

    private void CrossFinish()
    {
        if (crouched)
        {
            StopCrouch();
        }
        ChangeState(_stateFactory.GetState(PlayerStateType.Braking), false);
    }

    private void CheckForBodyCollision(ColliderCategory colliderCategory, float _)
    {
        Debug.Log("Collided with " + colliderCategory);
        if (colliderCategory == ColliderCategory.Body)
        {
            Die();
        }
    }

    private void Die()
    {
        _player.EventAnnouncer.InvokeAction(PlayerEvent.PreDie);
        ChangeState(_stateFactory.GetState(PlayerStateType.Ragdoll), false);
    }

    private void Fall()
    {
        ChangeState(_stateFactory.GetState(PlayerStateType.Fallen), false);
    }
}
