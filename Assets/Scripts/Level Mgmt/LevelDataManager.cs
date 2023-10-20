using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelDataManager : MonoBehaviour
{
    public SessionData sessionData;
    public Level currentLevel;
    public LevelRecords currentLevelRecords;
    private static LevelDataManager instance;
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
        Debug.Log($"Level nodes loaded: {levelList.levelNodes.Count}");
    }

    private void Start()
    {
        Debug.Log("Starting...");
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
        if (!(sessionData.levelRecordsDict.ContainsKey(currentLevel.Name)))
        {
            sessionData.AddLevel(currentLevel);
        }
        currentLevelRecords = sessionData.levelRecordsDict[currentLevel.Name];
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
            Debug.Log($"next node null: {currentLevelNode.next == null} Current node status: {currentLevelNode.status}");
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
        saveData.UpdateLevelRecords(sessionData.ExportLevelRecordList());
        currentLevelNode.GenerateStatus();
        SaveSerial.SaveGame(saveData);
    }


    public LevelRecords CurrentLevelRecords
    {
        get
        {
            if (sessionData == null)
            {
                Debug.Log("Player data is null");
                Awake();
            }
            if (sessionData.levelRecordsDict.ContainsKey(currentLevel.name))
            {
                return sessionData.levelRecordsDict[currentLevel.name];
            }
            if (currentLevelRecords.levelName is null)
            {
                currentLevelRecords = new LevelRecords(currentLevel);
            }
            return currentLevelRecords;
        }
    }

    public LevelRecords RecordFromLevel(string levelName)
    {
        if (sessionData.levelRecordsDict.ContainsKey(levelName))
        {
            return sessionData.levelRecordsDict[levelName];
        }
        return null;
    }

    public void AddAttempt()
    {
        currentLevelRecords.AddAttempt();
    }

    public static LevelDataManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            if (GameObject.FindGameObjectWithTag("LevelManager"))
            {
                instance = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelDataManager>();
                return instance;
            }
            GameObject managerObject = new GameObject("LevelManager");
            instance = managerObject.AddComponent<LevelDataManager>();
            return instance;
        }
    }

    public void PrintMedalCount()
    {
        sessionData.PrintMedalCount();
    }

}
