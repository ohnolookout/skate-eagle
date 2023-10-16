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

    void Awake()
    {
        levelManager = LevelDataManager.Instance;
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

}
