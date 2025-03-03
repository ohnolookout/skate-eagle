using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.IO;
using UnityEditor;

public class LevelManager : MonoBehaviour, ILevelManager
{
    #region Declarations
    public Vector3 startPosition = new();
    [SerializeField] private GroundManager _groundManager;
    [SerializeField] private InputEventController _inputEvents;
    [SerializeField] private CameraOperator _cameraOperator;
    private GameManager _gameManager;
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
    public GroundManager GroundManager { get => _groundManager; set => _groundManager = value; }
    public bool HasPlayer { get => _player != null; }
    public bool HasTerrainManager { get => _groundManager != null; }
    #endregion

    #region Monobehaviours
    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<IPlayer>();
        _gameManager = GameManager.Instance;
        OnFinish += _gameManager.UpdateRecord;
        GroundSegment.OnActivateFinish += ActivateFinishLine;
    }

    private void Start()
    {
        SubscribeToPlayerEvents();
        Timer.OnStopTimer += OnStopTimer;

#if UNITY_EDITOR
        StartCoroutine(WaitForGameManagerInitializationRoutine());
        return;
#endif        
        OnLanding?.Invoke(this);
        _inputEvents.OnRestart += GoToStandby;
        SerializeLevelUtility.DeserializeLevel(_gameManager.CurrentLevel, _groundManager);
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
    private IEnumerator WaitForGameManagerInitializationRoutine()
    {
        Debug.Log("Waiting for game manager to initialize...");
        yield return new WaitWhile(() => GameManager.Instance.IsInitializing);
        Debug.Log("Game manager initialized. Starting level...");
        OnLanding?.Invoke(this);
        _inputEvents.OnRestart += GoToStandby;
        SerializeLevelUtility.DeserializeLevel(_gameManager.CurrentLevel, _groundManager);
    }
#endif
#endregion

    #region Start/End Functions
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

    private void SetPlayerPosition(GroundSegment _, Vector2 position)
    {
        if (HasPlayer)
        {
            Debug.Log("Setting palayer position to " + position);
            float halfPlayerHeight = 4.25f;
            var startPosition = new Vector2(position.x, position.y + halfPlayerHeight + 1.2f);
            _player.Transform.position = startPosition;
            _player.NormalBody.position = startPosition;
            _player.RagdollBody.position = startPosition;
            _player.RagdollBoard.position = startPosition;
        } 
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
        SetPlayerPosition(null, _groundManager.StartPoint);
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

    public void ActivateFinishLine(IGroundSegment segment)
    {
        OnActivateFinishLine?.Invoke(_groundManager.FinishPoint);
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
