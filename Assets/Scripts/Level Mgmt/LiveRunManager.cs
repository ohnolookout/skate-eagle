using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public enum RunState { Landing, Standby, Active, Finished, GameOver, Fallen}
public class LiveRunManager : MonoBehaviour, ILevelManager
{
    [HideInInspector] private RunState _runState = RunState.Landing;
    private Vector3 finishPoint; 
    [SerializeField] private Level currentLevel;
    private GameManager gameManager;
    private AudioManager audioManager;
    private TerrainManager _terrainManager;
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
        gameManager = GameManager.Instance;
        CurrentLevel = gameManager.CurrentLevel;
        audioManager = AudioManager.Instance;
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
        IPlayer.FinishStop += finishStop;
    }
    private void OnDisable()
    {
        ResetStaticEvents();
        if (gameManager != null)
        {
            OnFinish -= gameManager.UpdateRecord;
        }
        if (audioManager != null)
        {
            OnRestart -= audioManager.ClearLoops;
        }
        IPlayer.FinishStop -= finishStop;
    }

    private void Start()
    {
        OnLanding?.Invoke(this);
    }

    private void ResetStaticEvents()
    {
        OnLanding = null;
        OnAttempt = null;
        OnFinish = null;
        OnGameOver = null;
        OnResultsScreen = null;
        OnStandby = null;
        OnRestart = null;    
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
        _runState = RunState.GameOver;
    }

    public void StartAttempt()
    {
        OnAttempt?.Invoke();
        _runState = RunState.Active;
    }

    public void GoToStandby()
    {
        OnStandby?.Invoke();
        _runState = RunState.Standby;
    }
    public void SetLevel(Level level)
    {
        CurrentLevel = level;
    }
    public void Finish()
    {
        _runState = RunState.Finished;
        float finishTime = timer.StopTimer();
        FinishScreenData finishData = FinishUtility.GenerateFinishData(gameManager.CurrentLevel, gameManager.CurrentPlayerRecord, finishTime);
        OnFinish?.Invoke(finishData);
    }
    public void Fall()
    {
        eagleScript.Fall();
        _runState = RunState.GameOver;
    }

    public Vector3 FinishPoint { get => finishPoint; set => finishPoint = value; }
    public IPlayer GetPlayer { get => eagleScript; }
    public ICameraOperator CameraOperator{ get => cameraScript; }
    public GroundSpawner GroundSpawner { get => groundSpawner; }
    public Level CurrentLevel { get => currentLevel; set => currentLevel = value; }
    public TerrainManager TerrainManager { get => _terrainManager; set => _terrainManager = value; }
    public RunState RunState { get => _runState; set => _runState = value; }
    public bool HasCameraOperator { get => cameraScript != null; }
    public bool HasPlayer { get => eagleScript != null; }
    public bool HasTerrainManager { get => _terrainManager != null; }
}
