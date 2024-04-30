using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public LevelPanelGenerator levelPanel;
    public Level defaultLevel;
    private GameManager gameManager;
    public GameObject levelScreen, titleScreen;
    public LevelMenu levelMenu;

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager.goToLevelMenu)
        {
            LevelScreen();
            gameManager.goToLevelMenu = false;
        }
    }
    public void ResetSaveData()
    {
        gameManager.ResetSaveData();
        levelMenu.Start();
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
