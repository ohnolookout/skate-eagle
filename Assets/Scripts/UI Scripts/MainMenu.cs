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
    void Start()
    {
        SetLevelPanel(defaultLevel);
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

    public void SetLevelPanel(Level level)
    {
        LevelRecords records = levelManager.RecordFromLevel(level.Name);
        if(records == null)
        {
            levelPanel.Generate(level, LevelNodeStatus.Locked, records, 1);
            return;
        }
        if (Single.IsPositiveInfinity(records.bestTime))
        {
            levelPanel.Generate(level, LevelNodeStatus.Incomplete, records);
            return;
        }
        levelPanel.Generate(level, LevelNodeStatus.Completed, records);
    }
}
