using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro.EditorUtilities;


public class Overlay : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverObject, _standbyObject;
    [SerializeField] private MobileControls _mobileControls;
    [SerializeField] private LandingScreen _landingScreen;
    [SerializeField] private FinishScreen _finishScreen;
    [SerializeField] private StompBar _stompBar;
    [SerializeField] private Timer _timer;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _gameplayRestartButton;
    [SerializeField] private Button _gameOverRestartButton;
    [SerializeField] private Button _continueButton;
    private ILevelManager _levelManager;
    public Timer Timer => _timer;

    private void Awake()
    {
        _levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ILevelManager>();

        LevelManager.OnGameOver += ActivateGameOverScreen;
        LevelManager.OnAttempt += StartAttempt;
        LevelManager.OnResultsScreen += ActivateResultsScreen;
        LevelManager.OnStandby += ActivateStandbyScreen;
        LevelManager.OnFinish += LoadResultsScreen;
        LevelManager.OnRestart += ActivateStartScreen;
        LevelManager.OnLanding += ActivateLandingScreen;

        _playButton.onClick.AddListener(_levelManager.GoToStandby);
        _nextLevelButton.onClick.AddListener(GameManager.Instance.LoadNextLevel);
        _menuButton.onClick.AddListener(GameManager.Instance.BackToLevelMenu);
        _gameOverRestartButton.onClick.AddListener(_levelManager.RestartLevel);
        _continueButton.onClick.AddListener(_levelManager.RestartLevel);
    }

    public void ActivateStartScreen()
    {
        _gameplayRestartButton.onClick.RemoveListener(_levelManager.RestartLevel);
        _landingScreen.ActivateDisplay(true);
        _finishScreen.ActivateDisplay(false);
        _gameOverObject.SetActive(false);
        _standbyObject.SetActive(false);
        _timer.ActivateDisplay(false);
        _mobileControls.ActivateDisplay(false);
    }

    public void ActivateStandbyScreen()
    {
        _landingScreen.ActivateDisplay(false);
        _standbyObject.SetActive(true);
        _timer.ActivateDisplay(true);
        _timer.ResetTimer();
        _mobileControls.ActivateDisplay(true);
    }

    public void StartAttempt()
    {
        _standbyObject.SetActive(false);
        _gameplayRestartButton.onClick.AddListener(_levelManager.RestartLevel);
    }

    public void ActivateGameOverScreen(ILevelManager _)
    {
        _gameOverObject.SetActive(true);
        _mobileControls.ActivateDisplay(false);
    }

    public void LoadResultsScreen(FinishData finishData)
    {
        _mobileControls.ActivateDisplay(false);
        _finishScreen.GenerateFinishScreen(finishData);
    }
    public void ActivateResultsScreen()
    {
        _timer.ActivateDisplay(false);
        _finishScreen.ActivateDisplay(true);
    }

    public void ActivateLandingScreen(Level level, PlayerRecord playerRecord)
    {
        _landingScreen.GenerateLanding(level, playerRecord);
        _landingScreen.ActivateDisplay(true);
    }
    
}


