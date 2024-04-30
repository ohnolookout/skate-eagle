using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using UnityEngine.SceneManagement;

public enum LoginStatus { LoggedIn, Guest, Offline };
public class GameManager : MonoBehaviour
{
    #region Declarations
    private SessionData _sessionData;
    public Level currentLevel;
    private static GameManager _instance;
    private SaveData _saveData;
    public bool goToLevelMenu = false;
    public bool clearPlayerPrefs = false;
    private bool _levelIsLoaded = false;
    private LoginStatus _loginStatus = LoginStatus.Offline;
    public LevelNode CurrentLevelNode => _sessionData.Node(currentLevel.UID);
    public Level CurrentLevel { get => currentLevel; set => currentLevel = value; }
    public bool LevelIsLoaded { get => _levelIsLoaded; set => _levelIsLoaded = value; }
    #endregion

    #region Monobehaviours
    private void Awake()
    {

        //Check to see if other instance exists that has already 
        if (_instance != null && _instance != this)
        {
            Debug.Log("Multiple game managers found. Self-harming...");
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(gameObject);
                return;
            }
#endif
            Destroy(gameObject);
            return;
        }
        _instance = this;
        if (SceneManager.GetActiveScene().name == "Level_Editor")
        {
            currentLevel = Resources.Load<Level>("EditorLevel");
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {            
            return;
        }
        if (clearPlayerPrefs)
        {
            PlayerPrefs.DeleteAll();
        }
#endif
        DontDestroyOnLoad(gameObject);
        _saveData = SaveSerial.LoadGame(_loginStatus);
        Debug.Log($"Loaded data file with {_saveData.PlayerRecords().Count} entries, first created on {_saveData.startDate}");
        int loadedRecordCount = _saveData.recordDict.Count;
        _sessionData = new(_saveData);
        //If new records were created during session setup, save game.
        if (loadedRecordCount < Session.RecordDict.Count)
        {
            SaveSerial.SaveGame(_saveData, _loginStatus);
        }
    }

    private void Start()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        DoLogin();

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;        

    }
    #endregion

    #region Level Loading
    public void LoadLevel(Level level)
    {
        currentLevel = level;
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

    public bool NextLevelUnlocked()
    {
        return _sessionData.NextLevelUnlocked(currentLevel);

    }
    #endregion

    #region Record Management
    public void ResetSaveData()
    {
        _saveData = SaveSerial.NewGame(_loginStatus);
        _sessionData = new(_saveData);
    }
    public void UpdateRecord(FinishScreenData finishData)
    {
        Session.UpdateRecord(finishData, CurrentLevel);
        SaveSerial.SaveGame(_saveData, _loginStatus);
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
            return _sessionData.Record(currentLevel);
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
    private async void DoLogin()
    {
        Debug.Log("Initializing login...");
        _loginStatus = await LoginUtility.GuestLogin();
    }

    #endregion
}
