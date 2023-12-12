using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum RunState { Landing, Standby, Active, Finished, GameOver, Fallen, GameOverAfterFinished}
public class LiveRunManager : MonoBehaviour
{
    public RunState runState = RunState.Landing;
    public bool startWithStomp = false, isMobile = true, pendingFinish = false;
    private Vector3 startPoint, finishPoint;
    private IEnumerator finishCoroutine;
    private float stompThreshold = 2, stompCharge = 0; //distanceToFinish = 0, distancePassed = 0f, 
    [SerializeField] private Level currentLevel;
    private GameManager gameManager;
    private AudioManager audioManager;
    [SerializeField] private EagleScript eagleScript;
    [SerializeField] public Overlay overlay;
    [SerializeField] private CameraScript cameraScript;
    [SerializeField] private GroundSpawner groundSpawner;

    void Awake()
    {
        gameManager = GameManager.Instance;
        currentLevel = gameManager.CurrentLevel;
        audioManager = AudioManager.Instance;
        audioManager.RunManager = this;
        /* UNCOMMENT IF LEVEL ISSUES
        if (currentLevel == null) {
            gameManager.CurrentLevel = currentLevel;
        }*/
    }

    private void Start()
    {
        overlay.StartScreen(gameManager.CurrentPlayerRecord);
    }


    public void BackToMenu()
    {
        SceneManager.LoadScene("Start_Menu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        audioManager.ClearLoops();
        runState = RunState.Landing;
    }

    public void GameOver()
    {
        groundSpawner.SwitchToRagdoll();
        if (!pendingFinish)
        {
            overlay.GameOverScreen();
            runState = RunState.GameOver;
        }
        else { 
            runState = RunState.GameOverAfterFinished;
        }
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
        gameManager.UpdateRecord(finishData);
        overlay.GenerateFinishScreen(finishData);
        finishCoroutine = SlowToFinish();
        StartCoroutine(finishCoroutine);
    }

    private IEnumerator SlowToFinish()
    {
        pendingFinish = true;
        eagleScript.SlowToStop();
        while (eagleScript.Velocity.x > 0.2f && runState != RunState.GameOverAfterFinished) 
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.75f);
        overlay.ActivateFinishScreen();
        pendingFinish = false;
    }

    public void Fall()
    {
        eagleScript.Fall();
        runState = RunState.GameOver;
    }


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

    public float CameraSize
    {
        get
        {
            return cameraScript.Camera.orthographicSize;
        }
    }

    public Vector3 LeadingCameraCorner
    {
        get
        {
            return cameraScript.LeadingCorner;
        }
    }
    public Vector3 TrailingCameraCorner
    {
        get
        {
            return cameraScript.TrailingCorner;
        }
    }

    public Vector3 CameraCenter
    {
        get
        {
            return cameraScript.Center;
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

    public float CameraOffset
    {
        get
        {
            return CameraScript.leadingEdgeOffset;
        }
    }

    public float DefaultCameraSize
    {
        get
        {
            return cameraScript.DefaultSize;
        }
    }

}
