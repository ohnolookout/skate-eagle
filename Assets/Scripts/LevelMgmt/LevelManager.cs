using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class LevelManager : MonoBehaviour, ILevelManager
{
    #region Declarations
    public Vector3 startPosition = new();
    private Vector3 _finishPoint;
    [SerializeField] private Level _currentLevel;
    private GameManager _gameManager;
    private static IPlayer _player;
    [SerializeField] private TerrainManager _terrainManager;
    [SerializeField] private InputEventController _inputEvents;
    [SerializeField] private Timer timer;
    public static Action<ILevelManager> OnLanding, OnGameOver;
    public static Action<FinishData> OnFinish;
    public static Action OnAttempt, OnStandby, OnResultsScreen, OnRestart, OnLevelExit;
    public static Action OnFall { get; set; }
    public static Action<Vector2> OnActivateFinish { get; set; }
    private bool _overlayLoaded = false;
    [SerializeField] private CameraOperator _cameraOperator;


    public Vector3 FinishPoint { get => _finishPoint; set => _finishPoint = value; }
    public static IPlayer GetPlayer { get => _player; }
    public ICameraOperator CameraOperator { get => _cameraOperator; }
    public Level CurrentLevel { get => _currentLevel; set => _currentLevel = value; }
    public TerrainManager TerrainManager { get => _terrainManager; set => _terrainManager = value; }
    public bool HasCameraOperator { get => _cameraOperator != null; }
    public bool HasPlayer { get => _player != null; }
    public bool HasTerrainManager { get => _terrainManager != null; }
    #endregion

    #region Monobehaviours
    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<IPlayer>();
        AddSingletonManagers();
    }

    private void Start()
    {
        ActivateTerrainManager(startPosition);
        SetPlayerPosition(startPosition);
        SubscribeToPlayerEvents();
#if UNITY_EDITOR
        StartCoroutine(CheckGameManagerInitializationRoutine());
        return;
#endif        
        OnLanding?.Invoke(this);
        if (!_overlayLoaded)
        {
            Debug.LogWarning("No overlay found by level manager. Going to standby.");
            GoToStandby();
        }
        else
        {
            _inputEvents.OnRestart += GoToStandby;
        }
        
    }

    private void OnEnable()
    {
        _inputEvents = new(InputType.UI);
        _inputEvents.OnSubmit += Submit;
        Overlay.OnOverlayLoaded += () => _overlayLoaded = true;
        Overlay.OnStandbyButton += GoToStandby;
        Overlay.OnRestartButton += RestartGame;
    }
    private void OnDisable()
    {
        OnLevelExit?.Invoke();
        ResetStaticEvents();
    }

    private IEnumerator CheckGameManagerInitializationRoutine()
    {
        yield return new WaitWhile(() => GameManager.Instance.IsInitializing);
        OnLanding?.Invoke(this);
        if (!_overlayLoaded)
        {
            Debug.LogWarning("No overlay found by level manager. Going to standby.");
            GoToStandby();
        }
        else
        {
            _inputEvents.OnRestart += GoToStandby;
        }
    }

    #endregion

    #region Start/End Functions
    private void ActivateTerrainManager(Vector3 startPosition)
    {

#if UNITY_EDITOR
        if (_gameManager.CurrentLevel == null)
        {
            Debug.LogWarning("No level found. Skipping terrain creation.");
            return;
        }
        if (!HasTerrainManager)
        {
            Debug.LogWarning("No terrain manager found. Skipping terrain creation.");
            return;
        }
#endif

        _finishPoint = _terrainManager.GenerateTerrain(_gameManager.CurrentLevel, startPosition);
        _terrainManager.OnActivateFinish += ActivateFinish;
    }

    private void AddSingletonManagers()
    {
        _gameManager = GameManager.Instance;
        if (_gameManager.CurrentLevel != null)
        {
            CurrentLevel = _gameManager.CurrentLevel;
        }
        else
        {
            Debug.Log("No level loaded in game manager. Adding default level from level manager.");
            _gameManager.CurrentLevel = CurrentLevel;
        }
        OnFinish += _gameManager.UpdateRecord;
        
    }

    private void SubscribeToPlayerEvents()
    {
        if (!HasPlayer)
        {
            return;
        }
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Brake, Finish);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.StartAttempt, StartAttempt);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Finish, ActivateResultsScreen);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Die, GameOver);
    }

    private void SetPlayerPosition(Vector3 position)
    {
        if (HasPlayer)
        {
            float halfPlayerHeight = 4.25f;
            _player.Transform.position = new(position.x, position.y + halfPlayerHeight + 1.1f);
        }
    }
    public void SetLevel(Level level)
    {
        CurrentLevel = level;
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
        OnActivateFinish = null;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Start_Menu");
    }
    #endregion

    #region Event Invokers

    public void RestartGame()
    {
        Debug.Log("Restarting...");
        _inputEvents.OnRestart -= RestartGame;
        OnRestart?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Submit()
    {

    }

    public void GameOver(IPlayer _ = null)
    {
        OnGameOver?.Invoke(this);
    }

    public void StartAttempt(IPlayer _ = null)
    {
        _inputEvents.OnRestart += RestartGame;
        OnAttempt?.Invoke();
    }

    public void ActivateResultsScreen(IPlayer _ = null)
    {
        OnResultsScreen?.Invoke();
    }

    public void GoToStandby()
    {
        _inputEvents.OnRestart -= GoToStandby;
        OnStandby?.Invoke();
    }

    public void ActivateFinish(Vector2 finishPoint)
    {
        OnActivateFinish?.Invoke(finishPoint);
    }
    public void Finish(IPlayer _ = null)
    {
        FinishData finishData;
        if (timer != null)
        {
            float finishTime = timer.StopTimer();
            finishData = FinishUtility.GenerateFinishData(_gameManager.CurrentLevel, _gameManager.CurrentPlayerRecord, finishTime);
        }
        else
        {
            finishData = FinishUtility.GenerateFinishData(_gameManager.CurrentLevel, _gameManager.CurrentPlayerRecord, 1);
        }
        OnFinish?.Invoke(finishData);
    }
    public void Fall()
    {
        OnFall?.Invoke();
    }
    #endregion
}
