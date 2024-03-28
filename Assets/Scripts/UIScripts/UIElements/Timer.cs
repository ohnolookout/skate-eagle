using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    public TMP_Text timeText;
    private char[] timerChars = new char[8];
    private float timeElapsed = 0;
    private bool running = false;
    public static Action<float> OnStopTimer;

    private void OnEnable()
    {
        LevelManager.OnAttempt += StartTimer;
        LevelManager.OnGameOver += _ => StopTimer();
    }

    void Update()
    {
        if (!running)
        {
            return;
        }
        timeElapsed += Time.deltaTime;
        if (Time.frameCount % 20 != 0)
        {
            UpdateTimeDisplay(timeElapsed);
        }
    }

    public void StartTimer()
    {
        running = true;
    }

    public float StopTimer()
    {
        running = false;
        return timeElapsed;
    }
    private void UpdateTimeDisplay(float time)
    {
        SecondsToCharArray(time, timerChars);
        timeText.SetCharArray(timerChars);
    }

    private static void SecondsToCharArray(float timeInSeconds, char[] array)
    {
        int minutes = (int)(timeInSeconds / 60f);
        array[0] = (char)(48 + (minutes % 10));
        array[1] = ':';

        int seconds = (int)(timeInSeconds - minutes * 60);
        array[2] = (char)(48 + seconds / 10);
        array[3] = (char)(48 + seconds % 10);
        array[4] = '.';

        int milliseconds = (int)((timeInSeconds % 1) * 1000);
        array[5] = (char)(48 + milliseconds / 100);
        array[6] = (char)(48 + (milliseconds % 100) / 10);
        array[7] = (char)(48 + milliseconds % 10);
    }
}
