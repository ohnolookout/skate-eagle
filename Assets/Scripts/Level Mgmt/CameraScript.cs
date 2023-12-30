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
    private bool isTracking = true, isFinished = false;

    void Awake()
    {
        AssignComponents();
    }
    void Start()
    {
        camY = runManager.GroundSpawner.LowestPoint.y;
        UpdatePosition();
    }

    void Update()
    {
        if (!isTracking)
        {
            return;
        }
        if (cam.WorldToScreenPoint(runManager.Player.Rigidbody.position).y < 0) runManager.Fall();
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
        transitionYCoroutine = TransitionLowY(runManager.GroundSpawner.LowestPoint);
        cam = GetComponent<Camera>();
    }

    public void UpdateZoom()
    {
        if(cameraZoomOut || runManager.Player.Rigidbody.position.y < LeadingCorner.y - Camera.main.orthographicSize * 0.2f)
        {
            return;
        }

        if (cameraZoomIn && runManager.Player.Rigidbody.velocity.y > 0)
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
        if (runManager.GroundSpawner.LowestPoint.y != targetY)
        {
            StopCoroutine(transitionYCoroutine);
            transitionYCoroutine = TransitionLowY(runManager.GroundSpawner.LowestPoint);
            StartCoroutine(transitionYCoroutine);
        }
        float cameraX = runManager.Player.Rigidbody.position.x + offset.x + (zoomYDelta * (1 / Camera.main.aspect));
        float cameraY = camY + offset.y + zoomYDelta;
        transform.position = new Vector3(cameraX, cameraY, transform.position.z);
        leadingCorner = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        trailingCorner = cam.ViewportToWorldPoint(new Vector3(0, 1, 0));
    }
    private IEnumerator TransitionLowY(Vector3 endPoint)
    {
        float startBirdX = runManager.Player.Rigidbody.position.x;
        float distance = Mathf.Clamp(Mathf.Abs(endPoint.x - (runManager.Player.Rigidbody.position.x + 20)), 15, 100);
        float startY = camY;
        targetY = endPoint.y;
        float t = 0;
        while(Mathf.Abs(camY - endPoint.y) > 0.2)
        {
            t = Mathf.Clamp01(Mathf.Abs(runManager.Player.Rigidbody.position.x - startBirdX) / distance);
            camY = Mathf.SmoothStep(startY, targetY, t);
            yield return null;
        }
    }

    private IEnumerator ZoomOut()
    {
        if (!AudioManager.playingSounds.ContainsValue(wind))
        {
            AudioManager.Instance.TimedFadeInZoomFadeOut(wind, 0.5f, 3f, defaultSize * 2f);
        }
        cameraZoomOut = true;
        while(runManager.Player.Rigidbody.position.y > LeadingCorner.y - cam.orthographicSize * 0.2f 
            || runManager.Player.Rigidbody.velocity.y > 0)
        {
            float change = Mathf.Clamp(runManager.Player.Rigidbody.velocity.y, 0.5f, 99999) * 0.65f * Time.fixedDeltaTime;
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
            float change = Mathf.Clamp(runManager.Player.Rigidbody.velocity.y, -666, -1) * 0.5f * Time.fixedDeltaTime;
            cam.orthographicSize += change;
            zoomYDelta += change;
            yield return new WaitForFixedUpdate();
        }
        cameraZoomIn = false;
        AudioManager.Instance.StopLoop(wind);
    }
    public Vector3 LeadingCorner { get => leadingCorner; }
    public Vector3 TrailingCorner { get => trailingCorner; }
    public Vector3 Center { get => cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)); }
    public float ZoomYDelta { get => zoomYDelta; }
    public Camera Camera { get => cam; }
    public float DefaultSize { get => defaultSize; }
}
