using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using LootLocker.Requests;
using System.Collections.Generic;
using System;

public enum LoginStatus { LoggedIn, Guest, Offline };
public class GameManager : MonoBehaviour
{
    #region Declarations
    private SessionData _sessionData;
    private LootLockerSessionResponse _lootLockerSession;
    private Level _currentLevel;
    private static GameManager _instance;
    private LeaderboardManager _leaderboardManager;
    private LevelLoader _levelLoader;
    public Action onFirstTimeUser;

    public bool clearPlayerPrefs = false;
    private LoginStatus _loginStatus = LoginStatus.Offline;
    public LevelNode CurrentLevelNode => _sessionData.Node(_currentLevel.levelUID);
    public Level CurrentLevel { get => _currentLevel; set => _currentLevel = value; }
    public LeaderboardManager Leaderboard => _leaderboardManager;
    public PlayerRecord CurrentPlayerRecord => _sessionData.Record(_currentLevel);
    public SessionData Session => _sessionData;
    public LevelLoader LevelLoader => _levelLoader;
    public LoginStatus LoginStatus { get => _loginStatus; set => _loginStatus = value; }
    public LootLockerSessionResponse LootLockerSession => _lootLockerSession;
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

#if UNITY_EDITOR
        if (SceneManager.GetActiveScene().name == "Level_Editor")
        {
            _currentLevel = Resources.Load<Level>("EditorLevel");
        }

        if (clearPlayerPrefs)
        {
            PlayerPrefs.DeleteAll();
            ResetSaveData();

        }
#endif

        _sessionData = SaveLoadUtility.LoadGame(this);        
        _leaderboardManager = new();
        _levelLoader = new(this);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        DoLogin();
    }
    #endregion

    #region Session Management
    private async Task CheckLogin()
    {
        if(_loginStatus == LoginStatus.Offline)
        {
            DoLogin();
        }
    }

    private async Task DoLogin()
    {
        _lootLockerSession = await LoginUtility.RefreshLogin(this);
        ParseSessionResponse(_lootLockerSession);
    }
    private void ParseSessionResponse(LootLockerSessionResponse response)
    {
        if (response == null || !response.success)
        {
            _loginStatus = LoginStatus.Offline;
        }
        else
        {
            _loginStatus = LoginStatus.Guest;
        }

        if (!response.seen_before)
        {
            onFirstTimeUser?.Invoke();
        }
    }

    public async Task ResetSaveData()
    {
        await CheckLogin();
        _sessionData = SaveLoadUtility.NewGame(this);
    }

    public async void UpdateRecord(FinishScreenData finishData)
    {
        await CheckLogin();
        SaveLoadUtility.UpdateRecord(this, finishData);
    }
    #endregion

    #region Player Management

    public async Task<PlayerNameResponse> SetPlayerName(string name)
    {
        return await SaveLoadUtility.SetPlayerNameTask(name);
    }

    #endregion
}
