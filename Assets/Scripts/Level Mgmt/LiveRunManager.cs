using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Events;
using System;

public enum RunState { Landing, Standby, Active, Finished, GameOver, Fallen, GameOverAfterFinished}
public class LiveRunManager : MonoBehaviour
{
    public static RunState runState = RunState.Landing;
    public bool startWithStomp = false, isMobile = true, pendingFinish = false;
    private Vector3 startPoint, finishPoint;
    private Timer timer;
    private IEnumerator finishCoroutine;
    [SerializeField] private Level currentLevel;
    private GameManager gameManager;
    [SerializeField] private EagleScript eagleScript;
    [SerializeField] private CameraScript cameraScript;
    [SerializeField] private GroundSpawner groundSpawner;
    public event Action<LiveRunManager> EnterLanding, EnterStandby, EnterAttempt, EnterGameOver, EnterFinish, EnterFinishScreen, RestartLevel;    
    private FinishScreenData? finishData;

    void Awake()
    {
        gameManager = GameManager.Instance;
        currentLevel = gameManager.CurrentLevel;
        AudioManager.Instance.AddRunManager(this);
        EnterFinish += _ => WaitForStop();
        EnterFinish += gameManager.UpdateRecord;
        timer = GameObject.FindGameObjectWithTag("Timer").GetComponent<Timer>();
        if (currentLevel == null) {
            gameManager.CurrentLevel = currentLevel;
        }
    }

    private void Start()
    {
        EnterLanding?.Invoke(this);
    }


    public void BackToMenu()
    {
        SceneManager.LoadScene("Start_Menu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        RestartLevel?.Invoke(this);
        runState = RunState.Landing;
    }

    public void GameOver()
    {
        EnterGameOver?.Invoke(this);
        if (!pendingFinish)
        {
            runState = RunState.GameOver;
        }
        else {
            runState = RunState.GameOverAfterFinished;
        }
    }

    public void StartAttempt()
    {
        EnterAttempt?.Invoke(this);
        runState = RunState.Active;
        startPoint = PlayerPosition;
    }

    public void GoToStandby()
    {
        EnterStandby?.Invoke(this);
    }
    public void SetLevel(Level level)
    {
        currentLevel = level;
    }

    public void Finish()
    {
        float finishTime = timer.StopTimer();
        finishData = FinishUtility.GenerateFinishData(gameManager.CurrentLevel, gameManager.CurrentPlayerRecord, finishTime);
        EnterFinish?.Invoke(this);
        runState = RunState.Finished;
    }

    private void WaitForStop() 
    {
        finishCoroutine = SlowToFinish();
        StartCoroutine(finishCoroutine);
    }

    private IEnumerator SlowToFinish()
    {
        pendingFinish = true;
        while (eagleScript.Velocity.x > 0.2f && runState != RunState.GameOverAfterFinished)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.75f);
        EnterFinishScreen?.Invoke(this);
        pendingFinish = false;
    }

    public void Fall()
    {
        eagleScript.Fall();
        runState = RunState.GameOver;
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

    public PlayerRecord CurrentPlayerRecord
    {
        get
        {
            return gameManager.CurrentPlayerRecord;
        }
    }

    public FinishScreenData? FinishData
    {
        get
        {
            if (finishData != null)
            {
                return finishData;
            }
            return null;
        }
        set
        {
            finishData = value;
        }
    }

}
