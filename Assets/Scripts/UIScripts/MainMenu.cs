using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button PlayButton;
    public Button QuitButton;
    public Button NewGameButton;
    public Button LevelScreenBackButton;
    public Button LevelScreenPlayButton;
    public MenuPanel FirstTimeSetNamePanel;
    public MenuPanel FirstTimeAddEmailPanel;
    public MenuPanel SetNamePanel;
    public MenuPanel SetEmailPanel;
    public MenuPanel ConfirmNewGamePanel;
    public LevelMenu LevelMenu;
    private GameManager _gameManager;
    public GameObject levelScreen, titleScreen;

    void Awake()
    {
        _gameManager = GameManager.Instance;
        _gameManager.OnStartupComplete += OnStartupComplete;
        _gameManager.OnMenuLoaded += OnMenuLoaded;
    }

    void Start()
    {

        PlayButton.onClick.AddListener(GoToLevelScreen);
        QuitButton.onClick.AddListener(Application.Quit);
        NewGameButton.onClick.AddListener(_gameManager.ResetSaveData);

    }


    //Show notification depending on whether it's a new player or not
    private void OnStartupComplete()
    {

    }

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
    /*
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    */

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

}
