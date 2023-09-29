using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelDataManager : MonoBehaviour
{
    public SessionData sessionData;
    public Level currentLevel;
    public LevelRecords currentLevelRecords;
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

    public void ResetSaveData() 
    {
        saveData = SaveSerial.NewGame();
        sessionData = new(saveData);
    }

    public void LoadLevel(Level level)
    {
        currentLevel = level;
        SceneManager.LoadScene("City");
        if(!(sessionData.levelTimeDict.ContainsKey(level.Name)))
        {
            sessionData.AddLevel(currentLevel);
        }
        currentLevelRecords = sessionData.levelTimeDict[level.Name];
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
            if (sessionData.levelTimeDict.ContainsKey(currentLevel.name))
            {
                return sessionData.levelTimeDict[currentLevel.name];
            }
            if (currentLevelRecords.levelName is null)
            {
                currentLevelRecords = new LevelRecords(currentLevel);
            }
            return currentLevelRecords;
        }
    }

}
