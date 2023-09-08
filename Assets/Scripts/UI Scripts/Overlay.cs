using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum LevelStatus { Uncompleted, MedalTimesUnlocked, LeaderboardUnlocked}

public class Overlay : MonoBehaviour
{
    private Sprite[] medals, smallMedals;
    public GameObject gameOver, finish, standby, hud, landing;
    public StompBar stompBar;
    public Timer timer;
    private string[] medalTimeTexts = new string[3];
    private Sprite[] medalSprites = new Sprite[3];
    private float bestTime;
    private int strikeThroughIndex;
    private LevelTimeData playerTimes;
    private Level currentLevel;

    private void Awake()
    {
        medals = Resources.LoadAll<Sprite>("Sprites/Medals/Medal Sheet.png");
        smallMedals = Resources.LoadAll<Sprite>("Sprites/Medals/Small Medal Sheet");
        Debug.Log($"Loaded small medal sheet: {smallMedals.Length}");
    }

    public void SetLevelData(LevelTimeData playerTime)
    {
        playerTimes = playerTime;
        currentLevel = playerTime.level;
    }

    public void GameOverScreen()
    {
        timer.StopTimer();
        gameOver.SetActive(true);
        finish.SetActive(false);
        standby.SetActive(false);
        hud.SetActive(false);
    }

    public void StartScreen(Level level, LevelTimeData playerTime)
    {
        BuildMedalTimes(level, playerTime);
        gameOver.SetActive(false);
        finish.SetActive(false);
        standby.SetActive(true);
        hud.SetActive(true);
    }

    public float FinishScreen(LevelTimeData playerTimeData)
    {
        float finishTime = timer.StopTimer();
        gameOver.SetActive(false);
        finish.SetActive(true);
        standby.SetActive(false);
        hud.SetActive(false);
        return finishTime;
    }

    public void StartAttempt()
    {
        timer.StartTimer();
        gameOver.SetActive(false);
        finish.SetActive(false);
        standby.SetActive(false);
        hud.SetActive(true);
    }

    public void FillStompBar(float fillAmount)
    {
        stompBar.Fill(fillAmount);
    }

    private void BuildMedalTimes(Level level, LevelTimeData playerTime)
    {
        strikeThroughIndex = 3;
        Medal bestMedal = playerTime.medal;
        int lowestMedalIndex = LowestMedalToDisplay(bestMedal);
        for (int i = 0; i < 3; i++)
        {
            float currentMedalTime = level.MedalTimes.TimeFromMedal((Medal)(i + 1));
            medalTimeTexts[i] = FormatTime(currentMedalTime);
            if (lowestMedalIndex + i >= (int)bestMedal)
            {
                
            }
        }
    }


    private int LowestMedalToDisplay(Medal bestMedal)
    {
        int lowestMedalIndex = 4;
        if (bestMedal == Medal.Red)
        {
            lowestMedalIndex = 2;
        }
        else if (bestMedal != Medal.Bronze && bestMedal != Medal.Participant)
        {
            lowestMedalIndex = (int)Mathf.Min((float)bestMedal, 3);
        }
        return lowestMedalIndex;
    }

    public static string FormatTime(float time)
    {
        int milliseconds = (int)(time * 1000);
        int totalSeconds = milliseconds / 1000;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int remainingMilliseconds = milliseconds % 1000;

        return string.Format("{0}:{1:D2}.{2:D3}", minutes, seconds, remainingMilliseconds);
    }

}


