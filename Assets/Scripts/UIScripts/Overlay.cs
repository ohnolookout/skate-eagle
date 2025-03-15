using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro.EditorUtilities;


public class Overlay : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverObject, _standbyObject, _mobileControlsObject, _hudObject;
    [SerializeField] private LandingScreen _landingScreen;
    [SerializeField] private FinishScreen _finishScreen;
    [SerializeField] private StompBar _stompBar;
    [SerializeField] private Timer _timer;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _restartButton;
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
        _restartButton.onClick.AddListener(_levelManager.RestartLevel);
        _continueButton.onClick.AddListener(_levelManager.RestartLevel);
    }

    public void ActivateStartScreen()
    {
        _landingScreen.gameObject.SetActive(true);
        _finishScreen.gameObject.SetActive(false);
        _gameOverObject.SetActive(false);
        _standbyObject.SetActive(false);
        _hudObject.SetActive(false);
        ActivateControls(false);
    }

    public void ActivateStandbyScreen()
    {
        _landingScreen.gameObject.SetActive(false);
        _standbyObject.SetActive(true);
        _hudObject.SetActive(true);
        _timer.ResetTimer();
        ActivateControls(true);
    }

    public void StartAttempt()
    {
        _standbyObject.SetActive(false);
    }

    public void ActivateGameOverScreen(ILevelManager _)
    {
        _gameOverObject.SetActive(true);
        ActivateControls(false);
    }

    public void LoadResultsScreen(FinishData finishData)
    {
        ActivateControls(false);
        _finishScreen.gameObject.SetActive(true);
        _finishScreen.DeactivateDisplay();
        _finishScreen.GenerateFinishScreen(finishData);
    }
    public void ActivateResultsScreen()
    {
        _hudObject.SetActive(false);
        _finishScreen.ActivateDisplay();
    }

    public void ActivateControls(bool activate)
    {
        if(_mobileControlsObject is null)
        {
            return;
        }
        _mobileControlsObject.SetActive(activate);
    }

    public void ActivateLandingScreen(Level level, PlayerRecord playerRecord)
    {
        _landingScreen.GenerateLanding(level, playerRecord);
        _landingScreen.gameObject.SetActive(true);
    }
    
}


