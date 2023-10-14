using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum RunState { Landing, Standby, Active, Finished, GameOver, Fallen}
public class LiveRunManager : MonoBehaviour
{
    public RunState runState = RunState.Landing;
    private EagleScript eagleScript;
    public bool startWithStomp = false, isMobile = true;
    private Vector3 startPoint, finishPoint;
    private float actualTerrainLength = 0, distanceToFinish = 0, distancePassed = 0f, stompThreshold = 2, stompCharge = 0;
    public GameObject levelManagerPrefab, bird;
    public Level currentLevel;
    private LevelDataManager levelManager;
    public Overlay overlay;

    void Awake()
    {
        
        if (GameObject.Find("LevelManager"))
        {
            levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelDataManager>();
            currentLevel = LevelDataManager.currentLevel;
        } 
        else {
            Debug.Log("LevelManager not found. Creating a new one...");
            GameObject levelManagerObject = Instantiate(levelManagerPrefab);
            levelManager = levelManagerObject.GetComponent<LevelDataManager>();
            LevelDataManager.currentLevel = currentLevel;
        }
        eagleScript = bird.GetComponent<EagleScript>();
    }

    private void Start()
    {
        overlay.StartScreen(levelManager.CurrentLevelRecords);
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
        if(runState == RunState.Active)
        {
            levelManager.AddAttempt();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        runState = RunState.Landing;
    }

    public void GameOver()
    {
        levelManager.AddAttempt();
        overlay.GameOverScreen();
        runState = RunState.GameOver;
    }

    public void StartAttempt()
    {
        overlay.StartAttempt();
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
            overlay.FillStompBar(stompCharge / stompThreshold);
        }
    }
    
    public void Finish()
    {
        runState = RunState.Finished;
        levelManager.AddAttempt();
        float finishTime = overlay.StopTimer();
        FinishScreenData finishData = FinishUtility.GenerateFinishData(LevelDataManager.currentLevel, levelManager.CurrentLevelRecords, finishTime);
        overlay.GenerateFinishScreen(finishData);
        levelManager.UpdateSessionData(finishData);
        StartCoroutine(SlowToFinish());
    }


    public IEnumerator SlowToFinish()
    {
        eagleScript.SlowToStop();
        while (eagleScript.Velocity.x > 0.2f)
        {
            yield return new WaitForFixedUpdate();
        }
        overlay.ActivateFinishScreen();
    }

    public void Fall()
    {
        eagleScript.Fall();
        runState = RunState.GameOver;
    }

    public LevelRecords CurrentLevelRecords
    {
        get
        {
            return levelManager.CurrentLevelRecords;
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

    public Level CurrentLevel
    {
        get
        {
            return LevelDataManager.currentLevel;
        }
    }


}
