using UnityEngine;

public abstract class CameraState
{
    protected Camera _camera;
    protected CameraStateMachine _cameraMachine;
    protected IPlayer _player;
    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void FixedUpdateState() { }

    public void ChangeState(CameraState newState)
    {
        ExitState();
        newState.EnterState();
    }
}


