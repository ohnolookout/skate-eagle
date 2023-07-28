using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEditor;


public class LogicScript : MonoBehaviour
{
    public TMP_Text timerText;
    private OverlayManager overlayManager;
    private GameObject bird;
    private bool finished = false, terrainGenerationCompleted = false, started = false, gameOver = false, fallen = false;
    public bool startWithStomp = false, mobile = false;
    private Vector3 startPoint, finishPoint;
    public float terrainLimit = 1000;
    private float actualTerrainLength = 0, distanceToFinish = 0, distancePassed = 0f, timer = 0, stompThreshold = 2, stompCharge = 0;
    public GameObject mobileControls, mobileUI, desktopUI;
    public Level currentLevelData;

    void Awake()
    {
        currentLevelData = (Level)AssetDatabase.LoadAssetAtPath("Assets/Session Data/CurrentLevel.Asset", typeof(Level));
        bird = GameObject.FindGameObjectWithTag("Player");
        if (Application.isMobilePlatform || mobile)
        {
            mobile = true;
            AddMobileUI();
        }
        else
        {
            AddDesktopUI();
        }
        overlayManager = GameObject.FindGameObjectWithTag("UI").GetComponent<OverlayManager>();
    }

    void Start()
    {
        overlayManager.StartScreen(currentLevelData);

    }

    private void Update()
    {
        
        if (!started || finished || gameOver)
        {
            return;
        }
        timer += Time.deltaTime;
        if (Time.frameCount % 20 != 0)
        {
            distancePassed = (bird.transform.position.x - startPoint.x)/distanceToFinish;
            overlayManager.UpdateTimer(timer);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameOver()
    {
        gameOver = true;
        overlayManager.GameOverScreen();
    }

    public void StartAttempt()
    {
        overlayManager.StartAttempt();
        started = true;
        startPoint = bird.transform.position;
    }

    public void AddMobileUI()
    {
        Instantiate(mobileUI);
        Instantiate(mobileControls);
    }

    public void AddDesktopUI()
    {
        Instantiate(desktopUI);
    }

    public void SetCurrentLevel(Level level)
    {
        currentLevelData = level.DeepCopy();
    }

    public float StompCharge
    {
        get
        {
            return stompCharge;
        }
        set
        {
            if (value <= stompThreshold)
            {
                stompCharge = value;
            }
            else
            {
                stompCharge = stompThreshold;
            }
            overlayManager.FillStompBar(stompCharge / stompThreshold);
        }
    }
    
    public void Finish()
    {
        finished = true;
        overlayManager.FinishScreen(timer);
    }

    public float DistancePassed
    {
        get
        {
            return distancePassed;
        }
    }

    public float StompThreshold
    {
        get
        {
            return stompThreshold;
        }
    }

    public bool Finished
    {
        get
        {
            return finished;
        }
    }

    public float ActualTerrainLength
    {
        get
        {
            return actualTerrainLength;
        }
        set
        {
            actualTerrainLength = value;
        }
    }
    public bool TerrainCompleted
    {
        get
        {
            return terrainGenerationCompleted;
        }
        set
        {
            terrainGenerationCompleted = value;
        }
    }

    public Vector3 FinishPoint
    {
        get
        {
            return finishPoint;
        }
        set
        {
            finishPoint = value;
            distanceToFinish = finishPoint.x - startPoint.x;
        }
    }

    public Level Level
    {
        get
        {
            return currentLevelData;
        }
    }

    public bool Started
    {
        get
        {
            return started;
        }
    }

    public bool Dead
    {
        get
        {
            return gameOver;
        }
    }

    public bool Fallen
    {
        get
        {
            return fallen;
        }
        set
        {
            fallen = value;
            bird.GetComponent<EagleScript>().Fallen = value;
        }
    }


}
