using UnityEngine;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;

public class CameraScript : MonoBehaviour
{
    public Vector3 offset, lowPoint, leadingCorner, trailingCorner;
    public float leadingEdgeOffset = 45;
    private float defaultSize, zoomYDelta = 0, camY, targetY = 0;
    public bool cameraZoomOut = false, cameraZoomIn = false;
    private LiveRunManager runManager;
    private IEnumerator transitionYCoroutine, zoomOutRoutine, zoomInRoutine;
    private Camera cam;
    [SerializeField]
    private Sound wind;

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
        if (cam.WorldToScreenPoint(runManager.PlayerPosition).y < 0) runManager.Fall();
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
        cam = GetComponent<Camera>();
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
        leadingCorner = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        trailingCorner = cam.ViewportToWorldPoint(new Vector3(0, 1, 0));
    }

    public Vector3 LeadingCorner
    {
        get
        {
            return leadingCorner;
        }
    }

    public Vector3 TrailingCorner
    {
        get
        {
            return trailingCorner;
        }
    }

    public Vector3 Center
    {
        get
        {
            return cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
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
        if (!AudioManager.playingSounds.ContainsValue(wind))
        {
            AudioManager.Instance.TimedFadeInZoomFadeOut(wind, 0.5f, 2f, defaultSize);
        }
        cameraZoomOut = true;
        while(runManager.PlayerPosition.y > LeadingCorner.y - cam.orthographicSize * 0.2f || runManager.PlayerBody.velocity.y > 0)
        {
            float change = Mathf.Clamp(runManager.PlayerBody.velocity.y, 0.5f, 99999) * 0.65f * Time.fixedDeltaTime;
            cam.orthographicSize += change;
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
        while(cam.orthographicSize > defaultSize)
        {
            float change = Mathf.Clamp(runManager.PlayerBody.velocity.y, -666, -1) * 0.5f * Time.fixedDeltaTime;
            cam.orthographicSize += change;
            zoomYDelta += change;
            yield return new WaitForFixedUpdate();
        }
        cameraZoomIn = false;
        AudioManager.Instance.StopLoop(wind);
    }

    public float ZoomYDelta
    {
        get
        {
            return zoomYDelta;
        }
    }

    public Camera Camera
    {
        get
        {
            return cam;
        }
    }

    public float DefaultSize
    {
        get
        {
            return defaultSize;
        }
    }
}
