using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraManager : MonoBehaviour
{
    private ProCamera2D _camera;
    private ProCamera2DForwardFocus _forwardFocus;
    private LinkedCameraTarget _currentTarget;
    private LinkedCameraTarget _rootKDTarget;
    [SerializeField] private CameraZoom _cameraZoom;
    private float _transitionSmoothness = 0.4f;
    public bool doLogPosition = false;
    private bool _doDuration = false;
    private bool _doUpdate = true;
    private List<LinkedCameraTarget> _targetsToCheck = new();
    private List<LinkedCameraTarget> _targetsToTrack = new();
    private IPlayer _player;
    private Transform _playerTransform;
    private int _frameCount = 0;
    void Awake()
    {
        _camera = ProCamera2D.Instance;
        _forwardFocus = _camera.GetComponent<ProCamera2DForwardFocus>();
        _transitionSmoothness = _forwardFocus.TransitionSmoothness;

        LevelManager.OnPlayerCreated += AddPlayerTarget;
        LevelManager.OnLanding += GoToStartPosition;
        LevelManager.OnAttempt += TurnOnDuration;


        //Freeze events
        LevelManager.OnStandby += UnfreezeCamera;
        LevelManager.OnFall += FreezeCamera;
        LevelManager.OnCrossFinish += FreezeCamera;
        LevelManager.OnGameOver += FreezeCamera;
        LevelManager.OnRestart += FreezeCamera;
        FreezeCamera();
    }

    void FixedUpdate()
    {
        if (_doUpdate)
        {
            CheckCurrentTarget();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(_currentTarget == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        foreach(var target in _targetsToTrack)
        {
            if (target == null || target.LowTarget == null)
            {
                continue;
            }
            Gizmos.DrawSphere(target.LowTarget.TargetPosition, 2f);
        }

        Gizmos.color = Color.blue;

        if (_currentTarget != null && _currentTarget.LowTarget != null)
        {
            Gizmos.DrawSphere(_currentTarget.LowTarget.TargetPosition, 2f);
        }

    }

    private void AddPlayerTarget(IPlayer player)
    {
        _player = player;
        _playerTransform = player.Transform;
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.SwitchDirection, OnSwitchPlayerDirection);
        _camera.AddCameraTarget(player.CameraTarget, CameraTargetUtility.GetDuration(CameraTargetType.Player));
    }

    private void CheckCurrentTarget()
    {

        var closestTarget = _currentTarget;
        if (_frameCount % 10 == 0)
        {
            closestTarget = KDTreeBuilder.FindNearest(_rootKDTarget, _playerTransform.position);
            _frameCount = 0;
        }

        _frameCount++;
        if(closestTarget != _currentTarget)
        {
            UpdateCurrentTarget(closestTarget);
        }
    }


    private void UpdateCurrentTarget(LinkedCameraTarget newTarget)
    {
        _currentTarget = newTarget;

        if (_player.FacingForward)
        {
            _targetsToTrack = _currentTarget.RightTargets;
        }
        else
        {
            _targetsToTrack = _currentTarget.LeftTargets;
        }


        var duration = CameraTargetUtility.GetDuration(CameraTargetType.GroundSegmentLowPoint);

        bool playerTargetIsFound = false;
        bool currentTargetIsFound = false;

        for (int i = 0; i < _camera.CameraTargets.Count; i++)
        {
            var target = _camera.CameraTargets[i];

            if (!playerTargetIsFound && target.TargetTransform.GetInstanceID() == _player.Transform.GetInstanceID())
            {
                playerTargetIsFound = true;
                continue;
            }

            if(target.TargetPosition == _currentTarget.LowTarget.TargetPosition)
            {
                currentTargetIsFound = true;
                continue;
            }

            _camera.RemoveCameraTarget(target, duration);
        }

        if (!currentTargetIsFound)
        {
            _camera.AddCameraTarget(_currentTarget.LowTarget, duration);
        }

        foreach (var target in _targetsToTrack)
        {
            if(target.LowTarget == null)
            {
                continue;
            }
            
            _camera.AddCameraTarget(target.LowTarget, duration);
        }

    }

    private void OnSwitchPlayerDirection(IPlayer player)
    {
        UpdateCurrentTarget(_currentTarget);
    }

    private void FreezeCamera()
    {
        _camera.RemoveAllCameraTargets();
        _camera.GetComponent<ProCamera2DForwardFocus>().TransitionSmoothness = 0f;
        _camera.FollowHorizontal = false;
        _camera.FollowVertical = false;
        _cameraZoom.gameObject.SetActive(false);
        _doDuration = false;
        _doUpdate = false;
    }

    private void UnfreezeCamera()
    {
        _camera.GetComponent<ProCamera2DForwardFocus>().TransitionSmoothness = _transitionSmoothness;
        _camera.FollowHorizontal = true;
        _camera.FollowVertical = true;
        _cameraZoom.gameObject.SetActive(true);
        _cameraZoom.ResetZoom();
        _doUpdate = true;
    }

    private void GoToStartPosition(Level level, PlayerRecord _, ICameraTargetable startTarget)
    {
        _rootKDTarget = level.RootCameraTarget;
        _camera.MoveCameraInstantlyToPosition(level.StartPoint);
        SetFirstTarget(startTarget);
    }

    private void SetFirstTarget(ICameraTargetable startTarget)
    {
        if (startTarget == null)
        {
            Debug.LogError("Start target is null");
            return;
        }
        _camera.AddCameraTarget(startTarget.LinkedCameraTarget.LowTarget, CameraTargetUtility.GetDuration(CameraTargetType.GroundSegmentLowPoint));
        UpdateCurrentTarget(startTarget.LinkedCameraTarget);
    }

    private void TurnOnDuration()
    {
        _doDuration = true;
    }
}
