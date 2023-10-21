using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainMenu : MonoBehaviour
{
    public LevelPanelGenerator levelPanel;
    public Level defaultLevel;
    private GameManager gameManager;
    public GameObject levelScreen, titleScreen;

    void Awake()
    {
        gameManager = GameManager.Instance;
    }

    private void Start()
    {
        if (gameManager.goToLevelMenu)
        {
            LevelScreen();
            gameManager.goToLevelMenu = false;
        }
    }
    public void ResetSaveData()
    {
        gameManager.ResetSaveData();
    }

    public void LoadLevel(Level level)
    {
        gameManager.LoadLevel(level);
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void Quit()
    {
        Application.Quit();
        Debug.Log("There is no escape.");
    }

    public void LevelScreen()
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
