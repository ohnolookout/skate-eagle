using UnityEngine;

public class ActiveState : PlayerState
{
    public bool doRotate = false;
    private Vector2 _rotationInput = new(0,0);
    private Vector2 _finishPoint;
    public bool crouched = false;
    private Transform _playerTransform;
    private Rigidbody2D _playerBody;
    private bool _checkFinish = false;

    public ActiveState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _playerTransform = _player.NormalBody.transform;
        _playerBody = _player.NormalBody;
        LevelManager.OnActivateFinishLine += DoActivateFinish;
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
        InitializeSubstate(_stateFactory.GetState(PlayerStateType.Pushing));
    }
    public override void ExitState()
    {
        _player.InputEvents.OnDownPress -= StartCrouch;
        _player.InputEvents.OnDownRelease -= StopCrouch;
        _player.InputEvents.OnRotate -= StartRotate;
        _player.InputEvents.OnRotateRelease -= StopRotate;
        _collisionManager.OnCollide -= CheckForBodyCollision;
    }

    public override void UpdateState()
    {
        DirectionCheck();
        if (_checkFinish)
        {
            FinishCheck();
        }
        if (_substate != null)
        {
            _substate.UpdateStates();
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
        _player.FacingForward = _playerBody.velocity.x >= 0;
        if (_player.FacingForward != lastDirection)
        {
            _player.SwitchDirection();
        }
    }
    private void FinishCheck()
    {
        if (_playerTransform.position.x >= _finishPoint.x && _player.CollisionManager.BothWheelsCollided)
        {
            if (crouched)
            {
                StopCrouch();
            }
            ChangeState(_stateFactory.GetState(PlayerStateType.Braking), false);
        }
    }

    private void CheckForBodyCollision(ColliderCategory colliderCategory, float _)
    {
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

    public void DoActivateFinish(Vector2 finishPoint)
    {
        _finishPoint = finishPoint;
        _checkFinish = true;
        LevelManager.OnActivateFinishLine -= DoActivateFinish;
    }
}
