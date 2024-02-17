using UnityEngine;
using System.Threading.Tasks;
public class FinishedState : PlayerState
{
    public FinishedState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _isRootState = true;
    }

    public override void EnterState()
    {
        PlayerAsyncUtility.DelayedFunc(() => _player.EventAnnouncer.InvokeAction(PlayerEvent.Finish), 0.5f);
    }

}