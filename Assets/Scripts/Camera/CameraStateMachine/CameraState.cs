using UnityEngine;

public abstract class CameraState
{
    protected CameraManager _cameraManager;
    protected CameraStateMachine _cameraMachine;
    protected IPlayer _player;
    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void FixedUpdateState() { }

    public CameraState(CameraStateMachine cameraMachine)
    {
        _cameraMachine = cameraMachine;
        _cameraManager = cameraMachine.cameraManager;
    }

    public void ChangeState(CameraState newState)
    {
        ExitState();
        _cameraMachine.cameraState = newState;
        newState.EnterState();
    }
}


