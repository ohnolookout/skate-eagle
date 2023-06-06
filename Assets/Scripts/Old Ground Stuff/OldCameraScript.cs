using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System;

public class OldCameraScript : MonoBehaviour
{
    public Transform bird;
    public Rigidbody2D birdBody;
    public SpriteShapeController shapeController;
    public Vector3 offset;
    private OldGroundSpline groundSpline;
    public Vector3 lowPoint;
    public float leadingEdgeOffset = 0;
    private Vector3 leadingCorner;
    private float defaultSize;
    private float cameraScaleTimer = 0;
    private bool cameraZoomout = false;
    private float zoomYDelta = 0;
    private Vector3 lastLeadingCorner;
    private EagleScript eagleScript;
    private LogicScript logic;
    void Start()
    {
        defaultSize = Camera.main.orthographicSize;
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        bird = GameObject.FindWithTag("Player").transform;
        eagleScript = bird.gameObject.GetComponent<EagleScript>();
        shapeController = GameObject.FindWithTag("Ground").GetComponent<SpriteShapeController>();
        birdBody = bird.GetComponent<Rigidbody2D>();
        groundSpline = shapeController.GetComponent<OldGroundSpline>();
        groundSpline.OldCameraFloor.GenerateMidpointsFromX(shapeController, NewLeadingCorner.x - leadingEdgeOffset);
        UpdatePosition();
        groundSpline.OldCameraFloor.GenerateMidpointsFromX(shapeController, NewLeadingCorner.x - leadingEdgeOffset);
        //UpdatePosition();
    }

    void Update()
    {
        if (!eagleScript.Fallen)
        {
            UpdateZoom();
            if (!logic.Finished)
            {
                UpdatePosition();
                Vector3 birdPosition = Camera.main.WorldToScreenPoint(bird.position);
                if (Camera.main.WorldToScreenPoint(bird.position).y < 0)
                {
                    eagleScript.Fallen = true;
                }
            }
        }
    }

    public void UpdateZoom()
    {
        lastLeadingCorner = leadingCorner;
        leadingCorner = NewLeadingCorner;
        if (cameraZoomout)
        {
            cameraScaleTimer += 1 * Time.deltaTime;
        }
        if (bird.position.y > leadingCorner.y - Camera.main.orthographicSize * 0.2f)
        {
            cameraZoomout = true;
            cameraScaleTimer = 0;
        }
        if (birdBody.velocity.y > 0 && cameraZoomout)
        {
            cameraScaleTimer = 0;
            Camera.main.orthographicSize += (Math.Abs(birdBody.velocity.y) / 2 + 6) * Time.deltaTime;
        }
        else if (Camera.main.orthographicSize > defaultSize && (cameraScaleTimer >= 0.6f || !cameraZoomout))
        {
            Camera.main.orthographicSize -= (Math.Abs(birdBody.velocity.y) / 2 + 2) * Time.deltaTime;
            cameraScaleTimer = 0;
            cameraZoomout = false;
        }
    }

    public void UpdatePosition()
    {
        lastLeadingCorner = leadingCorner;
        leadingCorner = NewLeadingCorner;
        lowPoint = groundSpline.transform.TransformPoint(groundSpline.OldCameraFloor.GetLowPoint(leadingCorner.x - leadingEdgeOffset));
        float camY;
        if (Camera.main.orthographicSize > defaultSize)
        {
            zoomYDelta += (leadingCorner.y - lastLeadingCorner.y);
        }
        else
        {
            zoomYDelta = 0;
        }
        camY = lowPoint.y + offset.y + zoomYDelta;
        if(camY.ToString() == "NaN")
        {
            camY = transform.position.y;
        }
        transform.position = new Vector3(bird.position.x + offset.x, camY, transform.position.z + offset.z);
    }

    public Vector3 NewLeadingCorner
    {
        get
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        }
    }

    public Vector3 NewTrailingCorner
    {
        get
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        }
    }
}
