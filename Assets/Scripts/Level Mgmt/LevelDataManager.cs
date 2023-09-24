using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelDataManager : MonoBehaviour
{
    public PlayerData playerData;
    public Level currentLevel;
    public LevelTimeData currentLevelTimeData;
    public float? currentLevelBestTime;
    public Medal? currentLevelMedal;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SaveData loadedData = SaveSerial.LoadGame();
        if (loadedData is null)
        {
            loadedData = SaveSerial.SaveGame();
        }
        playerData = new(loadedData);
        if (playerData == null)
        {
            Debug.Log("Player data is null");
        }
    }

    public void LoadLevel(Level level)
    {
        currentLevel = level;
        SceneManager.LoadScene("City");
        if(!(playerData.levelTimeDict.ContainsKey(level)))
        {
            playerData.AddLevel(currentLevel);
        }
        currentLevelTimeData = playerData.levelTimeDict[level];
        currentLevelBestTime = currentLevelTimeData.bestTime;
        currentLevelMedal = currentLevelTimeData.medal;
    }

    public Medal UpdateTime(float timeInSeconds)
    {
        currentLevelTimeData.UpdateTime(timeInSeconds, out Medal newMedal, out Medal? oldMedal);
        currentLevelBestTime = currentLevelTimeData.bestTime;
        currentLevelMedal = currentLevelTimeData.medal;
        return newMedal;
    }


    public LevelTimeData PlayerDataForCurrentLevel()
    {
        if(playerData == null)
        {
            Debug.Log("Player data is null");
            Awake();
        }
        if (playerData.levelTimeDict.ContainsKey(currentLevel))
        {
            return playerData.levelTimeDict[currentLevel];
        }
        if (currentLevelTimeData.level is null)
        {
            currentLevelTimeData = new LevelTimeData(currentLevel);
        }
        return currentLevelTimeData;
    }
}
