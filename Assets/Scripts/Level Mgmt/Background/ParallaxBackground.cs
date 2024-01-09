using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{

    private float defaultPanelLength, defaultHalfLayerWidth, lastShiftX;
    private Transform[] panels = new Transform[3];
    private Vector3 startPosition;
    private Camera cam;
    private ICameraOperator camScript;
    private SpriteRenderer[] spriteRenderers = new SpriteRenderer[3];
    public float parallaxMagnitude;
    public BackgroundContainer container;
    private int currentCenterPanel = 1;

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            panels[i] = transform.GetChild(i);
            spriteRenderers[i] = panels[i].gameObject.GetComponent<SpriteRenderer>();
        }
        defaultPanelLength = spriteRenderers[1].bounds.size.x;
        defaultHalfLayerWidth = defaultPanelLength * 1.5f;
        cam = Camera.main;
        camScript = cam.GetComponent<ICameraOperator>();
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);
        startPosition = transform.position;
        lastShiftX = startPosition.x;
    }

    void FixedUpdate()
    {
        float currentHalfLayerWidth = spriteRenderers[1].bounds.size.x * 1.5f;
        float xDelta = cam.transform.position.x * parallaxMagnitude;
        float camLayerDelta = cam.transform.position.x * (1 - parallaxMagnitude);
        float expectedPercentWidthFromCamera = camLayerDelta / defaultHalfLayerWidth;
        float currentPercentWidthFromCamera = camLayerDelta / currentHalfLayerWidth;
        float lengthDifference = (expectedPercentWidthFromCamera - currentPercentWidthFromCamera) * currentHalfLayerWidth;
        transform.position = new Vector3(startPosition.x + xDelta - lengthDifference, transform.position.y, transform.position.z);
        if (spriteRenderers[LeadingPanel()].bounds.max.x <= camScript.LeadingCorner.x)
        {
            ShiftPanelRight();
        } else if(spriteRenderers[TrailingPanel()].bounds.min.x >= camScript.TrailingCorner.x)
        {
            ShiftPanelLeft();
        }
    }

    private void ShiftPanelRight()
    {
        int panelToShift = TrailingPanel();
        panels[panelToShift].transform.position = new Vector3(panels[panelToShift].transform.position.x + spriteRenderers[0].bounds.size.x * 3, panels[panelToShift].transform.position.y, panels[panelToShift].transform.position.z);
        if (currentCenterPanel < 2)
        {
            currentCenterPanel++;
        }
        else
        {
            currentCenterPanel = 0;
        }
    }

    private void ShiftPanelLeft()
    {
        int panelToShift = LeadingPanel();
        panels[panelToShift].transform.position = new Vector3(panels[panelToShift].transform.position.x - spriteRenderers[0].bounds.size.x * 3, panels[panelToShift].transform.position.y, panels[panelToShift].transform.position.z);
        if (currentCenterPanel > 0)
        {
            currentCenterPanel--;
        }
        else
        {
            currentCenterPanel = 2;
        }

    }

    private int TrailingPanel()
    {
        if (currentCenterPanel > 0)
        {
            return currentCenterPanel - 1;
        }
        else
        {
            return 2;
        }
    }

    private int LeadingPanel()
    {
            if (currentCenterPanel < 2)
            {
                return currentCenterPanel + 1;
            }
            else
            {
                return 0;
            }
    }
}
