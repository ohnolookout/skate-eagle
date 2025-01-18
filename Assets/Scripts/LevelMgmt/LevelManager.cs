using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class LevelManager : MonoBehaviour, ILevelManager
{
    #region Declarations
    public Vector3 startPosition = new();
    [SerializeField] private Level _currentLevel;
    [SerializeField] private GroundManager _terrainManager;
    [SerializeField] private InputEventController _inputEvents;
    [SerializeField] private CameraOperator _cameraOperator;
    private GameManager _gameManager;
    private bool _doTriggerLoadLevel = true;
    private static IPlayer _player;
    public static Action<ILevelManager> OnLanding { get; set; }
    public static Action<ILevelManager> OnGameOver { get; set; }
    public static Action<FinishData> OnFinish { get; set; }
    public static Action OnAttempt { get; set; }
    public static Action OnStandby { get; set; }
    public static Action OnResultsScreen { get; set; }
    public static Action OnRestart { get; set; }
    public static Action OnLevelExit { get; set; }
    public static Action OnFall { get; set; }
    public static Action OnCrossFinish { get; set; }
    public static Action<Vector2> OnActivateFinishLine { get; set; }


    public static IPlayer GetPlayer { get => _player; }
    public Level CurrentLevel { get => _currentLevel; set => _currentLevel = value; }
    public GroundManager TerrainManager { get => _terrainManager; set => _terrainManager = value; }
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
        Timer.OnStopTimer += OnStopTimer;

#if UNITY_EDITOR
        StartCoroutine(CheckGameManagerInitializationRoutine());
        return;
#endif        
        OnLanding?.Invoke(this);
        _inputEvents.OnRestart += GoToStandby;
        
    }

    private void OnEnable()
    {
        _inputEvents = new(InputType.UI);
        _inputEvents.OnSubmit += Submit;
    }
    private void OnDisable()
    {
        OnLevelExit?.Invoke();
        ResetStaticEvents();
    }
#if UNITY_EDITOR
    private IEnumerator CheckGameManagerInitializationRoutine()
    {
        yield return new WaitWhile(() => GameManager.Instance.IsInitializing);
        OnLanding?.Invoke(this);
        if (_currentLevel.Name == "EditorLevel")
        {
            Debug.LogWarning("No overlay found by level manager. Going to standby.");
            GoToStandby();
        }
        else
        {
            _inputEvents.OnRestart += GoToStandby;
        }
        if (_doTriggerLoadLevel)
        {
            _gameManager.OnLevelLoaded?.Invoke(_currentLevel);
        }
    }
#endif
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

        _terrainManager.GenerateTerrain(_gameManager.CurrentLevel, startPosition);
        _terrainManager.OnActivateFinish += ActivateFinishLine;
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
        _gameManager.OnLevelLoaded += _ => _doTriggerLoadLevel = false;
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
        OnCrossFinish = null;
        OnFinish = null;
        OnGameOver = null;
        OnResultsScreen = null;
        OnStandby = null;
        OnRestart = null;
        OnActivateFinishLine = null;
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
        Debug.Log("Starting attempt...");
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

    public void ActivateFinishLine(Vector2 finishPoint)
    {
        OnActivateFinishLine?.Invoke(finishPoint);
    }
    public void Finish(IPlayer _ = null)
    {
        OnCrossFinish?.Invoke();        
    }

    private void OnStopTimer(float finishTime)
    {
        FinishData finishData;
        finishData = FinishUtility.GenerateFinishData(_gameManager.CurrentLevel, _gameManager.CurrentPlayerRecord, finishTime);
        OnFinish?.Invoke(finishData);
    }
    public void Fall()
    {
        OnFall?.Invoke();
    }
    #endregion
}
