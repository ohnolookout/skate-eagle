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
        DelayedFinishStop(500);
    }

    private async void DelayedFinishStop(int delay)
    {
        await Task.Delay(delay);
        _player.FinishStop?.Invoke();
    }
}