using UnityEngine;

public class CameraStateMachine
{
    public CameraState cameraState { get; set; }
    private CameraStateFactory _stateFactory;
    public Camera camera { get; set; }

    public CameraStateMachine(Camera cam)
    {
        camera = cam;
        _stateFactory = new(this);
    }

    public void InitializeState(CameraStateType startingState = CameraStateType.Standby)
    {
        cameraState = _stateFactory.GetState(startingState);
        cameraState.EnterState();
    }

}
