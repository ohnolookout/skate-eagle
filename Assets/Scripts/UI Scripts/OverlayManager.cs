using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

public class OverlayManager : MonoBehaviour
{
    private GameObject hud, gameOverScreen, startText, stompBar, finishScreen, progressBar, mobileControls;
    private TMP_Text timerText;
    private Level _level;
    private char[] timerChars = new char[8];
    private LogicScript logic;
    public Sprite[] medalSprites;
    // Start is called before the first frame update
    void Awake()
    {
        AssignComponents();
        gameOverScreen.SetActive(false);
        finishScreen.SetActive(false);
        startText.SetActive(false);
        hud.SetActive(false);

    }

    void Start()
    {
        if (logic.mobile)
        {
            stompBar = GameObject.FindGameObjectWithTag("Mobile Stomp Bar");
            mobileControls = GameObject.FindGameObjectWithTag("Mobile Controls");
        }
        else
        {
            stompBar = hud.transform.Find("Stomp Bar").gameObject;
        }
        if (logic.startWithStomp)
        {
            logic.StompCharge = logic.StompThreshold;
        }

    }

    private void AssignComponents()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        hud = transform.Find("HUD").gameObject;
        gameOverScreen = transform.Find("Game Over Screen").gameObject;
        startText = transform.Find("Start Text").gameObject;
        finishScreen = transform.Find("Finish Screen").gameObject;
        progressBar = hud.transform.Find("Progress Bar").gameObject;
        timerText = hud.transform.Find("Timer Text").gameObject.GetComponent<TMP_Text>();

    }

    public void StartScreen(Level level)
    {
        _level = level;
        float[] times = _level.MedalTimes.TimesArray;
        startText.SetActive(true);
        TMP_Text[] medalTexts = startText.transform.GetChild(0).transform.GetComponentsInChildren<TMP_Text>();
        for (int i = 0; i < medalTexts.Length; i++)
        {
            medalTexts[i].text = FormatTime(times[i + 2]);
        }
        hud.SetActive(true);
    }

    public void FinishScreen(float finishTime)
    {
        hud.SetActive(false);
        if (logic.mobile)
        {
            TurnOffMobileControls();
        }
        finishScreen.GetComponent<FinishScreenScript>().GenerateFinishScreen(_level, finishTime, this);
        finishScreen.SetActive(true);
    }

    public void StartAttempt()
    {
        startText.SetActive(false);
    }

    public void GameOverScreen()
    {
        gameOverScreen.SetActive(true);
        if (logic.mobile)
        {
            TurnOffMobileControls();
        }
    }

    public void FillStompBar(float fillAmount)
    {
        stompBar.GetComponent<StompBar>().FillStompBar(fillAmount);
    }

    public void UpdateTimer(float time)
    {
        SecondsToCharArray(time, timerChars);
        timerText.SetCharArray(timerChars);
    }

    private static void SecondsToCharArray(float timeInSeconds, char[] array)
    {
        int minutes = (int)(timeInSeconds / 60f);
        array[0] = (char)(48 + (minutes % 10));
        array[1] = ':';

        int seconds = (int)(timeInSeconds - minutes * 60);
        array[2] = (char)(48 + seconds / 10);
        array[3] = (char)(48 + seconds % 10);
        array[4] = '.';

        int milliseconds = (int)((timeInSeconds % 1) * 1000);
        array[5] = (char)(48 + milliseconds / 100);
        array[6] = (char)(48 + (milliseconds % 100) / 10);
        array[7] = (char)(48 + milliseconds % 10);
    }

    public static string FormatTime(float time)
    {
        int milliseconds = (int)(time * 1000);
        int totalSeconds = milliseconds / 1000;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int remainingMilliseconds = milliseconds % 1000;

        return string.Format("{0}:{1:D2}.{2:D3}", minutes, seconds, remainingMilliseconds);
    }

    public static void UpdateTimeText(float timeInSeconds, TMP_Text tmpText)
    {
        char[] array = new char[8];
        int minutes = (int)(timeInSeconds / 60f);
        array[0] = (char)(48 + (minutes % 10));
        array[1] = ':';

        int seconds = (int)(timeInSeconds - minutes * 60);
        array[2] = (char)(48 + seconds / 10);
        array[3] = (char)(48 + seconds % 10);
        array[4] = '.';

        int milliseconds = (int)((timeInSeconds % 1) * 1000);
        array[5] = (char)(48 + milliseconds / 100);
        array[6] = (char)(48 + (milliseconds % 100) / 10);
        array[7] = (char)(48 + milliseconds % 10);

        tmpText.SetCharArray(array);
    }

    public void TurnOffMobileControls()
    {
        mobileControls.transform.GetChild(0).gameObject.SetActive(false);
        mobileControls.transform.GetChild(1).gameObject.SetActive(false);
    }

    public Sprite[] MedalSprites
    {
        get
        {
            return medalSprites;
        }
    }

    public Sprite RedSprite
    {
        get
        {
            return medalSprites[0];
        }
    }
    public Sprite BlueSprite
    {
        get
        {
            return medalSprites[1];
        }
    }
    public Sprite GoldSprite
    {
        get
        {
            return medalSprites[2];
        }
    }
    public Sprite SilverSprite
    {
        get
        {
            return medalSprites[3];
        }
    }

    public Sprite BronzeSprite
    {
        get
        {
            return medalSprites[4];
        }
    }
    public Sprite ParticipantSprite
    {
        get
        {
            return medalSprites[5];
        }
    }

}
