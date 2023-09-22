using UnityEngine;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;

public class CameraScript : MonoBehaviour
{
    private Transform bird;
    private Rigidbody2D birdBody;
    private GroundSpawner groundSpawner;
    public Vector3 offset, lowPoint;
    public float leadingEdgeOffset = 0;
    private float defaultSize, zoomYDelta = 0, camY, targetY = 0;
    private bool cameraZoomOut = false, cameraZoomIn = false;
    private LiveRunManager logic;
    private IEnumerator transitionYCoroutine, zoomOutRoutine, zoomInRoutine;

    void Awake()
    {
        AssignComponents();
    }
    void Start()
    {
        camY = groundSpawner.LowestPoint.y;
        UpdatePosition();
    }

    void Update()
    {
        if (logic.runState == RunState.Fallen)
        {
            return;
        }
        if (Camera.main.WorldToScreenPoint(bird.position).y < 0) logic.Fall();
        UpdateZoom();
        if (logic.runState != RunState.Finished)
        {
            UpdatePosition();
        }
    }

    private void AssignComponents()
    {
        defaultSize = Camera.main.orthographicSize;
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        bird = GameObject.FindWithTag("Player").transform;
        groundSpawner = GameObject.FindWithTag("GroundSpawner").GetComponent<GroundSpawner>();
        birdBody = bird.GetComponent<Rigidbody2D>();
        transitionYCoroutine = TransitionLowY(groundSpawner.LowestPoint);
    }

    public void UpdateZoom()
    {
        if(cameraZoomOut || bird.position.y < LeadingCorner.y - Camera.main.orthographicSize * 0.2f)
        {
            return;
        }

        if (cameraZoomIn && birdBody.velocity.y > 0)
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
        if (groundSpawner.LowestPoint.y != targetY)
        {
            StopCoroutine(transitionYCoroutine);
            transitionYCoroutine = TransitionLowY(groundSpawner.LowestPoint);
            StartCoroutine(transitionYCoroutine);
        }
        float cameraX = bird.position.x + offset.x + (zoomYDelta * (1 / Camera.main.aspect));
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

    private IEnumerator TransitionLowY(Vector3 endPoint)
    {
        float startBirdX = bird.position.x;
        float distance = Mathf.Clamp(Mathf.Abs(endPoint.x - (bird.position.x + 20)), 15, 100);
        float startY = camY;
        targetY = endPoint.y;
        float t = 0;
        while(Mathf.Abs(camY - endPoint.y) > 0.2)
        {
            t = Mathf.Clamp01(Mathf.Abs(bird.position.x - startBirdX) / distance);
            camY = Mathf.SmoothStep(startY, targetY, t);
            yield return null;
        }
    }

    private IEnumerator ZoomOut()
    {
        cameraZoomOut = true;
        while(bird.position.y > LeadingCorner.y - Camera.main.orthographicSize * 0.2f || birdBody.velocity.y > 0)
        {
            float change = Mathf.Clamp(birdBody.velocity.y, 0.5f, 99999) * 0.65f * Time.fixedDeltaTime;
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
            float change = Mathf.Clamp(birdBody.velocity.y, -666, -1) * 0.5f * Time.fixedDeltaTime;
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
