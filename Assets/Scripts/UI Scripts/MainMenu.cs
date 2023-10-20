using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainMenu : MonoBehaviour
{
    public LevelPanelGenerator levelPanel;
    public Level defaultLevel;
    private LevelDataManager levelManager;
    public GameObject levelScreen, titleScreen;

    void Awake()
    {
        levelManager = LevelDataManager.Instance;
    }

    private void Start()
    {
        if (levelManager.goToLevelMenu)
        {
            LevelScreen();
            levelManager.goToLevelMenu = false;
        }
    }
    public void ResetSaveData()
    {
        levelManager.ResetSaveData();
    }

    public void LoadLevel(Level level)
    {
        levelManager.LoadLevel(level);
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
