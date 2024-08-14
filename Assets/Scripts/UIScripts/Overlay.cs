using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;


public class Overlay : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverObject, _finishObject, _standbyObject, _mobileControlsObject, _hudObject, _landingObject;
    [SerializeField] private LandingScreenLoader _landingLoader;
    [SerializeField] private FinishScreenLoader _finishLoader;
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

        _playButton.onClick.AddListener(_levelManager.GoToStandby);
        _nextLevelButton.onClick.AddListener(GameManager.Instance.LoadNextLevel);
        _menuButton.onClick.AddListener(GameManager.Instance.BackToLevelMenu);
        _restartButton.onClick.AddListener(_levelManager.RestartGame);
        _continueButton.onClick.AddListener(_levelManager.RestartGame);
    }

    public void ActivateStartScreen()
    {
        _landingObject.SetActive(true);
        _gameOverObject.SetActive(false);
        _standbyObject.SetActive(false);
        _hudObject.SetActive(false);
        ActivateControls(false);
    }

    public void ActivateStandbyScreen()
    {
        _landingObject.SetActive(false);
        _standbyObject.SetActive(true);
        _hudObject.SetActive(true);
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
        _finishLoader.GenerateFinishScreen(finishData);
    }
    public void ActivateResultsScreen()
    {
        _hudObject.SetActive(false);
        ActivateControls(false);
        _finishLoader.ActivateDisplay();
    }

    public void ActivateControls(bool activate)
    {
        if(_mobileControlsObject is null)
        {
            return;
        }
        _mobileControlsObject.SetActive(activate);
    }
    
}


