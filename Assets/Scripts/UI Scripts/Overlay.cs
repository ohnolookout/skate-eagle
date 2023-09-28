using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class Overlay : MonoBehaviour
{
    public GameObject gameOver, finish, standby, mobileControls, hud, landing;
    public LandingScreenLoader landingLoader;
    public FinishScreenLoader finishLoader;
    public LiveRunManager runManager;
    public StompBar stompBar;
    public Timer timer;
    //public OverlayManager overlayManager;

    public void StartScreen(LevelRecords playerInfo)
    {
        landing.SetActive(true);
        gameOver.SetActive(false);
        finish.SetActive(false);
        standby.SetActive(false);
        hud.SetActive(false);
        ActivateControls(false);
        landingLoader.GenerateLanding(runManager.CurrentLevel, playerInfo);
    }

    public void StandbyScreen()
    {
        landing.SetActive(false);
        standby.SetActive(true);
        hud.SetActive(true);
        ActivateControls(true);
        runManager.runState = RunState.Standby;
    }


    public void StartAttempt()
    {
        standby.SetActive(false);
        timer.StartTimer();
    }

    public void GameOverScreen()
    {
        gameOver.SetActive(true);
        timer.StopTimer();
        ActivateControls(false);
    }

    public void GenerateFinishScreen(FinishScreenData screenData)
    {
        finishLoader.GenerateFinishScreen(screenData);
    }

    public void ActivateFinishScreen()
    {
        finish.SetActive(true);
        hud.SetActive(false);
        ActivateControls(false);
    }

    public void FillStompBar(float fillAmount)
    {
        stompBar.Fill(fillAmount);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Start_Menu");
    }

    public void ActivateControls(bool activate)
    {
        if(mobileControls is null)
        {
            return;
        }
        mobileControls.SetActive(activate);
    }

    public void RestartLevel()
    {
        runManager.RestartGame();
    }

    public float StopTimer()
    {
        return timer.StopTimer();
    }


}


