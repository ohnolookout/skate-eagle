using UnityEngine;

public class CameraFreezeState : CameraState
{
    public CameraFreezeState(CameraStateMachine cameraMachine) : base(cameraMachine)
    {
    }

    public override void EnterState()
    {
        Debug.Log("Entering freeze state.");
        LevelManager.OnStandby += OnExitFreeze;
    }

    public override void ExitState()
    {
        LevelManager.OnStandby -= OnExitFreeze;
    }

    public override void FixedUpdateState()
    {
        return;
    }

    public void OnExitFreeze()
    {
        ChangeState(_cameraMachine.Factory.GetState(CameraStateType.TrackGround));
    }
}