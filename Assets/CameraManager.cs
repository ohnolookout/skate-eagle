using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private ProCamera2D _camera;
    private ProCamera2DForwardFocus _forwardFocus;
    [SerializeField] private CameraZoom _cameraZoom;
    private float _transitionSmoothness = 0.4f;
    public bool doLogPosition = false;
    private bool _doDuration = false;
    void Awake()
    {
        _camera = ProCamera2D.Instance;
        _forwardFocus = _camera.GetComponent<ProCamera2DForwardFocus>();
        _transitionSmoothness = _forwardFocus.TransitionSmoothness;
        //Targeting events
        _camera.RemoveAllCameraTargets();
        LevelManager.OnPlayerCreated += AddPlayerTarget;
        LevelManager.OnLanding += GoToStartPosition;
        LevelManager.OnAttempt += TurnOnDuration;
        //GroundSegment.OnSegmentBecomeVisible += AddSegmentTargets;
        //GroundSegment.OnSegmentBecomeInvisible += RemoveSegmentTargets;


        //Freeze events
        LevelManager.OnStandby += UnfreezeCamera;
        LevelManager.OnFall += FreezeCamera;
        LevelManager.OnCrossFinish += FreezeCamera;
        LevelManager.OnGameOver += FreezeCamera;
        LevelManager.OnRestart += FreezeCamera;
        FreezeCamera();
    }

    void Update()
    {
        if(doLogPosition)
        {
            Debug.Log("Camera position: " + _camera.transform.position);
        }
    }

    private void AddPlayerTarget(IPlayer player)
    {
        _camera.AddCameraTarget(player.CameraTarget, CameraTargetUtility.GetDuration(CameraTargetType.Player));
    }

    private void AddSegmentTargets(GroundSegment segment)
    {
        if(!segment.DoTarget)
        {
            return;
        }
        if (_doDuration)
        {
            _camera.AddCameraTarget(segment.LowPointTarget, CameraTargetUtility.GetDuration(CameraTargetType.GroundSegmentLowPoint));
        }
        else
        {
            _camera.AddCameraTarget(segment.LowPointTarget);
        }
        //_camera.AddCameraTarget(segment.HighPointTarget, CameraTargetUtility.GetDuration(CameraTargetType.GroundSegmentHighPoint));
    }

    private void RemoveSegmentTargets(GroundSegment segment)
    {
        if (!segment.DoTarget)
        {
            return;
        }
        if (_doDuration)
        {
            _camera.RemoveCameraTarget(segment.LowPointTarget.TargetTransform, CameraTargetUtility.GetDuration(CameraTargetType.GroundSegmentLowPoint));
        }
        else
        {
            _camera.RemoveCameraTarget(segment.LowPointTarget.TargetTransform);
        }
        //_camera.RemoveCameraTarget(segment.HighPointTarget.TargetTransform, CameraTargetUtility.GetDuration(CameraTargetType.GroundSegmentHighPoint));
    }

    private void FreezeCamera()
    {
        _camera.RemoveAllCameraTargets();
        _camera.GetComponent<ProCamera2DForwardFocus>().TransitionSmoothness = 0f;
        _camera.FollowHorizontal = false;
        _camera.FollowVertical = false;
        _cameraZoom.gameObject.SetActive(false);
        _doDuration = false;
    }

    private void UnfreezeCamera()
    {
        _camera.GetComponent<ProCamera2DForwardFocus>().TransitionSmoothness = _transitionSmoothness;
        _camera.FollowHorizontal = true;
        _camera.FollowVertical = true;
        _cameraZoom.gameObject.SetActive(true);
        _cameraZoom.ResetZoom();
    }

    private void GoToStartPosition(Level level, PlayerRecord _)
    {
        _camera.MoveCameraInstantlyToPosition(level.StartPoint);
    }

    private void TurnOnDuration()
    {
        _doDuration = true;
    }
}
