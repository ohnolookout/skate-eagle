using UnityEngine;

public class CameraStateMachine
{
    public CameraState cameraState { get; set; }
    private CameraStateFactory _stateFactory;
    public CameraManager cameraManager { get; set; }
    public Camera Camera { get => cameraManager.camera; }
    public CameraStateFactory Factory { get => _stateFactory; }

    public CameraStateMachine(CameraManager camManager)
    {
        cameraManager = camManager;
        LevelManager.OnLanding += GoToStartPosition;
        _stateFactory = new(this);
        InitializeState();
    }

    public void InitializeState(CameraStateType startingState = CameraStateType.Standby)
    {
        cameraState = _stateFactory.GetState(startingState);
        cameraState.EnterState();
    }

    public void GoToStartPosition(Level level, PlayerRecord __)
    {
        cameraManager.ResetCamera(level.SerializedStartLine);
        InitializeState(CameraStateType.Standby);
    }

    public void FixedUpdate()
    {
        cameraState.FixedUpdateState();
    }


}
