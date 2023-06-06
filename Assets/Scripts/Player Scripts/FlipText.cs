using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlipText : MonoBehaviour
{
    private float fadeOutTimer = 2;
    private GameObject bird;
    private Vector3 textVelocity;
    public float velocityMultiplier = 250;
    private Rigidbody2D birdBody;
    private Text displayText;
    private bool canceled = false;
    private Color defaultColor;
    private FontStyle defaultFontStyle;
    private IEnumerator activeFadeOut;

    public void StartLifecycle()
    {
        if (canceled)
        {
            canceled = false;
            SetDefaultFont();
        }
        gameObject.SetActive(true);
        StopCoroutine(activeFadeOut);
        activeFadeOut = FadeOut(fadeOutTimer);
        StartCoroutine(activeFadeOut);
    }

    private void Awake()
    {
        displayText = GetComponent<Text>();
        defaultColor = displayText.color;
        defaultFontStyle = displayText.fontStyle;
        activeFadeOut = FadeOut(fadeOutTimer);
    }
    void Start()
    {
        bird = GameObject.FindWithTag("Player");
        birdBody = bird.GetComponent<Rigidbody2D>();
        gameObject.SetActive(false);
    }

    public void SetText(string text)
    {
        displayText.text = text;
        if (birdBody.velocity.x >= 0)
        {
            textVelocity = new Vector3(-velocityMultiplier, velocityMultiplier / 6);
        }
        else
        {
            textVelocity = new Vector3(velocityMultiplier, velocityMultiplier / 6);
        }
    }

    public void CancelText()
    {
        SetCanceledFont();
        displayText.text = "Bummer!";
        canceled = true;
    }


    public bool Canceled
    {
        get
        {
            return canceled;
        }
    }

    private IEnumerator FadeOut(float timeLimit)
    {
        bool faded = false;
        displayText.CrossFadeAlpha(1, 0, false);
        float timer = 0;
        while (timer < timeLimit)
        {
            if (timer > timeLimit * 0.4f && !faded)
            {
                faded = true;
                displayText.CrossFadeAlpha(0, 1, false);
            }
            transform.position += textVelocity * Time.deltaTime;
            textVelocity -= (textVelocity) * 1.75f * Time.deltaTime;
            timer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        gameObject.SetActive(false);
    }

    private void SetCanceledFont()
    {
        displayText.fontStyle = FontStyle.BoldAndItalic;
        displayText.color = Color.gray;
    }

    private void SetDefaultFont()
    {
        displayText.fontStyle = defaultFontStyle;
        displayText.color = defaultColor;
    }
}
