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
    private IPlayer player;


    private void OnEnable()
    {
        IPlayer.OnStartWithStomp += (player) => Fill((float)player.StompCharge / (float)player.StompThreshold);
        IPlayer.OnFlip += (player, _) => Fill((float)player.StompCharge / (float)player.StompThreshold);
        IPlayer.OnStomp += () => Fill(0);
    }
    private void OnDisable()
    {
    }

    public void Fill(float fillAmount)
    {
        if (filling)
        {
            StopCoroutine(fillRoutine);
        }
        fillRoutine = FillRoutine(fillAmount);
        StartCoroutine(fillRoutine);
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
