using UnityEngine;
using UnityEngine.SceneManagement;
using System;


public class Overlay : MonoBehaviour
{
    public GameObject gameOver, finish, standby, mobileControls, hud, landing;
    public LandingScreenLoader landingLoader;
    public FinishScreenLoader finishLoader;
    [SerializeField] private LiveRunManager _runManager;
    private Action<LiveRunManager> onLanding, onGameOver;
    private Action<FinishScreenData> onFinish;
    public StompBar stompBar;
    public Timer timer;

    private void Awake()
    {
        _runManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
    }

    private void OnEnable()
    {
        onLanding += _ => ActivateStartScreen();
        LiveRunManager.OnLanding += onLanding;
        onGameOver += _ => ActivateGameOverScreen();
        LiveRunManager.OnGameOver += onGameOver;
        LiveRunManager.OnAttempt += StartAttempt;
        LiveRunManager.OnStandby += ActivateStandbyScreen;
        onFinish += _ => ActivateControls(false);
        LiveRunManager.OnFinish += onFinish;
        LiveRunManager.OnResultsScreen += ActivateFinishScreen;
    }

    private void OnDisable()
    {
        LiveRunManager.OnLanding -= onLanding;
        LiveRunManager.OnGameOver -= onGameOver;
        LiveRunManager.OnAttempt -= StartAttempt;
        LiveRunManager.OnStandby -= ActivateStandbyScreen;
        LiveRunManager.OnFinish -= onFinish;
        LiveRunManager.OnResultsScreen -= ActivateFinishScreen;
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
        _runManager.GoToStandby();
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
        _runManager.RestartGame();
    }

    public void NextLevel()
    {
        GameManager.Instance.NextLevel();
    }

}


