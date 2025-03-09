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
    [SerializeField] private CameraOperator _cameraOperator;
    private bool _finishIsActive = false;
    private ProCamera2D _camera;
    [SerializeField] private GameObject _playerPrefab;
    private GameManager _gameManager;
    private static Player _player;
    public static Action<Level, PlayerRecord> OnLanding { get; set; }
    public static Action<ILevelManager> OnGameOver { get; set; }
    public static Action<FinishData> OnFinish { get; set; }
    public static Action OnAttempt { get; set; }
    public static Action OnStandby { get; set; }
    public static Action OnResultsScreen { get; set; }
    public static Action OnRestart { get; set; }
    public static Action OnLevelExit { get; set; }
    public static Action OnFall { get; set; }
    public static Action OnCrossFinish { get; set; }


    public static IPlayer GetPlayer { get => _player; }
    public GroundManager GroundManager { get => _groundManager; set => _groundManager = value; }
    public bool HasPlayer { get => _player != null; }
    public bool HasTerrainManager { get => _groundManager != null; }
    #endregion

    #region Monobehaviours
    void Awake()
    {
        _gameManager = GameManager.Instance;
        _camera = ProCamera2D.Instance;
        InstantiatePlayer();
        GroundSegment.OnActivateFinish += _ => _finishIsActive = true;
    }

    private void Start()
    {
        SubscribeToPlayerEvents();
        Timer.OnStopTimer += OnStopTimer;

#if UNITY_EDITOR
        StartCoroutine(WaitForGameManagerInitializationRoutine());
        return;
#endif        
        InitializeLevel();
    }

    void Update()
    {
        if (_finishIsActive)
        {
            CheckFinish();
        }
    }

    private void CheckFinish()
    {
        Debug.Log("Checking finish...");
        if (_player.Transform.position.x >= _gameManager.CurrentLevel.FinishPoint.x && _player.CollisionManager.BothWheelsCollided)
        {
            Debug.Log("Finish crossed!");
            OnCrossFinish?.Invoke();
            _finishIsActive = false;
        }
    }
    private void InitializeLevel()
    {
        _finishIsActive = true;
        OnLanding?.Invoke(_gameManager.CurrentLevel, _gameManager.CurrentPlayerRecord);
        SerializeLevelUtility.DeserializeLevel(_gameManager.CurrentLevel, _groundManager);
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
    private void SubscribeToPlayerEvents()
    {
        if (!HasPlayer)
        {
            return;
        }
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.StartAttempt, StartAttempt);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Finish, ActivateResultsScreen);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Die, GameOver);
    }

    private void SetPlayerPosition(Vector2 position)
    {
        if (HasPlayer)
        {
            Debug.Log("Setting palayer position to " + position);
            float halfPlayerHeight = 4.25f;
            var startPosition = new Vector2(position.x, position.y + halfPlayerHeight + 1.2f);
            _player.Transform.position = startPosition;
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
    }

    private void InstantiatePlayer()
    {

        _player = Instantiate(_playerPrefab).GetComponent<Player>();
        SetPlayerPosition(_gameManager.CurrentLevel.StartPoint);
        _camera.AddCameraTarget(_player.Transform, 1, 1, 0, new(-0.13f, 0));
        _camera.Reset();
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
        Debug.Log("Starting attempt...");
        _inputEvents.OnRestart += RestartLevel;
        OnAttempt?.Invoke();
    }
    public void RestartLevel()
    {
        Debug.Log("Restarting...");
        _inputEvents.OnRestart -= RestartLevel;

        if (_player != null)
        {
            _camera.RemoveAllCameraTargets();
            Destroy(_player.gameObject);
        }

        OnRestart?.Invoke();
        InstantiatePlayer();
        Start();
    }

    public void Submit()
    {

    }

    public void GameOver(IPlayer _ = null)
    {
        OnGameOver?.Invoke(this);
    }

    public void ActivateResultsScreen(IPlayer _ = null)
    {
        OnResultsScreen?.Invoke();
    }

    public void CrossFinish(IPlayer _ = null)
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
