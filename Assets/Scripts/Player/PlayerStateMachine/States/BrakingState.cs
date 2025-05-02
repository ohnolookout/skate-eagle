using UnityEngine;
public class BrakingState : PlayerState
{
    private Rigidbody2D _playerBody;
    private bool _onBoard = true;
    public BrakingState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _playerBody = _player.NormalBody;
        _isRootState = true;
    }

    //Figure out why sound isn't working.
    public override void EnterState()
    {
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Brake);
        _player.InputEvents.DisableInputs();
        if (_player.Trail != null)
        {
            _player.Trail.emitting = false;
        }

    }

    public override void FixedUpdateState()
    {
        _playerBody.linearVelocity -= _playerBody.linearVelocity * 0.08f;
        CheckForDismount();
        if (Mathf.Abs(_playerBody.linearVelocity.x) < 1)
        {
            _playerBody.linearVelocity = new Vector2(0, 0);
            ChangeState(_stateFactory.GetState(PlayerStateType.Finished));
            return;
        }
        
            
    }

    private void CheckForDismount()
    {
        if (_playerBody.linearVelocity.x < 10f && _onBoard)
        {
            _onBoard = false;
            _player.AnimationManager.SetOnBoard(false);
        }
    }


}