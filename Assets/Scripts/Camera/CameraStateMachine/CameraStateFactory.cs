using System.Collections.Generic;
using UnityEngine;

public enum CameraStateType
{
    Standby,
    TrackGround,
    ChangeDirection,
    ExitChangeDirection,
    Freefall
}

public class CameraStateFactory
{
    private CameraStateMachine _cameraMachine;
    private Dictionary<CameraStateType, CameraState> _stateDict;

    public CameraStateFactory(CameraStateMachine machine)
    {
        _cameraMachine = machine;
        _stateDict = new();
        //_stateDict[CameraStateType.Standby] = new CameraStandbyState(_cameraMachine, this);
        //_stateDict[CameraStateType.TrackGround] = new CameraTrackGroundState(_cameraMachine, this);
        //_stateDict[CameraStateType.ChangeDirection] = new CameraChangeDirectionState(_cameraMachine, this);
        //_stateDict[CameraStateType.ExitChangeDirection] = new CameraExitChangeDirectionState(_cameraMachine, this);
        //_stateDict[CameraStateType.Freefall] = new CameraFreefallState(_cameraMachine, this);
    }

    public CameraState GetState(CameraStateType type)
    {
        return _stateDict[type];
    }
}
