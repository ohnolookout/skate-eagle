using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundContainer : MonoBehaviour
{
    private Vector3 startPosition, startScale;
    private Camera mainCam;
    private CameraScript camScript;
    private float camSize;
    public float scaleRatio, scaleChange;

    private void Awake()
    {
        startPosition = transform.position;
        startScale = transform.localScale;
    }
    void Start()
    {
        mainCam = Camera.main;
        camScript = mainCam.GetComponent<CameraScript>();
        camSize = mainCam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        float camSizeChange = mainCam.orthographicSize - camSize;
        scaleChange = (camSizeChange / camSize) * scaleRatio;
        transform.localScale = startScale * (1 + scaleChange);
        transform.localPosition = startPosition - new Vector3(0, camScript.ZoomYDelta/4, 0);
    }

}
