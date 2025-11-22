using GooglePlayGames.BasicApi;
using TMPro;
using UnityEngine;

public class CameraMover
{
    public const float DefaultXStep = 0.3f;
    public float xStep = DefaultXStep;
    public const float DefaultYStep = 0.25f;
    public float yStep = DefaultYStep;
    public const float DefaultZoomStep = 0.15f;
    public float zoomStep = DefaultZoomStep;
    public const float MaxYDampen = 0.2f;
    private const float _maxXDeltaPercent = .2f;
    private const float _maxYDeltaPercent = .15f;
    private const float _maxOrthoDeltaPercent = .04f;
    private float _lastXDelta = 0;
    private float _lastYDelta = 0;
    private float _lastOrthoDelta = 0;
    private const float MaxXAccel = .3f;
    private const float MaxYAccel = .3f;
    private const float MaxOrthoAccel = 0.05f;
    private bool _undoDirectionChangeDampen = false;

    private Camera _camera;
    private CameraManager _cameraManager;

    public CameraMover(CameraManager cameraManager)
    {
        _cameraManager = cameraManager;
        _camera = cameraManager.Camera;
    }

    public void UpdatePosition(CameraTargeter targeter)
    {
        UpdatePosition(targeter.TargetPosition, targeter.TargetOrthoSize);
    }

    public void UpdatePosition(Vector3 targetPosition, float targetSize)
    {
        if (_undoDirectionChangeDampen)
        {
            UndoDampen();
        }
        var camPos = _camera.transform.position;
        var playerViewportPoint = _camera.WorldToViewportPoint(_cameraManager.player.NormalBody.position);

        //Find y delta
        var yDelta = FindDelta(camPos.y, targetPosition.y, yStep, _maxYDeltaPercent, _lastYDelta, MaxYAccel);

        if (playerViewportPoint.y < .05f && yDelta < 0)
        {
            yDelta *= 1.25f;
        }

        //Find x delta
        var xDelta = FindDelta(camPos.x, targetPosition.x, xStep, _maxXDeltaPercent, _lastXDelta, MaxXAccel);

        if (playerViewportPoint.x < .1f && xDelta < 0)
        {
            xDelta *= 1.25f;
        }
        else if (playerViewportPoint.x > .9f && xDelta > 0)
        {
            xDelta *= 1.25f;
        }

        //Find ortho delta
        var orthoDelta = FindDelta(_camera.orthographicSize, targetSize, zoomStep, _maxOrthoDeltaPercent, _lastOrthoDelta, MaxOrthoAccel);

        _camera.transform.Translate(xDelta, yDelta, 0);
        _camera.orthographicSize += orthoDelta;

        _lastXDelta = xDelta;
        _lastYDelta = yDelta;
        _lastOrthoDelta = orthoDelta;
    }


    private float FindDelta(float currentVal, float targetVal, float step, float maxDeltaPercent, float lastDelta, float maxAccel)
    {
        var totalDifference = targetVal - currentVal;
        var maxDelta = maxDeltaPercent * _camera.orthographicSize;

        var lerpedVal = Mathf.SmoothStep(currentVal, targetVal, step);
        var desiredDelta = lerpedVal - currentVal;
        var delta = desiredDelta;
        var desiredDeltaSign = Mathf.Sign(desiredDelta);

        if (Mathf.Abs(desiredDelta) > maxDelta)
        {
            delta = maxDelta * desiredDeltaSign;
        }

        var deltaAccel = delta - lastDelta;

        //Slow down as target is approached.
        if (lastDelta > maxDelta / 3 && totalDifference / lastDelta < 6)
        {
            delta = lastDelta * 0.75f;
        }
        else if (Mathf.Abs(deltaAccel) > maxAccel)
        {
            delta = lastDelta + (maxAccel * Mathf.Sign(deltaAccel));
        }

        //Reduce overshoot
        if (Mathf.Sign(delta) != desiredDeltaSign)
        {
            delta *= 0.5f;
        }
        ;

        return delta;
    }

    private void UndoDampen()
    {
        xStep = Mathf.SmoothStep(xStep, DefaultXStep, 0.1f);
        yStep = Mathf.SmoothStep(yStep, DefaultYStep, 0.1f);
        zoomStep = Mathf.SmoothStep(zoomStep, DefaultZoomStep, 0.1f);
        if (Mathf.Abs(DefaultXStep - xStep) < 0.02 && Mathf.Abs(DefaultYStep - yStep) < 0.02)
        {
            xStep = DefaultXStep;
            yStep = DefaultYStep;
            zoomStep = DefaultZoomStep;
            _undoDirectionChangeDampen = false;
        }
    }

    public void MoveToStart(SerializedStartLine startLine)
    {
        SetPosition(startLine.CamStartPosition, startLine.CamOrthoSize);
        ResetVariables();
    }

    public void SetPosition(Vector3 position, float orthoSize)
    {
        _camera.transform.position = position;
        _camera.orthographicSize = orthoSize;
    }

    public void ResetVariables()
    {
        xStep = DefaultXStep;
        yStep = DefaultYStep;
        zoomStep = DefaultZoomStep;
        _lastXDelta = 0;
        _lastYDelta = 0;
        _lastOrthoDelta = 0;
        _undoDirectionChangeDampen = false;        
    }

    public void OnExitDirectionChange()
    {
        _undoDirectionChangeDampen = true;
    }
}
