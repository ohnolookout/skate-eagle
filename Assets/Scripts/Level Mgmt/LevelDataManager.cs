using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelDataManager : MonoBehaviour
{
    public static SessionData sessionData;
    public static Level currentLevel;
    public static LevelRecords currentLevelRecords;
    private static SaveData saveData;
    private void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("LevelManager").Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        saveData = SaveSerial.LoadGame();
        Debug.Log($"Loaded data file that was first created on {saveData.startDate}");
        sessionData = new(saveData);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    public void ResetSaveData() 
    {
        saveData = SaveSerial.NewGame();
        sessionData = new(saveData);
    }

    public void LoadLevel(Level level)
    {
        currentLevel = level;
        Debug.Log($"Loading current level {currentLevel}");
        SceneManager.LoadScene("City");
        if(!(sessionData.levelRecordsDict.ContainsKey(currentLevel.Name)))
        {
            sessionData.AddLevel(currentLevel);
        }
        currentLevelRecords = sessionData.levelRecordsDict[currentLevel.Name];
    }

    public void UpdateSessionData(FinishScreenData finishData)
    {
        sessionData.UpdateLevelRecords(finishData, currentLevel);
        saveData.UpdateLevelRecords(sessionData.ExportLevelRecordList());
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


}
