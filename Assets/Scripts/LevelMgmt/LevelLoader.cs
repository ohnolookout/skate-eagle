using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader
{
    private GameManager _gameManager;
    private bool _levelIsLoaded = false;
    private bool _goToLevelMenu = false;
    public bool LevelIsLoaded => _levelIsLoaded;
    public bool GoToLevelMenu { get => _goToLevelMenu; set => _goToLevelMenu = value; }
    public LevelLoader(GameManager manager)
    {
        _gameManager = manager;
    }
    public void LoadLevel(Level level)
    {
        _gameManager.CurrentLevel = level;
        _levelIsLoaded = true;
        SceneManager.LoadScene("City");
        AudioManager.Instance.ClearLoops();
    }

    public void BackToLevelMenu()
    {
        _goToLevelMenu = true;
        _levelIsLoaded = false;
        SceneManager.LoadScene("Start_Menu");
        AudioManager.Instance.ClearLoops();
    }

    public void LoadNextLevel()
    {
        if (_gameManager.CurrentLevelNode.next == null)
        {
            return;
        }
        LoadLevel(_gameManager.CurrentLevelNode.next.level);
    }
}
