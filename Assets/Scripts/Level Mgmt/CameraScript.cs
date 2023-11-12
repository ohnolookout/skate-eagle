using UnityEngine;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;

public class CameraScript : MonoBehaviour
{
    public Vector3 offset, lowPoint;
    public float leadingEdgeOffset = 0;
    private float defaultSize, zoomYDelta = 0, camY, targetY = 0;
    private bool cameraZoomOut = false, cameraZoomIn = false;
    private LiveRunManager runManager;
    private IEnumerator transitionYCoroutine, zoomOutRoutine, zoomInRoutine;

    void Awake()
    {
        AssignComponents();
    }
    void Start()
    {
        camY = runManager.LowestGroundPoint.y;
        UpdatePosition();
    }

    void Update()
    {
        if (runManager.runState == RunState.Fallen)
        {
            return;
        }
        if (Camera.main.WorldToScreenPoint(runManager.PlayerPosition).y < 0) runManager.Fall();
        UpdateZoom();
        if (runManager.runState != RunState.Finished)
        {
            UpdatePosition();
        }
    }

    private void AssignComponents()
    {
        defaultSize = Camera.main.orthographicSize;
        runManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        transitionYCoroutine = TransitionLowY(runManager.LowestGroundPoint);
    }

    public void UpdateZoom()
    {
        if(cameraZoomOut || runManager.PlayerPosition.y < LeadingCorner.y - Camera.main.orthographicSize * 0.2f)
        {
            return;
        }

        if (cameraZoomIn && runManager.PlayerBody.velocity.y > 0)
        {
            StopCoroutine(zoomInRoutine);
            cameraZoomIn = false;
        }
        else if (!cameraZoomIn)
        {
            zoomOutRoutine = ZoomOut();
            StartCoroutine(zoomOutRoutine);
        }
    }

    public void UpdatePosition()
    {
        if (runManager.LowestGroundPoint.y != targetY)
        {
            StopCoroutine(transitionYCoroutine);
            transitionYCoroutine = TransitionLowY(runManager.LowestGroundPoint);
            StartCoroutine(transitionYCoroutine);
        }
        float cameraX = runManager.PlayerPosition.x + offset.x + (zoomYDelta * (1 / Camera.main.aspect));
        float cameraY = camY + offset.y + zoomYDelta;
        transform.position = new Vector3(cameraX, cameraY, transform.position.z);
    }

    public Vector3 LeadingCorner
    {
        get
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        }
    }

    public Vector3 TrailingCorner
    {
        get
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        }
    }

    public Vector3 Center
    {
        get
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        }
    }

    private IEnumerator TransitionLowY(Vector3 endPoint)
    {
        float startBirdX = runManager.PlayerPosition.x;
        float distance = Mathf.Clamp(Mathf.Abs(endPoint.x - (runManager.PlayerPosition.x + 20)), 15, 100);
        float startY = camY;
        targetY = endPoint.y;
        float t = 0;
        while(Mathf.Abs(camY - endPoint.y) > 0.2)
        {
            t = Mathf.Clamp01(Mathf.Abs(runManager.PlayerPosition.x - startBirdX) / distance);
            camY = Mathf.SmoothStep(startY, targetY, t);
            yield return null;
        }
    }

    private IEnumerator ZoomOut()
    {
        cameraZoomOut = true;
        while(runManager.PlayerPosition.y > LeadingCorner.y - Camera.main.orthographicSize * 0.2f || runManager.PlayerBody.velocity.y > 0)
        {
            float change = Mathf.Clamp(runManager.PlayerBody.velocity.y, 0.5f, 99999) * 0.65f * Time.fixedDeltaTime;
            Camera.main.orthographicSize += change;
            zoomYDelta += change;
            yield return new WaitForFixedUpdate();
        }
        cameraZoomOut = false;
        if (cameraZoomIn) StopCoroutine(zoomInRoutine);
        zoomInRoutine = ZoomIn();
        StartCoroutine(zoomInRoutine);
    }

    private IEnumerator ZoomIn()
    {
        cameraZoomIn = true;
        while(Camera.main.orthographicSize > defaultSize)
        {
            float change = Mathf.Clamp(runManager.PlayerBody.velocity.y, -666, -1) * 0.5f * Time.fixedDeltaTime;
            Camera.main.orthographicSize += change;
            zoomYDelta += change;
            yield return new WaitForFixedUpdate();
        }
        cameraZoomIn = false;
    }

    public float ZoomYDelta
    {
        get
        {
            return zoomYDelta;
        }
    }
}
