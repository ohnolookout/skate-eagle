using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private ProCamera2D _camera;
    private ProCamera2DForwardFocus _forwardFocus;
    private LinkedCameraTarget _currentTarget;
    [SerializeField] private CameraZoom _cameraZoom;
    private float _transitionSmoothness = 0.4f;
    public bool doLogPosition = false;
    private bool _doDuration = false;
    private bool _doUpdate = true;
    private List<LinkedCameraTarget> _targetsToCheck = new();
    private List<LinkedCameraTarget> _targetsToTrack = new();
    private IPlayer _player;
    private Transform _playerTransform;
    private LayerMask _groundLayerMask;
    private const float _groundCheckRadius = 100f;
    private int _frameCount = 0;
    void Awake()
    {
        _camera = ProCamera2D.Instance;
        _forwardFocus = _camera.GetComponent<ProCamera2DForwardFocus>();
        _transitionSmoothness = _forwardFocus.TransitionSmoothness;
        _groundLayerMask = LayerMask.GetMask("Ground");

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
        foreach(var target in _targetsToCheck)
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
            Gizmos.DrawSphere(_currentTarget.LowTarget.TargetPosition, 1f);
        }

        Gizmos.DrawWireSphere(_camera.transform.position, _groundCheckRadius);
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
        if (_frameCount % 20 == 0)
        {
            RefreshOverlapSphere();
            _frameCount = 0;
        }

        _frameCount++;

        var currentClosestDistance = float.PositiveInfinity;
        var currentTarget = _currentTarget;

        foreach (var target in _targetsToCheck)
        {
            var sqDistance = (target.LowTarget.TargetPosition - _camera.transform.position).sqrMagnitude;
            if (sqDistance < currentClosestDistance)
            {
                currentClosestDistance = sqDistance;
                currentTarget = target;
            }
        }

        if(currentTarget != _currentTarget)
        {
            UpdateCurrentTarget(currentTarget);
        }
    }

    private void RefreshOverlapSphere()
    {
        var colliders = Physics2D.OverlapCircleAll(_camera.transform.position, _groundCheckRadius, _groundLayerMask);
        _targetsToCheck.Clear();
        foreach (var collider in colliders)
        {
            var groundSegment = collider.GetComponentInParent<GroundSegment>();
            if (groundSegment == null || !groundSegment.DoTarget)
            {
                continue;
            }
            _targetsToCheck.Add(groundSegment.LinkedCameraTarget);
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

        _targetsToTrack.Add(_currentTarget);

        var duration = CameraTargetUtility.GetDuration(CameraTargetType.GroundSegmentLowPoint);

        bool playerTargetIsFound = false;

        for(int i = 0; i < _camera.CameraTargets.Count; i++)
        {
            var target = _camera.CameraTargets[i];

            if (!playerTargetIsFound && target.TargetTransform.GetInstanceID() == _player.Transform.GetInstanceID())
            {
                playerTargetIsFound = true;
                continue;
            }

            _camera.RemoveCameraTarget(target, duration);
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
        RefreshOverlapSphere();
    }

    private void GoToStartPosition(Level level, PlayerRecord _, ICameraTargetable startTarget)
    {
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
