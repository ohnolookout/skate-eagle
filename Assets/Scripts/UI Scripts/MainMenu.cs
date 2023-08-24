using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MainMenu : MonoBehaviour
{
    public Level currentLevel;
    public List<Level> levels;

    void Awake()
    {
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("Level_0");
    }

    public void LoadLevel(Level level)
    {
        currentLevel = level;
        PlayGame();
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
