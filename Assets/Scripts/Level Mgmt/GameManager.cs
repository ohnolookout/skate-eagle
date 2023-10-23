using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private SessionData sessionData;
    private Level currentLevel;
    private static GameManager instance;
    private SaveData saveData;
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
        Debug.Log($"Loaded data file with {saveData.PlayerRecords().Count} entries, first created on {saveData.startDate}");
        int loadedRecordCount = saveData.recordDict.Count;
        sessionData = new(saveData);
        //If new records were created during session setup, save game.
        Debug.Log($"First record in session data. Name: {Session.RecordDict[Session.RecordDict.Keys.ToList<string>()[0]].levelName} " +
            $"UID: {Session.RecordDict[Session.RecordDict.Keys.ToList<string>()[0]].UID}");
        if (loadedRecordCount < Session.RecordDict.Count)
        {
            Debug.Log($"First record before saved data. Name: {saveData.PlayerRecords()[saveData.PlayerRecords().Keys.ToList<string>()[0]].levelName}" +
                $" UID: {saveData.PlayerRecords()[saveData.PlayerRecords().Keys.ToList<string>()[0]].UID}");
            SaveSerial.SaveGame(saveData);
            Debug.Log($"First record in saved data. Name {saveData.PlayerRecords()[saveData.PlayerRecords().Keys.ToList<string>()[0]].levelName}" +
                $" UID: {saveData.PlayerRecords()[saveData.PlayerRecords().Keys.ToList<string>()[0]].UID}");
        }
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
    }


    public void BackToLevelMenu()
    {
        goToLevelMenu = true;
        SceneManager.LoadScene("Start_Menu");
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
        return sessionData.NextLevelUnlocked(currentLevel);

    }

    public void UpdateRecord(FinishScreenData finishData)
    {
        Session.UpdateRecord(finishData, CurrentLevel);
        SaveSerial.SaveGame(saveData);
    }


    public PlayerRecord CurrentPlayerRecord
    {
        get
        {
            return sessionData.Record(currentLevel.UID);
        }
    }

    public LevelNode CurrentLevelNode
    {
        get
        {

            return sessionData.Node(currentLevel.UID);
        }
    }

    public void AddAttempt()
    {
        CurrentPlayerRecord.AddAttempt();
    }

    public static GameManager Instance
    {
        get
        {
            if (instance != null)
            {
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

    public SessionData Session
    {
        get
        {
            return sessionData;
        }
    }

    public Level CurrentLevel
    {
        get
        {
            return currentLevel;
        }
        set
        {
            currentLevel = value;
        }
    }

}
