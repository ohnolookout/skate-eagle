using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.IO;
using UnityEditor;
using Com.LuisPedroFonseca.ProCamera2D;

public class LevelManager : MonoBehaviour, ILevelManager
{
    #region Declarations
    public Vector3 startPosition = new();
    [SerializeField] private GroundManager _groundManager;
    [SerializeField] private InputEventController _inputEvents;
    [SerializeField] private GameObject _playerPrefab;
    private GameManager _gameManager;
    private Player _player;
    private Transform _playerTransform;
    public static Action<Level, PlayerRecord, ICameraTargetable> OnLanding { get; set; }
    public static Action OnGameOver { get; set; }
    public static Action<FinishData> OnFinish { get; set; }
    public static Action OnAttempt { get; set; }
    public static Action OnStandby { get; set; }
    public static Action OnResultsScreen { get; set; }
    public static Action OnRestart { get; set; }
    public static Action OnLevelExit { get; set; }
    public static Action OnFall { get; set; }
    public static Action OnCrossFinish { get; set; }
    public static Action<IPlayer> OnPlayerCreated { get; set; }

    public GroundManager GroundManager { get => _groundManager; set => _groundManager = value; }
    public bool HasPlayer { get => _player != null; }
    public bool HasTerrainManager { get => _groundManager != null; }
    #endregion

    #region Monobehaviours
    void Awake()
    {
        _gameManager = GameManager.Instance;
        Timer.OnStopTimer += OnStopTimer;
    }

    private void Start()
    {
        InstantiatePlayer();

#if UNITY_EDITOR
        StartCoroutine(WaitForGameManagerInitializationRoutine());
        return;
#endif        
        InitializeLevel();
    }

    void OnDrawGizmosSelected()
    {
        if (_gameManager != null && _gameManager.CurrentLevel != null)
        {         
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_gameManager.CurrentLevel.StartPoint, 1);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_gameManager.CurrentLevel.FinishPoint, 1);
        }
    }

    private void InitializeLevel()
    {
        SerializeLevelUtility.DeserializeLevel(_gameManager.CurrentLevel, _groundManager);
        OnLanding?.Invoke(_gameManager.CurrentLevel, _gameManager.CurrentPlayerRecord, _groundManager.StartTarget);

        _groundManager.FinishLine.DoFinish += CrossFinish;
        _groundManager.Grounds[0].SegmentList[0].gameObject.SetActive(false);
        _groundManager.Grounds[0].SegmentList[0].gameObject.SetActive(true);
        
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
        Timer.OnStopTimer -= OnStopTimer;
    }

#if UNITY_EDITOR
    private IEnumerator WaitForGameManagerInitializationRoutine()
    {
        yield return new WaitWhile(() => GameManager.Instance.IsInitializing);
        InitializeLevel();
    }
#endif
#endregion

    #region Start/End Functions


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
        OnFall = null;
        OnPlayerCreated = null;
        GroundSegment.OnSegmentBecomeVisible = null;
        GroundSegment.OnSegmentBecomeInvisible = null;
    }

    private void InstantiatePlayer()
    {
        _player = Instantiate(_playerPrefab).GetComponent<Player>();
        _playerTransform = _player.Transform;
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.StartAttempt, StartAttempt);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Finish, ActivateResultsScreen);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Die, GameOver);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Fall, GameOver);
        OnPlayerCreated?.Invoke(_player);
    }

    #endregion

    #region Event Invokers
    public void GoToStandby()
    {
        _inputEvents.OnRestart -= GoToStandby;
        OnStandby?.Invoke();
    }

    public void StartAttempt(IPlayer _ = null)
    {
        _inputEvents.OnRestart += RestartLevel;
        OnAttempt?.Invoke();

    }
    public void RestartLevel()
    {
        _inputEvents.OnRestart -= RestartLevel;
        _groundManager.FinishLine.DoFinish -= CrossFinish;

        if (_player != null)
        {
            Destroy(_player.gameObject);
        }

        OnRestart?.Invoke();
        Start();
    }

    public void CrossFinish()
    {
        _groundManager.FinishLine.DoFinish -= CrossFinish;
        OnCrossFinish?.Invoke();
        _inputEvents.OnRestart -= RestartLevel;
    }

    public void Submit()
    {

    }

    public void GameOver(IPlayer _ = null)
    {
        OnGameOver?.Invoke();
    }

    public void ActivateResultsScreen(IPlayer _ = null)
    {
        OnResultsScreen?.Invoke();
        _inputEvents.OnRestart += RestartLevel;
    }

    private void OnStopTimer(float finishTime)
    {
        FinishData finishData;
        finishData = FinishUtility.GenerateFinishData(_gameManager.CurrentLevel, _gameManager.CurrentPlayerRecord, finishTime);
        OnFinish?.Invoke(finishData);
    }
    public void Fall()
    {
        Debug.Log("Falling");
        OnFall?.Invoke();
    }

    #endregion
}
