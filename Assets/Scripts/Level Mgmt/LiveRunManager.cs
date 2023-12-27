using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public enum RunState { Landing, Standby, Active, Finished, GameOver, Fallen}
public class LiveRunManager : MonoBehaviour
{
    [HideInInspector] public RunState runState = RunState.Landing;
    private Vector3 finishPoint; 
    [SerializeField] private Level currentLevel;
    private GameManager gameManager;
    private AudioManager audioManager;
    [SerializeField] private EagleScript eagleScript;
    [SerializeField] private CameraScript cameraScript;
    [SerializeField] private GroundSpawner groundSpawner;
    [SerializeField] private Timer timer;
    public static Action<LiveRunManager> OnLanding, OnGameOver;
    public static Action<FinishScreenData> OnFinish;
    public static Action OnAttempt, OnStandby, OnResultsScreen, OnRestart;
    private Action finishStop;

    void Awake()
    {

        Debug.Log("Subscribed event counts:");
        if(OnLanding == null)
        {
            Debug.Log($"OnLanding : null");
        }
        gameManager = GameManager.Instance;
        CurrentLevel = gameManager.CurrentLevel;
        audioManager = AudioManager.Instance;
        audioManager.RunManager = this;
        audioManager.ClearLoops();
        if (gameManager.LevelIsLoaded)
        {
            CurrentLevel = gameManager.CurrentLevel;
        } else {
            gameManager.CurrentLevel = CurrentLevel;
        }
    }

    private void OnEnable()
    {
        OnFinish += gameManager.UpdateRecord;
        OnRestart += audioManager.ClearLoops;
        finishStop += () => OnResultsScreen?.Invoke();
        eagleScript.FinishStop += finishStop;
    }
    private void OnDisable()
    {
        if (gameManager != null)
        {
            OnFinish -= gameManager.UpdateRecord;
        }
        if (audioManager != null)
        {
            OnRestart -= audioManager.ClearLoops;
        }
        eagleScript.FinishStop -= finishStop;
    }

    private void Start()
    {
        OnLanding?.Invoke(this);
    }


    public void BackToMenu()
    {
        SceneManager.LoadScene("Start_Menu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameOver()
    {
        OnGameOver?.Invoke(this);
        runState = RunState.GameOver;
    }

    public void StartAttempt()
    {
        OnAttempt?.Invoke();
        runState = RunState.Active;
    }

    public void GoToStandby()
    {
        OnStandby?.Invoke();
        runState = RunState.Standby;
    }
    public void SetLevel(Level level)
    {
        CurrentLevel = level;
    }
    public void Finish()
    {
        runState = RunState.Finished;
        float finishTime = timer.StopTimer();
        FinishScreenData finishData = FinishUtility.GenerateFinishData(gameManager.CurrentLevel, gameManager.CurrentPlayerRecord, finishTime);
        OnFinish?.Invoke(finishData);
    }
    public void Fall()
    {
        eagleScript.Fall();
        runState = RunState.GameOver;
    }

    public Vector3 FinishPoint { get => finishPoint; set => finishPoint = value; }
    public EagleScript Player { get => eagleScript; }
    public CameraScript CameraScript { get => cameraScript; }
    public GroundSpawner GroundSpawner { get => groundSpawner; }
    public Level CurrentLevel { get => currentLevel; set => currentLevel = value; }
}
