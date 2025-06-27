using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour, IOverlayScreen
{
    public TMP_Text timeText;
    private float timeElapsed = 0;
    private bool running = false;
    [SerializeField] private GameObject _display;
    public static Action<float> OnStopTimer { get; set; }

    private void Awake()
    {
        LevelManager.OnAttempt += StartTimer;
        LevelManager.OnGameOver += GameOver;
        LevelManager.OnCrossFinish += FinishLevel;
        LevelManager.OnLanding += Restart;
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

    private void OnDestroy()
    {
        OnStopTimer = null;
        LevelManager.OnAttempt -= StartTimer;
        LevelManager.OnGameOver -= GameOver;
        LevelManager.OnCrossFinish -= FinishLevel;
        LevelManager.OnLanding -= Restart;
    }

    public void StartTimer()
    {
        running = true;
    }

    public void FinishLevel()
    {
        if (!running)
        {
            return;
        }
        StopTimer(true);
    }

    public void GameOver()
    {
        StopTimer(false);
    }

    public void Restart(Level _, PlayerRecord __, ICameraTargetable ___)
    {
        StopTimer(false);
    }

    public void ResetTimer()
    {
        timeElapsed = 0;
        UpdateTimeDisplay(timeElapsed);
    }

    public void StopTimer(bool doTriggerEvent)
    {
        running = false;

        if (doTriggerEvent)
        {
            OnStopTimer?.Invoke(timeElapsed);
        }
    }
    private void UpdateTimeDisplay(float time)
    {
        char[] timerChars = new char[8];
        SecondsToCharArray(time, timerChars);
        timeText.SetCharArray(timerChars);
    }

    public static void SecondsToCharArray(float timeInSeconds, char[] array)
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

    public void ActivateDisplay(bool doActivate)
    {
        _display.SetActive(doActivate);
    }
}
