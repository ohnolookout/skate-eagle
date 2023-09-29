using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MainMenu : MonoBehaviour
{
    public Level currentLevel;
    public List<Level> levels;
    private LevelDataManager levelManager;

    private void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelDataManager>();
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
