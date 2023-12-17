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

    void Awake()
    {
        runManager.EnterLanding += _ => LandingScreen();
        runManager.EnterLanding += landingLoader.GenerateLanding;
        runManager.EnterStandby += _ => StandbyScreen();
        runManager.EnterAttempt += _ => StartAttempt();
        runManager.EnterGameOver += _ => GameOverScreen();
        runManager.EnterFinish += finishLoader.GenerateFinishScreen;
        runManager.EnterFinishScreen += _ => ActivateFinishScreen();
    }

    void Start()
    {
        runManager.Player.EndFlip += (playerScript, _) => UpdateStompBar(playerScript);
        runManager.Player.StompEvent += (playerScript) => UpdateStompBar(playerScript);
    }

    public void LandingScreen()
    {
        landing.SetActive(true);
        gameOver.SetActive(false);
        finish.SetActive(false);
        standby.SetActive(false);
        hud.SetActive(false);
        ActivateControls(false);
    }
    public void LandingScreen(PlayerRecord playerRecord)
    {
        landing.SetActive(true);
        gameOver.SetActive(false);
        finish.SetActive(false);
        standby.SetActive(false);
        hud.SetActive(false);
        ActivateControls(false);
        landingLoader.GenerateLanding(runManager.CurrentLevel, playerRecord);
    }

    public void StandbyScreen()
    {
        landing.SetActive(false);
        standby.SetActive(true);
        hud.SetActive(true);
        ActivateControls(true);
        LiveRunManager.runState = RunState.Standby;
        if (runManager.Player.StompCharge > 0)
        {
            UpdateStompBar(runManager.Player);
        }
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

    public void ActivateFinishScreen()
    {
        finish.SetActive(true);
        hud.SetActive(false);
        ActivateControls(false);
    }
    public void UpdateStompBar(EagleScript playerScript)
    {
        float fillAmount = (float)playerScript.StompCharge / (float)playerScript.StompThreshold;
        if(fillAmount != stompBar.SliderValue)
        {
            stompBar.Fill(fillAmount);
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Start_Menu");
    }

    public void BackToLevelMenu()
    {
        GameManager.Instance.BackToLevelMenu();
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

    public void NextLevel()
    {
        GameManager.Instance.NextLevel();
    }

    public float StopTimer()
    {
        return timer.StopTimer();
    }


}


