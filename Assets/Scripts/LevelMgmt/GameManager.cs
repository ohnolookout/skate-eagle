using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public enum LoginStatus { LoggedIn, Guest, Offline };
public class GameManager : MonoBehaviour
{
    #region Declarations
    private SessionData _sessionData;
    private Level _currentLevel;
    private static GameManager _instance;
    private LeaderboardManager _leaderboardManager;
    private LevelLoader _levelLoader;
    public bool clearPlayerPrefs = false;
    private LoginStatus _loginStatus = LoginStatus.Offline;
    public LevelNode CurrentLevelNode => _sessionData.Node(_currentLevel.levelUID);
    public Level CurrentLevel { get => _currentLevel; set => _currentLevel = value; }
    public LeaderboardManager Leaderboard => _leaderboardManager;
    public PlayerRecord CurrentPlayerRecord => _sessionData.Record(_currentLevel);
    public SessionData Session => _sessionData;
    public LevelLoader LevelLoader => _levelLoader;
    public LoginStatus LoginStatus => _loginStatus;
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
            Debug.Log("Multiple game managers found. Self-harming...");
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
        }
#endif

        _sessionData = SaveLoadUtility.LoadGame(_loginStatus);        
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

    public async Task DoLogin()
    {
        _loginStatus = await LoginUtility.RefreshLogin(this);
    }

    public async Task ResetSaveData()
    {
        await DoLogin();
        _sessionData = SaveLoadUtility.NewGame(_loginStatus);
    }

    #region Record Management
    public async void UpdateRecord(FinishScreenData finishData)
    {
        bool isNewBest = _sessionData.UpdateRecord(finishData, _currentLevel);
        bool uploadSuccessful = false;
        await DoLogin();
        //Will need to change to not be guest later;
        if (isNewBest && _loginStatus != LoginStatus.Offline)
        {
            Debug.Log("Uploading new best time to leaderboard...");
            uploadSuccessful = await _leaderboardManager.UpdateRecord(_sessionData.Record(_currentLevel.levelUID));
        }
        if(isNewBest && !uploadSuccessful)
        {
            Debug.Log("Setting record to dirty because player is not logged in.");
            _sessionData.SaveData.dirtyRecords[_currentLevel.levelUID] = _sessionData.Record(_currentLevel.levelUID);
        }
        SaveLoadUtility.SaveGame(_sessionData, _loginStatus);
    }
    
    public async Task SubmitDirtyRecords()
    {
        var dirtyUIDs = _sessionData.SaveData.dirtyRecords.Keys.ToList();
        foreach(string levelUID in dirtyUIDs)
        {
            Debug.Log("Uploading dirty record for level " + _sessionData.NodeDict[levelUID].Level.Name);
            bool uploadSuccessful = await _leaderboardManager.UpdateRecord(_sessionData.SaveData.dirtyRecords[levelUID]);
            if (uploadSuccessful)
            {
                Debug.Log("Dirty record upload successful!");
                _sessionData.SaveData.dirtyRecords.Remove(levelUID);
            }
        }
    }

    #endregion
}
