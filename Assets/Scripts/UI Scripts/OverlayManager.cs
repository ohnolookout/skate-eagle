using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

public class OverlayManager : MonoBehaviour
{
    private GameObject hud, gameOverScreen, startText, stompBar, finishScreen, progressBar;
    public GameObject mobileUI, mobileControls, desktopUI;
    private Overlay overlay;
    private TMP_Text timerText;
    private Level _level;
    private char[] timerChars = new char[8];
    private LiveRunManager logic;
    public Sprite[] medalSprites;
    public LevelTimeData _currentLevelPlayerData;
    // Start is called before the first frame update
    void Awake()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
    }

    public void AddUI(Level level, bool isMobile, LevelTimeData playerTime = null)
    {
        if(playerTime == null)
        {
            playerTime = new(level);
        }
        GameObject uiObject;
        if (isMobile)
        {
            Debug.Log("Adding mobile UI");
            uiObject = Instantiate(mobileUI);
        }
        else
        {
            Debug.Log("Adding desktop UI");
            uiObject = Instantiate(desktopUI);
        }
        overlay = uiObject.GetComponent<Overlay>();
        overlay.SetLevelData(playerTime);
        _level = level;
        _currentLevelPlayerData = playerTime;
        overlay.StartScreen(_level, _currentLevelPlayerData);
    }

    public void GameOver()
    {
        overlay.GameOverScreen();
    }

    public void Finish(LevelTimeData playerTime)
    {
        overlay.FinishScreen(playerTime);
    }

    public void Finish()
    {
        overlay.FinishScreen(_currentLevelPlayerData);
    }

    public void StartScreen(Level level, LevelTimeData playerTime)
    {
        overlay.StartScreen(level, playerTime);
    }

    public void StartScreen()
    {
        overlay.StartScreen(_level, _currentLevelPlayerData);
    }

    public void StartAttempt()
    {
        overlay.StartAttempt();
    }

    public void BackToMenu()
    {
        logic.BackToMenu();
    }

    public void FillStompBar(float fillAmount)
    {
        overlay.FillStompBar(fillAmount);
    }
}
