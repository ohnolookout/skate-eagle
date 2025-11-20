using GooglePlayGames.BasicApi;
using TMPro;
using UnityEngine;

public class CameraMover
{
    private bool undoDirectionChangeDampen = false;
    public const float defaultXStep = 0.35f;
    public float xStep = defaultXStep;
    public const float defaultYStep = 0.2f;
    public float yStep = defaultYStep;
    public const float defaultZoomStep = 0.2f;
    public float zoomStep = defaultZoomStep;
    public const float maxYDampen = 0.2f;
    private const float maxXDeltaPercent = .2f;
    private const float maxYDeltaPercent = .15f;
    private const float maxOrthoDeltaPercent = .04f;
    private float lastXDelta = 0;
    private float lastYDelta = 0;
    private float lastOrthoDelta = 0;
    private const float maxXAccel = .3f;
    private const float maxYAccel = .3f;
    private const float maxOrthoAccel = 0.05f;

    private Camera _camera;
    private CameraManager _cameraManager;

    public CameraMover(CameraManager cameraManager)
    {
        _cameraManager = cameraManager;
        _camera = cameraManager.camera;
    }

    public void UpdatePosition(CameraTargeter targeter)
    {
        UpdatePosition(targeter.TargetPosition, targeter.TargetOrthoSize);
    }

    public void UpdatePosition(Vector3 targetPosition, float targetSize)
    {
        if (undoDirectionChangeDampen)
        {
            UndoDampen();
        }
        var camPos = _camera.transform.position;
        var playerViewportPoint = _camera.WorldToViewportPoint(_cameraManager.player.NormalBody.position);

        //Find y delta
        var yDelta = FindDelta(camPos.y, targetPosition.y, yStep, maxYDeltaPercent, lastYDelta, maxYAccel);

        if (playerViewportPoint.y < .05f && yDelta < 0)
        {
            yDelta *= 1.25f;
        }

        //Find x delta
        var xDelta = FindDelta(camPos.x, targetPosition.x, xStep, maxXDeltaPercent, lastXDelta, maxXAccel);

        if (playerViewportPoint.x < .1f && xDelta < 0)
        {
            xDelta *= 1.25f;
        }
        else if (playerViewportPoint.x > .9f && xDelta > 0)
        {
            xDelta *= 1.25f;
        }

        //Find ortho delta
        var orthoDelta = FindDelta(_camera.orthographicSize, targetSize, zoomStep, maxOrthoDeltaPercent, lastOrthoDelta, maxOrthoAccel);

        _camera.transform.Translate(xDelta, yDelta, 0);
        _camera.orthographicSize += orthoDelta;

        lastXDelta = xDelta;
        lastYDelta = yDelta;
        lastOrthoDelta = orthoDelta;
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
            Debug.Log("Slowing down as target approached.");
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
        xStep = Mathf.SmoothStep(xStep, defaultXStep, 0.1f);
        yStep = Mathf.SmoothStep(yStep, defaultYStep, 0.1f);
        zoomStep = Mathf.SmoothStep(zoomStep, defaultZoomStep, 0.1f);
        if (Mathf.Abs(defaultXStep - xStep) < 0.02 && Mathf.Abs(defaultYStep - yStep) < 0.02)
        {
            xStep = defaultXStep;
            yStep = defaultYStep;
            zoomStep = defaultZoomStep;
            undoDirectionChangeDampen = false;
        }
    }

    public void MoveToStart(SerializedStartLine startLine)
    {
        _camera.transform.position = startLine.CamStartPosition;
        _camera.orthographicSize = startLine.CamOrthoSize;
        ResetVariables();
    }

    public void ResetVariables()
    {
        xStep = defaultXStep;
        yStep = defaultYStep;
        zoomStep = defaultZoomStep;
        lastXDelta = 0;
        lastYDelta = 0;
        lastOrthoDelta = 0;
        undoDirectionChangeDampen = false;        
    }

    public void OnExitDirectionChange()
    {
        undoDirectionChangeDampen = true;
    }
}
