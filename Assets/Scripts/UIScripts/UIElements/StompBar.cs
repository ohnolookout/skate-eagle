using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StompBar : MonoBehaviour
{
    private bool filling = false;
    public GameObject stompBlast;
    [SerializeField] private Slider slider;
    private IEnumerator fillRoutine;
    public Sprite jaggedOutline, smoothOutline;
    [SerializeField] private CutoutMask fill;


    private void OnEnable()
    {
        Player player = FindFirstObjectByType<Player>();
        player.EventAnnouncer.SubscribeToEvent(PlayerEvent.StartWithStomp, (player) => Fill((float)player.Params.StompCharge / (float)player.Params.StompThreshold));
        player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Flip, (player) => Fill((float)player.Params.StompCharge / (float)player.Params.StompThreshold));
        player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Stomp, (_) => Fill(0));
    }

    public void Fill(float fillAmount)
    {
        if (filling)
        {
            StopCoroutine(fillRoutine);
        }
        if (gameObject.activeInHierarchy)
        {
            fillRoutine = FillRoutine(fillAmount);
            StartCoroutine(fillRoutine);
        }
    }
    private IEnumerator FillRoutine(float fillAmount)
    {
        filling = true;
        if (fillAmount == 0)
        {
            stompBlast.SetActive(false);
            fill.sprite = smoothOutline;
        }
        while (!FastApproximately(slider.value, fillAmount, 0.05f))
        {
            slider.value = Mathf.SmoothStep(slider.value, fillAmount, 0.2f);
            yield return null;
        }
        slider.value = fillAmount;
        if (fillAmount == 1)
        {
            fill.sprite = jaggedOutline;
            stompBlast.SetActive(true);
        }
        filling = false;
    }
    public static bool FastApproximately(float a, float b, float threshold)
    {
        return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
    }

}
