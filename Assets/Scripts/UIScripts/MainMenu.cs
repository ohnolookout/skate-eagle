using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.UI;
using PlayFab;
using System.Text.RegularExpressions;

public class MainMenu : MonoBehaviour
{
    public Button MainPlayButton;
    public Button MainQuitButton;
    public Button MainNewGameButton;
    public Button MainPlayerSettingsButton;
    public Button LevelScreenBackButton;
    public Button LevelScreenPlayButton;
    public PopUpFactory _popUpFactory;
    public MenuPanel PopUpPanel;
    public MenuPanel SettingsPanel;
    public TMP_Text UsernameDisplay;
    public LevelMenu LevelMenu;
    public GameObject levelScreen, titleScreen;
    public GameObject loadingScreen;

    private GameManager _gameManager;

    void Awake()
    {
        _gameManager = GameManager.Instance; 
        _gameManager.PlayFabManager.OnInitializationComplete += HandleStartUp;
        _gameManager.OnMenuLoaded += OnMenuLoaded;
        _gameManager.PlayFabManager.OnUpdateStoredName += UpdateStoredName;
        _gameManager.OnAccountReset += OnAccountReset;
        _popUpFactory = new(PopUpPanel);
        
        //Main screen buttons
        MainPlayButton.onClick.AddListener(GoToLevelScreen);
        MainQuitButton.onClick.AddListener(Application.Quit);
        MainNewGameButton.onClick.AddListener( () => _popUpFactory.ShowNewGamePanel(PopUpPanel));
        MainPlayerSettingsButton.onClick.AddListener(() => _popUpFactory.ShowSettingsPanel(PopUpPanel));

    }
    
    private void OnDisable()
    {
        _gameManager.PlayFabManager.OnInitializationComplete -= HandleStartUp;
        _gameManager.OnMenuLoaded -= OnMenuLoaded;
        _gameManager.PlayFabManager.OnUpdateStoredName -= UpdateStoredName;
        _gameManager.OnAccountReset -= OnAccountReset;
    }

    
    //Show notification depending on whether it's a new player or not
    private void HandleStartUp(InitializationResult result)
    {
        _gameManager.PlayFabManager.OnInitializationComplete -= HandleStartUp;
        if (result.isFirstTime)
        {
            _popUpFactory.ShowFirstTimeSetNamePopUp(PopUpPanel, result.isFirstTime);
            return;
        }
        if (result.doResetAccount)
        {
            _popUpFactory.ShowResetAccountSuccessPopUp(PopUpPanel);
            return;
        }

        UsernameDisplay.text = PlayerPrefs.GetString(PlayFabManager.FormattedDisplayNameKey, "");

        if (_gameManager.InitializationResult.doAskEmail)
        {
            _popUpFactory.ShowFirstTimeAddEmailPopUp(PopUpPanel);
        }

    }

    private void OnAccountReset()
    {
        _gameManager.PlayFabManager.OnInitializationComplete += HandleStartUp;
    }

    private void UpdateStoredName(string _, string formattedDisplayName)
    {
        UsernameDisplay.text = formattedDisplayName;

    }

    void Start()
    {
        UsernameDisplay.text = PlayerPrefs.GetString(PlayFabManager.FormattedDisplayNameKey, "Settings");
    }

    #region Navigation
    private void OnMenuLoaded(bool goToLevelMenu)
    {
        if (goToLevelMenu)
        {
            GoToLevelScreen();
        }
    }

    public void LoadLevel(Level level)
    {
        _gameManager.LoadLevel(level);
    }

    public void GoToLevelScreen()
    {
        titleScreen.SetActive(false);
        levelScreen.SetActive(true);
    }

    public void StartScreen()
    {
        titleScreen.SetActive(true);
        levelScreen.SetActive(false);
    }
    #endregion
}
