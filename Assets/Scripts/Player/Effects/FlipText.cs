using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlipText : MonoBehaviour
{
    private float _fadeOutTimer = 2;
    private Vector3 textVelocity;
    [SerializeField] private float _velocityMultiplier = 250;
    [SerializeField] private Text _displayText;
    private bool _canceled = false;
    private Color _defaultColor;
    private FontStyle _defaultFontStyle;
    private IEnumerator _activeFadeOut;

    public void StartLifecycle()
    {
        if (_canceled)
        {
            _canceled = false;
            SetToDefaultFont();
        }
        gameObject.SetActive(true);
        StopCoroutine(_activeFadeOut);
        _activeFadeOut = FadeOut(_fadeOutTimer);
        StartCoroutine(_activeFadeOut);
    }

    private void Awake()
    {
        _displayText = GetComponent<Text>();
        _defaultColor = _displayText.color;
        _defaultFontStyle = _displayText.fontStyle;
        _activeFadeOut = FadeOut(_fadeOutTimer);
    }

    public void SetText(string text, Vector2 playerVelocity)
    {
        _displayText.text = text;
        if (playerVelocity.x >= 0)
        {
            textVelocity = new Vector3(-_velocityMultiplier, _velocityMultiplier / 6);
        }
        else
        {
            textVelocity = new Vector3(_velocityMultiplier, _velocityMultiplier / 6);
        }
    }

    public void CancelText()
    {
        SetCanceledFont();
        _displayText.text = "Bummer!";
        _canceled = true;
    }


    public bool Canceled => _canceled;

    private IEnumerator FadeOut(float timeLimit)
    {
        bool faded = false;
        _displayText.CrossFadeAlpha(1, 0, false);
        float timer = 0;
        while (timer < timeLimit)
        {
            if (timer > timeLimit * 0.4f && !faded)
            {
                faded = true;
                _displayText.CrossFadeAlpha(0, 1, false);
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
        _displayText.fontStyle = FontStyle.BoldAndItalic;
        _displayText.color = Color.gray;
    }

    private void SetToDefaultFont()
    {
        _displayText.fontStyle = _defaultFontStyle;
        _displayText.color = _defaultColor;
    }
}
