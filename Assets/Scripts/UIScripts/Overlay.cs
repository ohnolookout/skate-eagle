using UnityEngine;
using UnityEngine.SceneManagement;
using System;


public class Overlay : MonoBehaviour
{
    public GameObject gameOver, finish, standby, mobileControls, hud, landing;
    public LandingScreenLoader landingLoader;
    public FinishScreenLoader finishLoader;
    [SerializeField] private ILevelManager _levelManager;
    private Action<ILevelManager> onLanding, onGameOver;
    private Action<FinishData> onFinish;
    public StompBar stompBar;
    public Timer timer;
    public static Action OnOverlayLoaded, OnStandbyButton, OnRestartButton;

    private void Awake()
    {
        OnOverlayLoaded?.Invoke();
        _levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ILevelManager>();
    }

    private void OnEnable()
    {
        onLanding += _ => ActivateStartScreen();
        LevelManager.OnLanding += onLanding;
        onGameOver += _ => ActivateGameOverScreen();
        LevelManager.OnGameOver += onGameOver;
        LevelManager.OnAttempt += StartAttempt;
        LevelManager.OnStandby += ActivateStandbyScreen;
        onFinish += _ => ActivateControls(false);
        LevelManager.OnFinish += onFinish;
        LevelManager.OnResultsScreen += ActivateFinishScreen;
    }

    private void OnDisable()
    {
        OnOverlayLoaded = null;
        OnStandbyButton = null;
        OnRestartButton = null;
    }
    public void ActivateStartScreen()
    {
        landing.SetActive(true);
        gameOver.SetActive(false);
        standby.SetActive(false);
        hud.SetActive(false);
        ActivateControls(false);
    }
    public void StartScreen(PlayerRecord playerRecord)
    {
        landing.SetActive(true);
        gameOver.SetActive(false);
        standby.SetActive(false);
        hud.SetActive(false);
        ActivateControls(false);
        landingLoader.GenerateLanding(GameManager.Instance.CurrentLevel, playerRecord);
    }

    public void ActivateStandbyScreen()
    {
        landing.SetActive(false);
        standby.SetActive(true);
        hud.SetActive(true);
        ActivateControls(true);
    }

    public void StandbyScreen()
    {
        OnStandbyButton?.Invoke();
        _levelManager.GoToStandby();
    }


    public void StartAttempt()
    {
        standby.SetActive(false);
    }

    public void GameOverScreen()
    {
        gameOver.SetActive(true);
        ActivateControls(false);
    }
    public void ActivateGameOverScreen()
    {
        gameOver.SetActive(true);
        timer.StopTimer();
        ActivateControls(false);
    }
    public void ActivateFinishScreen()
    {
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
        OnRestartButton?.Invoke();
        _levelManager.RestartGame();
    }
    
    public void LoadNextLevel()
    {
        GameManager.Instance.LoadNextLevel();
    }
    
}


