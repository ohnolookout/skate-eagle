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
        _player.Trail.emitting = false;

    }

    public override void FixedUpdateState()
    {
        _playerBody.velocity -= _playerBody.velocity * 0.08f;
        CheckForDismount();
        if (Mathf.Abs(_playerBody.velocity.x) < 1)
        {
            Debug.Log("Exiting brake state");
            _playerBody.velocity = new Vector2(0, 0);
            ChangeState(_stateFactory.GetState(PlayerStateType.Finished));
            return;
        }
        
            
    }

    private void CheckForDismount()
    {
        if (_playerBody.velocity.x < 10f && _onBoard)
        {
            Debug.Log("Dismounting...");
            _onBoard = false;
            _player.AnimationManager.SetOnBoard(false);
        }
    }


}