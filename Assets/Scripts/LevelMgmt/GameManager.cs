using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    #region Declarations
    private static GameManager _instance;
    private SessionData _sessionData;
    private Level _currentLevel = null;
    private LevelDatabase _levelDB;
    [SerializeField] private PlayFabManager _playFabManager;
    private bool _isAwaitingPlayFab = false;
    private SaveLoadUtility _saveLoadUtility = SaveLoadUtility.Instance;
    private Action<Level> _onLevelLoaded;
    private Action<bool> _onMenuLoaded;
    private Action<bool> _onLoading;
    private Action _onAccountReset;
    [SerializeField] private GameObject _loadingScreen;
    public Slider loadingBar;

    public bool clearPlayerPrefs = false;
    private bool _isInitializing = false;
    public bool IsInitializing => _isInitializing;
    public SessionData SessionData { get => _sessionData; set => _sessionData = value; }
    public Level CurrentLevel { get => _currentLevel; set => _currentLevel = value; }
    public PlayerRecord CurrentPlayerRecord => _sessionData.GetRecordByLevel(_currentLevel);
    public PlayFabManager PlayFabManager => _playFabManager;
    public InitializationResult InitializationResult { get => _playFabManager.InitializationResult; set => _playFabManager.InitializationResult = value; }
    public Action<Level> OnLevelLoaded { get => _onLevelLoaded; set => _onLevelLoaded = value; }
    public Action<bool> OnMenuLoaded { get => _onMenuLoaded; set => _onMenuLoaded = value; } //bool true if load level menue;
    public Action<bool> OnLoading { get => _onLoading; set => _onLoading = value; }
    public Action OnAccountReset { get => _onAccountReset; set => _onAccountReset = value; }
    public LevelDatabase LevelDB => _levelDB;
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
        _levelDB = (LevelDatabase)AssetDatabase.LoadAssetAtPath("Assets/LevelDatabase/LevelDB.asset", typeof(LevelDatabase));
        _currentLevel = _levelDB.GetLevelByUID(_levelDB.lastLevelLoadedUID);
        
        OnLoading += ActivateLoadingScreen;

#if UNITY_EDITOR
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
        _isInitializing = true;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        OnLoading?.Invoke(true);
        _isAwaitingPlayFab = true;

        _playFabManager.OnInitializationComplete += OnInitializationComplete;
        StartCoroutine(_playFabManager.Initialize(this, false));
    }

    public void OnInitializationComplete(InitializationResult result)
    {
        OnLoading?.Invoke(false);

        _isInitializing = false;
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

    public void OnResetAccount()
    {
        _sessionData = _saveLoadUtility.NewGame();
        _onAccountReset?.Invoke();
         StartCoroutine(_playFabManager.Initialize(this, true));
    }

    public void UpdateRecord(FinishData finishData)
    {
        StartCoroutine(UpdateRecordRoutine(finishData));
    }

    private IEnumerator UpdateRecordRoutine(FinishData finishData)
    {
        bool isNewBest = _sessionData.UpdateRecord(finishData, _currentLevel);

        if (isNewBest && !InitializationResult.isLoggedIn)
        {
            _isAwaitingPlayFab = true;
            StartCoroutine(_playFabManager.Initialize(this, false));

            yield return new WaitWhile(() => _isAwaitingPlayFab);
        }

        if (isNewBest && InitializationResult.isLoggedIn == true)
        {
            _isAwaitingPlayFab = true;
            _playFabManager.OnLeadboardUpdateComplete += OnLeaderboardUpdateComplete;
            _playFabManager.UpdateLeaderboardRecord(_sessionData.GetRecordByUID(finishData.levelUID));
        }

        _saveLoadUtility.SaveGame(_sessionData);
    }

    private void OnLeaderboardUpdateComplete(PlayerRecord record, bool isSuccess)
    {
        OnLoading?.Invoke(false);
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
        StartCoroutine(LoadSceneRoutine("City", () => OnLevelLoaded?.Invoke(level)));

    }

    private IEnumerator LoadSceneRoutine(string sceneId, UnityAction callback = null)
    {
        OnLoading?.Invoke(true);
        loadingBar.value = 0;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneId);

        float progressValue;

        while (!loadOperation.isDone)
        {
            progressValue = loadOperation.progress > 0.9f ? 1 : Mathf.Clamp01(loadingBar.value + Mathf.Clamp(loadOperation.progress - loadingBar.value, (1 - loadingBar.value)/100, (1 - loadingBar.value)/50));
            loadingBar.value = progressValue;
            yield return null;
        }
        
        OnLoading?.Invoke(false);
        callback();
    }

    public void LoadNextLevel()
    {
        var nextLevel = _levelDB.GetNextLevel(CurrentLevel);
        if (nextLevel == null)
        {
            return;
        }
        LoadLevel(nextLevel);
    }
    public void BackToLevelMenu()
    {
        _currentLevel = null;
        StartCoroutine(LoadSceneRoutine("Start_Menu", () => OnMenuLoaded?.Invoke(true)));
    }

    private void ActivateLoadingScreen(bool isOn)
    {
        _loadingScreen.gameObject.SetActive(isOn);
    }

    #endregion
}
