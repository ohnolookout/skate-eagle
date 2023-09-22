using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class Overlay : MonoBehaviour
{
    public GameObject gameOver, finish, standby, mobileControls, hud, landing;
    public StompBar stompBar;
    public Timer timer;
    public OverlayManager overlayManager;

    public void StartScreen(LevelTimeData playerInfo)
    {
        landing.SetActive(true);
        gameOver.SetActive(false);
        finish.SetActive(false);
        standby.SetActive(false);
        hud.SetActive(false);
        ActivateControls(false);
        landing.GetComponent<LandingScreenLoader>().GenerateLanding(playerInfo);
    }

    public void StandbyScreen()
    {
        landing.SetActive(false);
        standby.SetActive(true);
        hud.SetActive(true);
        ActivateControls(true);
        overlayManager.SetRunState(RunState.Standby);
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

    public float FinishScreen(LevelTimeData playerTime)
    {
        float finishTime = timer.StopTimer();
        finish.GetComponent<FinishScreenLoader>().GenerateFinishScreen(playerTime, finishTime);
        finish.SetActive(true);
        hud.SetActive(false);
        ActivateControls(false);
        return finishTime;
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
        overlayManager.RestartLevel();
    }

}


