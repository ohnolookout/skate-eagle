using UnityEngine;
using UnityEngine.SceneManagement;

public enum RunState { Landing, Standby, Active, Finished, GameOver, Fallen}
public class LiveRunManager : MonoBehaviour
{
    public RunState runState = RunState.Landing;
    private OverlayManager overlayManager;
    private GameObject bird;
    private bool terrainGenerationCompleted = false;
    public bool startWithStomp = false, isMobile = true;
    private Vector3 startPoint, finishPoint;
    private float actualTerrainLength = 0, distanceToFinish = 0, distancePassed = 0f, stompThreshold = 2, stompCharge = 0;
    public GameObject levelManagerPrefab;
    public Level currentLevel;
    private LevelDataManager levelManager;

    void Awake()
    {
        
        if (GameObject.Find("LevelManager"))
        {
            levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelDataManager>();
        } 
        else {
            GameObject levelManagerObject = Instantiate(levelManagerPrefab);
            levelManager = levelManagerObject.GetComponent<LevelDataManager>();
            levelManager.currentLevel = currentLevel;
        }
        bird = GameObject.FindGameObjectWithTag("Player");
        overlayManager = GameObject.FindGameObjectWithTag("UI").GetComponent<OverlayManager>();
        overlayManager.AddUI(levelManager.currentLevelTimeData, isMobile);
    }


    private void Update()
    {
        
        if (runState != RunState.Active)
        {
            return;
        }
        if (Time.frameCount % 20 != 0)
        {
            distancePassed = (bird.transform.position.x - startPoint.x)/distanceToFinish;
        }
    }


    public void BackToMenu()
    {
        SceneManager.LoadScene("Start_Menu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        runState = RunState.Landing;
    }

    public void GameOver()
    {
        overlayManager.GameOver();
        runState = RunState.GameOver;
    }

    public void StartAttempt()
    {
        overlayManager.StartAttempt();
        runState = RunState.Active;
        startPoint = bird.transform.position;
    }
    public void SetLevel(Level level)
    {
        currentLevel = level;
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
        runState = RunState.Finished;
        float time = overlayManager.Finish(levelManager.currentLevelTimeData);
        levelManager.UpdateTime(time);
    }

    public void Fall()
    {
        bird.GetComponent<EagleScript>().Fall();
        runState = RunState.GameOver;
    }

    public LevelTimeData PlayerDataCurrentLevel
    {
        get
        {
            return levelManager.PlayerDataForCurrentLevel();
        }
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
            return currentLevel;
        }
    }

}
