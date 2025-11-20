using UnityEngine;

public class CameraStandbyState : CameraState
{
    public CameraStandbyState(CameraStateMachine cameraMachine) : base(cameraMachine)
    {
    }

    public override void EnterState()
    {
        LevelManager.OnStandby += OnExitStandby;
    }

    public override void ExitState()
    {
        LevelManager.OnStandby -= OnExitStandby;
    }

    public override void FixedUpdateState()
    {
        return;
    }

    public void OnExitStandby()
    {
        ChangeState(_cameraMachine.Factory.GetState(CameraStateType.TrackGround));
    }
}