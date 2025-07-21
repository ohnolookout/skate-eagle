using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/*
public class CameraHighLowManager
{
    #region Declarations
    private PositionalMinMax<SortablePositionObject<Vector3>> _lowPoints; 
    private PositionalMinMax<SortablePositionObject<HighPoint>> _highPoints;
    private ICameraOperator _cameraOperator;
    private Vector3 _targetLowPoint, _lastLowPoint, _currentLowPoint;
    private HighPoint _targetHighPoint, _lastHighPoint;
    private float _lowPointBuffer, _highPointBuffer, _lowPointDistance = 0;
    private Func<float> _currentX;
    public const float DefaultLowPointBuffer = 100, DefaultHighPointBuffer = 100;
    public float LowPointBuffer => _lowPointBuffer;
    public float HighPointBuffer => _highPointBuffer;
    public Vector3 CurrentLowPoint => _currentLowPoint;
    public Vector3 TargetLowPoint => _targetLowPoint;
    public Vector3 LastLowPoint => _lastLowPoint;
    public HighPoint TargetHighPoint => _targetHighPoint;
    public HighPoint LastHighPoint => _lastHighPoint;
    public PositionalMinMax<SortablePositionObject<Vector3>> LowPoints { get => _lowPoints; }
    public PositionalMinMax<SortablePositionObject<HighPoint>> HighPoints { get => _highPoints; }
    #endregion

    #region Constructors
    public CameraHighLowManager(ICameraOperator cameraOperator, Ground terrain)
    {
        _currentX = () => _cameraOperator.CurrentHighLowX;
        _cameraOperator = cameraOperator;
        _lowPointBuffer = DefaultLowPointBuffer;
        _highPointBuffer = DefaultHighPointBuffer;
        
        BuildHighLowCaches(terrain);
        
        _targetLowPoint = _lowPoints.CurrentMinMax.Value;
        _lastLowPoint = _targetLowPoint;
        _currentLowPoint = _targetLowPoint;
        
        _targetHighPoint = _highPoints.CurrentMinMax.Value;
        _lastHighPoint = _targetHighPoint;

        LevelManager.GetPlayer.EventAnnouncer.SubscribeToEvent(PlayerEvent.SwitchDirection, OnSwitchDirection);
    }

    private void BuildHighLowCaches(Ground terrain)
    {
        //Get low and high point minmax caches from positional list factory
        PositionalListFactory<PositionObject<Vector3>>.HighLowPositional(this, _cameraOperator, terrain, out _lowPoints, out _highPoints);

        //Subscribe to new low/high point events in minmax caches
        _lowPoints.MinMax.OnNewMinMax += NewLowPoint;
        _highPoints.MinMax.OnNewMinMax += NewHighPoint;
    }
    #endregion

    #region Update
    public void UpdateCurrentHighLow(bool updateBuffers = false)
    {
        if (updateBuffers)
        {
            UpdateHighLowBuffers();
        }

        _lowPoints.Update();
        _highPoints.Update();
        _currentLowPoint = LerpedPoint(_targetLowPoint, _lastLowPoint, _lowPointDistance);
    }

    private void UpdateHighLowBuffers()
    {
        UpdateHighPointBuffer();
        UpdateLowPointBuffer();
    }

    private void UpdateLowPointBuffer()
    {
        _lowPointBuffer = Mathf.Clamp(_cameraOperator.Camera.orthographicSize / _cameraOperator.DefaultSize, 1, 2) * DefaultLowPointBuffer;
    }

    private void UpdateHighPointBuffer()
    {
        _highPointBuffer = Mathf.Clamp(_cameraOperator.Camera.orthographicSize / _cameraOperator.DefaultSize, 1, 2) * DefaultHighPointBuffer;
    }
    #endregion

    #region Add/Remove
    private void NewLowPoint(SortablePositionObject<Vector3> newLow)
    {
        _lastLowPoint = _currentLowPoint;
        _targetLowPoint = newLow.Value;
        _lowPointDistance = Mathf.Max(Mathf.Abs(_lastLowPoint.x - _targetLowPoint.x), 25);
    }
    private void NewHighPoint(SortablePositionObject<HighPoint> newHigh)
    {
        _lastHighPoint = _targetHighPoint;
        _targetHighPoint = newHigh.Value;
    }

    public Vector3 LerpedPoint(Vector3 target, Vector3 last, float totalDistance)
    {
        float currentDistance = Mathf.Abs(_currentX() - last.x);
        float t = Mathf.Clamp01(currentDistance / totalDistance);
        float lerpedY;

        if(t >= 1)
        {
            lerpedY = target.y;
        } else
        {
            lerpedY = Mathf.SmoothStep(last.y, target.y, t);
        }

        return new(_currentX(), lerpedY);
    }
    #endregion

    #region Switch Directions
    private void OnSwitchDirection(IPlayer player)
    {
        if (player.FacingForward)
        {
            _currentX = () => _cameraOperator.CurrentHighLowX;
        }
        else
        {
            _currentX = () => _cameraOperator.ReverseHighLowX;
        }
        SwitchLowFuncs(player.FacingForward);

        _targetLowPoint = new(_currentX(), _targetLowPoint.y);
        _lastLowPoint = new(_currentX(), _currentLowPoint.y);
    }

    private void SwitchLowFuncs(bool facingForward)
    {
        Func<float> updateTrailing, updateLeading;

        if (facingForward)
        {
            updateTrailing = TrailingCamLow;
            updateLeading = LeadingCamLow;
        }
        else
        {
            updateTrailing = TrailingPlayerLow;
            updateLeading = LeadingPlayerLow;
        }

        _lowPoints.PositionList.ChangeUpdateFuncs(updateTrailing, updateLeading);
    }

    private void SwitchHighFuncs(bool facingForward)
    {
        Func<float> updateTrailing, updateLeading;

        if (facingForward)
        {
            updateTrailing = TrailingCamHigh;
            updateLeading = LeadingCamHigh;
        }
        else
        {
            updateTrailing = TrailingPlayerHigh;
            updateLeading = LeadingPlayerHigh;
        }

        _lowPoints.PositionList.ChangeUpdateFuncs(updateTrailing, updateLeading);
    }

    public float LeadingCamHigh() => _cameraOperator.gameObject.transform.position.x + HighPointBuffer;
    public float LeadingCamLow() => _cameraOperator.gameObject.transform.position.x + LowPointBuffer;
    public float TrailingCamHigh() => _cameraOperator.gameObject.transform.position.x - HighPointBuffer / 2;
    public float TrailingCamLow() => _cameraOperator.gameObject.transform.position.x - LowPointBuffer / 2;
    public float LeadingPlayerHigh() => _cameraOperator.PlayerBody.position.x + HighPointBuffer / 2;
    public float LeadingPlayerLow() => _cameraOperator.PlayerBody.position.x + LowPointBuffer / 2;
    public float TrailingPlayerHigh() => _cameraOperator.PlayerBody.position.x - HighPointBuffer;
    public float TrailingPlayerLow() => _cameraOperator.PlayerBody.position.x - LowPointBuffer;
    #endregion

}
*/
