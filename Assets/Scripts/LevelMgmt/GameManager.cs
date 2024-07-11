using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections;

public class GameManager : MonoBehaviour
{
    #region Declarations
    private static GameManager _instance;
    private SessionData _sessionData;
    private Level _currentLevel = null;
    private PlayFabManager _playFabManager;
    private InitializationResult _initializationResult;
    private bool _isAwaitingPlayFab = false;
    private SaveLoadUtility _saveLoadUtility = SaveLoadUtility.Instance;
    private Action _onFirstTimeUser;
    private Action _onStartupComplete;
    private Action<Level> _onLevelLoaded;
    private Action<bool> _onMenuLoaded;
    public GameObject loadingScreen;

    public const string SeenBeforeKey = "SeenBefore";
    public const string LastSeenKey = "LastSeen";
    public const string CreateAccountOnSuccessfulAuth = "CreateAccountOnSuccessfulAuth";
    public const string RegisteredEmailKey = "RegisteredEmail";

    public bool clearPlayerPrefs = false;
    public SessionData SessionData { get => _sessionData; set => _sessionData = value; }
    public LevelNode CurrentLevelNode => _sessionData.Node(_currentLevel.levelUID);
    public Level CurrentLevel { get => _currentLevel; set => _currentLevel = value; }
    public PlayerRecord CurrentPlayerRecord => _sessionData.GetRecordByLevel(_currentLevel);
    public InitializationResult InitializationResult => _initializationResult;
    public Action OnFirstTimeUser { get => _onFirstTimeUser; set => _onFirstTimeUser = value; }
    public Action OnStartupComplete { get => _onStartupComplete; set => _onStartupComplete = value; }
    public Action<Level> OnLevelLoaded { get => _onLevelLoaded; set => _onLevelLoaded = value; }
    public Action<bool> OnMenuLoaded { get => _onMenuLoaded; set => _onMenuLoaded = value; } //bool true if load level menue;
    public static GameManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            Debug.Log("No game manager found. Creating instance.");
            GameObject managerObject = new GameObject("GameManager");
            _instance = managerObject.AddComponent<GameManager>();
            return _instance;
        }
    }
    #endregion

    #region Monobehaviours
    private void Awake()
    {

        //Check to see if other instance exists that has already 
        if (_instance != null && _instance != this)
        {
            Debug.Log("Multiple game managers found. Deleting this instance...");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        _playFabManager = new();

#if UNITY_EDITOR
        if (SceneManager.GetActiveScene().name == "Level_Editor")
        {
            _currentLevel = Resources.Load<Level>("EditorLevel");
        }

        if (clearPlayerPrefs)
        {
            Debug.Log("Clearing player prefs...");
            PlayerPrefs.DeleteAll();
            ResetSaveData();

        }
#endif

    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        loadingScreen.SetActive(true);
        _isAwaitingPlayFab = true;

        _playFabManager.OnInitializationComplete += OnInitializationComplete;
        StartCoroutine(_playFabManager.Initialize(this));
    }

    private void OnInitializationComplete(InitializationResult result)
    {
        loadingScreen.SetActive(false);

        _initializationResult = result;
        if (_initializationResult.isFirstTime)
        {
            OnFirstTimeUser?.Invoke();
        }
        OnStartupComplete?.Invoke();

        _isAwaitingPlayFab = false;
    }
    #endregion

    #region Session Management

    public void LoadExternalGame(SaveData externalSave)
    {
        _sessionData = new(externalSave);
    }

    public void ResetSaveData()
    {
        _sessionData = _saveLoadUtility.NewGame();
    }

    public void UpdateRecord(FinishData finishData)
    {
        StartCoroutine(UpdateRecordRoutine(finishData));
    }

    private IEnumerator UpdateRecordRoutine(FinishData finishData)
    {
        bool isNewBest = _sessionData.UpdateRecord(finishData, _currentLevel);

        if (isNewBest && !_initializationResult.isLoggedIn)
        {
            loadingScreen.SetActive(true);
            _isAwaitingPlayFab = true;

            PlayFabManager initializer = new();
            initializer.OnInitializationComplete += OnInitializationComplete;
            StartCoroutine(initializer.Initialize(this));

            yield return new WaitWhile(() => _isAwaitingPlayFab);
        }

        if (isNewBest && _initializationResult.isLoggedIn == true)
        {
            loadingScreen.SetActive(true);
            _isAwaitingPlayFab = true;

            Debug.Log("Uploading new best time to leaderboard...");
            _playFabManager.OnLeadboardUpdateComplete += OnLeaderboardUpdateComplete;
            _playFabManager.UpdateLeaderboardRecord(_sessionData.GetRecordByUID(finishData.levelUID));
        }

        _saveLoadUtility.SaveGame(_sessionData);
    }

    private void OnLeaderboardUpdateComplete(PlayerRecord record, bool isSuccess)
    {
        _playFabManager.OnLeadboardUpdateComplete -= OnLeaderboardUpdateComplete;

        if (!isSuccess)
        {
            SetRecordDirty(record);
        }

        _isAwaitingPlayFab = false;
    }


    public void SetRecordDirty(PlayerRecord record)
    {
        _sessionData.SaveData.dirtyRecords[record.levelUID] = record;
    }

    public void LoadLevel(Level level)
    {
        _currentLevel = level;
        SceneManager.LoadScene("City");
        OnLevelLoaded?.Invoke(level);

    }

    public void LoadNextLevel()
    {
        if (CurrentLevelNode.next == null)
        {
            return;
        }
        LoadLevel(CurrentLevelNode.next.level);
    }
    public void BackToLevelMenu()
    {
        _currentLevel = null;
        SceneManager.LoadScene("Start_Menu");
        OnMenuLoaded?.Invoke(true);
    }

    #endregion
}
