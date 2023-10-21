using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public SessionData sessionData;
    public Level currentLevel;
    public PlayerRecord currentPlayerRecord;
    private static GameManager instance;
    private SaveData saveData;
    public LevelList levelList;
    public LevelNode currentLevelNode = null;
    public LevelNode[] levelNodes;
    public bool goToLevelMenu = false;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        saveData = SaveSerial.LoadGame();
        Debug.Log($"Loaded data file that was first created on {saveData.startDate}");
        sessionData = new(saveData);
        levelList = Resources.Load<LevelList>("Level List");
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
    }

    public void CheckForOtherManagers()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    public void ResetSaveData()
    {
        saveData = SaveSerial.NewGame();
        sessionData = new(saveData);
    }

    public void LoadLevel(Level level)
    {
        currentLevel = level;
        SceneManager.LoadScene("City");
        if (!(sessionData.playerRecordsDict.ContainsKey(currentLevel.Name)))
        {
            sessionData.AddLevel(currentLevel);
        }
        currentPlayerRecord = sessionData.playerRecordsDict[currentLevel.Name];
    }


    public void BackToLevelMenu()
    {
        goToLevelMenu = true;
        SceneManager.LoadScene("Start_Menu");
    }

    public void NextLevel()
    {
        if (currentLevelNode.next == null)
        {
            return;
        }
        currentLevelNode = currentLevelNode.next;
        LoadLevel(currentLevelNode.level);
    }

    public bool NextLevelUnlocked()
    {
        if (currentLevelNode.next == null || currentLevelNode.status != LevelNodeStatus.Complete) {
            return false;
        }
        if(currentLevelNode.next.status != LevelNodeStatus.Locked)
        {
            return true;
        }
        currentLevelNode.next.GenerateStatus();
        return currentLevelNode.next.status != LevelNodeStatus.Locked;

    }

    public void UpdateSessionData(FinishScreenData finishData)
    {
        sessionData.UpdateLevelRecords(finishData, currentLevel);
        saveData.UpdatePlayerRecord(sessionData.ExportLevelRecordList());
        currentLevelNode.GenerateStatus();
        SaveSerial.SaveGame(saveData);
    }


    public PlayerRecord CurrentPlayerRecord
    {
        get
        {
            if (sessionData == null)
            {
                Debug.Log("Player data is null");
                Awake();
            }
            if (sessionData.playerRecordsDict.ContainsKey(currentLevel.name))
            {
                return sessionData.playerRecordsDict[currentLevel.name];
            }
            if (currentPlayerRecord.levelName is null)
            {
                currentPlayerRecord = new PlayerRecord(currentLevel);
            }
            return currentPlayerRecord;
        }
    }

    public PlayerRecord RecordFromLevel(string levelName)
    {
        if (sessionData.playerRecordsDict.ContainsKey(levelName))
        {
            return sessionData.playerRecordsDict[levelName];
        }
        return null;
    }

    public void AddAttempt()
    {
        currentPlayerRecord.AddAttempt();
    }

    public static GameManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            if (GameObject.FindGameObjectWithTag("GameManager"))
            {
                instance = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
                return instance;
            }
            GameObject managerObject = new GameObject("GameManager");
            instance = managerObject.AddComponent<GameManager>();
            return instance;
        }
    }

    public void PrintMedalCount()
    {
        sessionData.PrintMedalCount();
    }

}
