using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public enum LoginStatus { LoggedIn, Guest, Offline };
public class GameManager : MonoBehaviour
{
    #region Declarations
    private SessionData _sessionData;
    private Level _currentLevel;
    private static GameManager _instance;
    private SaveData _saveData;
    private LeaderboardManager _leaderboardManager;
    public bool goToLevelMenu = false;
    public bool clearPlayerPrefs = false;
    private bool _levelIsLoaded = false;
    private LoginStatus _loginStatus = LoginStatus.Offline;
    public LevelNode CurrentLevelNode => _sessionData.Node(_currentLevel.levelUID);
    public Level CurrentLevel { get => _currentLevel; set => _currentLevel = value; }
    public bool LevelIsLoaded => _levelIsLoaded;
    public LeaderboardManager Leaderboard => _leaderboardManager;
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

        LoadGame();        
        _leaderboardManager = new();
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        DoLogin();
    }
    #endregion

    #region Level Loading
    public void LoadLevel(Level level)
    {
        _currentLevel = level;
        _levelIsLoaded = true;
        SceneManager.LoadScene("City");
        AudioManager.Instance.ClearLoops();
    }

    public void BackToLevelMenu()
    {
        goToLevelMenu = true;
        _levelIsLoaded = false;
        SceneManager.LoadScene("Start_Menu");
        AudioManager.Instance.ClearLoops();
    }

    public void NextLevel()
    {
        if (CurrentLevelNode.next == null)
        {
            return;
        }
        LoadLevel(CurrentLevelNode.next.level);
    }

    #endregion

    #region Record Management
    public async void ResetSaveData()
    {
        await RefreshLogin();
        _saveData = SaveSerial.NewGame(_loginStatus);
        _sessionData = new(_saveData);
    }
    public async void UpdateRecord(FinishScreenData finishData)
    {
        bool isNewBest = _sessionData.UpdateRecord(finishData, _currentLevel);
        bool uploadSuccessful = false;
        await RefreshLogin();
        SaveSerial.SaveGame(_saveData, _loginStatus);
        //Will need to change to not be guest later;
        if (isNewBest && _loginStatus != LoginStatus.Offline)
        {
            Debug.Log("Uploading new best time to leaderboard...");
            uploadSuccessful = await _leaderboardManager.UpdateRecord(_sessionData.Record(_currentLevel.levelUID));
        }
        if(isNewBest && !uploadSuccessful)
        {
            Debug.Log("Setting record to dirty because player is not logged in.");
            //***TO DO*** Switch dirty records to serialized dict to avoid duplicate records for single level
            _sessionData.DirtyRecords.Add(_sessionData.Record(_currentLevel.levelUID));
        }
    }
    
    public async void SubmitDirtyRecords()
    {
        foreach(PlayerRecord record in _sessionData.DirtyRecords)
        {
            bool uploadSuccessful = await _leaderboardManager.UpdateRecord(record);
            if (uploadSuccessful)
            {
                _sessionData.DirtyRecords.Remove(record);
            }
        }
    }

    public PlayerRecord CurrentPlayerRecord
    {
        get
        {
            //If sessionData hasn't been created, it means that GM is being run in editor mode, so Awake may need to be called manually.
            if(_sessionData == null)
            {
                Awake();
            }
            return _sessionData.Record(_currentLevel);
        }
    }
    #endregion

    #region Singleton Management
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

    public SessionData Session
    {
        get
        {
            if(_sessionData == null)
            {
                Awake();
            }
            return _sessionData;
        }
    }
    #endregion

    #region Login
    private async Task DoLogin()
    {
        Debug.Log("Initializing login...");
        _loginStatus = await LoginUtility.GuestLogin();
        //Will need to change to not be guest later;
        if (_loginStatus != LoginStatus.Offline)
        {
            SubmitDirtyRecords();
        }
        //Save serial to update removed dirty records
        SaveSerial.SaveGame(_saveData, _loginStatus);
    }

    private async Task RefreshLogin()
    {
        if(_loginStatus == LoginStatus.Offline)
        {
            await DoLogin();
        }
    }

    private void LoadGame()
    {
        _saveData = SaveSerial.LoadGame(_loginStatus);
        Debug.Log($"Loaded data file with {_saveData.recordDict.Count} entries, first created on {_saveData.startDate}");
        int loadedRecordCount = _saveData.recordDict.Count;
        _sessionData = new(_saveData);
    }

    #endregion
}
