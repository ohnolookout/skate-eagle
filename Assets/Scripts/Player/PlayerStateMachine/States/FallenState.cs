
public class FallenState : PlayerState
{
    public FallenState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
    }

    public override void EnterState()
    {
        PlayerAsyncUtility.DelayedFreeze(_player, 0.5f);
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Fall);
    }


}