using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public LevelPanelGenerator levelPanel;
    public Level defaultLevel;
    private GameManager _gameManager;
    public GameObject levelScreen, titleScreen;
    public LevelMenu levelMenu;

    private void Start()
    {
        _gameManager = GameManager.Instance;
        if (_gameManager.LevelLoader.GoToLevelMenu)
        {
            LevelScreen();
            _gameManager.LevelLoader.GoToLevelMenu = false;
        }
    }
    public void ResetSaveData()
    {
        _gameManager.ResetSaveData();
        levelMenu.Start();
    }

    public void LoadLevel(Level level)
    {
        _gameManager.LevelLoader.LoadLevel(level);
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
