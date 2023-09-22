using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEditor;

public class OverlayManager : MonoBehaviour
{
    public GameObject mobileUI, mobileControls, desktopUI;
    private Overlay overlay;
    private LevelTimeData playerTime;
    public LiveRunManager runManager;
    // Start is called before the first frame update

    public void AddUI(LevelTimeData playerData, bool isMobile = true)
    {
        GameObject uiObject;
        if (isMobile)
        {
            uiObject = Instantiate(mobileUI);
        }
        else
        {
            uiObject = Instantiate(desktopUI);
        }
        overlay = uiObject.GetComponent<Overlay>();
        overlay.overlayManager = this;
        overlay.StartScreen(playerData);
    }

    public void GameOver()
    {
        overlay.GameOverScreen();
    }

    public float Finish(LevelTimeData playerTime)
    {
        float time = overlay.FinishScreen(playerTime);
        return time;
    }

    public void StartScreen(LevelTimeData playerTime)
    {
        overlay.StartScreen(playerTime);
    }

    public void StartAttempt()
    {
        overlay.StartAttempt();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Start_Menu");
    }

    public void FillStompBar(float fillAmount)
    {
        overlay.FillStompBar(fillAmount);
    }

    public void SetRunState(RunState runState)
    {
        runManager.runState = runState;
    }

    public void RestartLevel()
    {
        runManager.RestartGame();
    }
}
