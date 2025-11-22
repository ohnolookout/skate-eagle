using UnityEngine;

public class CameraStateMachine
{
    public CameraState cameraState { get; set; }
    private CameraStateFactory _stateFactory;
    public CameraManager cameraManager { get; set; }
    public Camera Camera { get => cameraManager.Camera; }
    public CameraStateFactory Factory { get => _stateFactory; }

    public CameraStateMachine(CameraManager camManager)
    {
        cameraManager = camManager;
        LevelManager.OnLanding += GoToStartPosition;
        LevelManager.OnGameOver += () => cameraState.ChangeState(_stateFactory.GetState(CameraStateType.Freeze));
        LevelManager.OnCrossFinish += () => cameraState.ChangeState(_stateFactory.GetState(CameraStateType.Freeze));
        _stateFactory = new(this);
        InitializeState();
    }

    public void InitializeState(CameraStateType startingState = CameraStateType.Freeze)
    {
        cameraState = _stateFactory.GetState(startingState);
        cameraState.EnterState();
    }

    public void GoToStartPosition(Level level, PlayerRecord __)
    {
        cameraManager.ResetCamera(level.SerializedStartLine);
        if (cameraState != null)
        {
            cameraState.ChangeState(_stateFactory.GetState(CameraStateType.Freeze));
        } else
        {
            InitializeState();
        }
    }

    public void FixedUpdate()
    {
        cameraState.FixedUpdateState();
    }


}
