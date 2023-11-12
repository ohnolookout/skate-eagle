using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum RunState { Landing, Standby, Active, Finished, GameOver, Fallen}
public class LiveRunManager : MonoBehaviour
{
    public RunState runState = RunState.Landing;
    public bool startWithStomp = false, isMobile = true;
    private Vector3 startPoint, finishPoint;
    private float stompThreshold = 2, stompCharge = 0; //distanceToFinish = 0, distancePassed = 0f, 
    [SerializeField] private Level currentLevel;
    private GameManager gameManager;
    [SerializeField] private EagleScript eagleScript;
    [SerializeField] public Overlay overlay;
    [SerializeField] private CameraScript cameraScript;
    [SerializeField] private GroundSpawner groundSpawner;

    void Awake()
    {

        gameManager = GameManager.Instance;
        currentLevel = gameManager.CurrentLevel;
        /* UNCOMMENT IF LEVEL ISSUES
        if (currentLevel == null) {
            gameManager.CurrentLevel = currentLevel;
        }*/
    }

    private void Start()
    {
        overlay.StartScreen(gameManager.CurrentPlayerRecord);
    }

    private void Update()
    {
        /*
        if (runState != RunState.Active)
        {
            return;
        }
        
        if (Time.frameCount % 20 != 0)
        {
            distancePassed = (PlayerPosition.x - startPoint.x) / distanceToFinish;
        }*/
    }


    public void BackToMenu()
    {
        SceneManager.LoadScene("Start_Menu");
    }

    public void RestartGame()
    {
        if (runState == RunState.Active)
        {
            gameManager.AddAttempt();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        runState = RunState.Landing;
    }

    public void GameOver()
    {
        groundSpawner.SwitchToRagdoll();
        gameManager.AddAttempt();
        overlay.GameOverScreen();
        runState = RunState.GameOver;
    }

    public void StartAttempt()
    {
        overlay.StartAttempt();
        if (startWithStomp)
        {
            StompCharge = 2;
        }
        runState = RunState.Active;
        startPoint = PlayerPosition;
    }

    public void GoToStandby()
    {
        overlay.StandbyScreen();
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
        float finishTime = overlay.StopTimer();
        overlay.ActivateControls(false);
        FinishScreenData finishData = FinishUtility.GenerateFinishData(gameManager.CurrentLevel, gameManager.CurrentPlayerRecord, finishTime);
        overlay.GenerateFinishScreen(finishData);
        gameManager.UpdateRecord(finishData);
        StartCoroutine(SlowToFinish());
    }


    public IEnumerator SlowToFinish()
    {
        eagleScript.SlowToStop();
        while (eagleScript.Velocity.x > 0.2f)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.75f);
        overlay.ActivateFinishScreen();
    }

    public void Fall()
    {
        eagleScript.Fall();
        runState = RunState.GameOver;
    }

    /*
    public float DistancePassed
    {
        get
        {
            return distancePassed;
        }
    }*/

    public float StompThreshold
    {
        get
        {
            return stompThreshold;
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
            //distanceToFinish = finishPoint.x - startPoint.x;
        }
    }

    public Level CurrentLevel
    {
        get
        {
            return gameManager.CurrentLevel;
        }
    }

    public Vector2 PlayerPosition
    {
        get
        {
            return eagleScript.Transform.position;
        }
    }

    public bool PlayerDirectionForward
    {
        get
        {
            return eagleScript.DirectionForward;
        }
    }

    public EagleScript Player
    {
        get
        {
            return eagleScript;
        }
    }

    public Rigidbody2D PlayerBody
    {
        get
        {
            return eagleScript.Rigidbody;
        }
    }

    public CameraScript CameraScript
    {
        get
        {
            return cameraScript;
        }
    }

    public Vector3 LeadingCameraCorner
    {
        get
        {
            return CameraScript.LeadingCorner;
        }
    }
    public Vector3 TrailingCameraCorner
    {
        get
        {
            return CameraScript.TrailingCorner;
        }
    }

    public Vector3 CameraCenter
    {
        get
        {
            return CameraScript.Center;
        }
    }

    public Vector3 LowestGroundPoint
    {
        get
        {
            return groundSpawner.LowestPoint;
        }
    }

    public bool PlayerIsRagdoll
    {
        get
        {
            return eagleScript.IsRagdoll;
        }
    }

    public Rigidbody2D RagdollBoard
    {
        get
        {
            return eagleScript.RagdollBoard;
        }
    }

}
