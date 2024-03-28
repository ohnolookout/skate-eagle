using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseText : MonoBehaviour
{
    private CanvasRenderer canvas;
    private bool fadeOut = true;
    private float alpha = 1, alphaChange = 0.12f;
    // Start is called before the first frame update
    private void Awake()
    {
        canvas = gameObject.GetComponent<CanvasRenderer>();
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float change = Time.deltaTime * 10 * alphaChange;
        canvas.SetAlpha(alpha);
        if (fadeOut)
        {
            alpha -= change + change * (1f - alpha);
        }
        else
        {
            alpha += change + change * (1f - alpha);
        }
        if(alpha >= 1 || alpha <= 0)
        {
            fadeOut = !fadeOut;
            alpha = Mathf.Clamp01(alpha);
        }
    }
}
