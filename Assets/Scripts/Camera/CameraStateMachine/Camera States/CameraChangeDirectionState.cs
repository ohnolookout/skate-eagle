using System;
using UnityEngine;

public class CameraChangeDirectionState : CameraState
{
    private const float _xTForExit = 0.7f;
    public CameraChangeDirectionState(CameraStateMachine cameraMachine) : base(cameraMachine)
    {
    }

    public override void EnterState()
    {
        Debug.Log("Entering change direction state.");
        var highPointTracker = _cameraManager.Targeter.HighPointTracker;
        highPointTracker.Update(_cameraManager.PlayerTransform.position.x);

        if(highPointTracker.Current == null)
        {
            ChangeState(_cameraMachine.Factory.GetState(CameraStateType.Freefall));
            return;
        }

        var currentHighPointX = highPointTracker.Current.position.x;

        float targetX;
        if (highPointTracker.Current.Next != null)
        {
            targetX = currentHighPointX + ((highPointTracker.Current.Next.position.x - currentHighPointX) / 2);
        }
        else
        {
            targetX = currentHighPointX;
        }

        _cameraManager.Targeter.UpdateLookaheadTarget(targetX);
        _cameraManager.Targeter.SetCamX(targetX);
        _cameraManager.Targeter.UpdateParams();

        DoDampening();
    }

    public override void ExitState()
    {
        _cameraManager.Mover.OnExitDirectionChange();
    }

    public override void FixedUpdateState()
    {
        if (_cameraManager.Targeter.UpdateTargetsDirectionChange() || PlayerOutsideBounds())
        {
            if (_cameraManager.Targeter.PlayerTracker.IsOverExtended)
            {
                ChangeState(_cameraMachine.Factory.GetState(CameraStateType.Freefall));
                return ;
            }
            ChangeState(_cameraMachine.Factory.GetState(CameraStateType.TrackGround));
            return;
        }
        _cameraManager.Mover.UpdatePosition(_cameraManager.Targeter);
    }

    private void DoDampening()
    {
        var cameraMover = _cameraManager.Mover;
        if (_cameraManager.TrackingPointsAreInCamera())
        {
            cameraMover.xStep = CameraMover.DefaultXStep / 6;
            cameraMover.yStep = CameraMover.DefaultYStep / 6;
            cameraMover.zoomStep = CameraMover.DefaultZoomStep / 4;
        }
        else
        {
            cameraMover.xStep = CameraMover.DefaultXStep / 4;
        }
    }

    private bool PlayerOutsideBounds()
    {
        var playerX = _cameraManager.PlayerTransform.position.x;
        var camDist = Mathf.Abs(playerX - _cameraManager.Camera.transform.position.x);

        if (camDist < _cameraManager.Camera.orthographicSize * _cameraManager.AspectRatio * _xTForExit)
        {
            return false;
        }

        return true;
    }

}