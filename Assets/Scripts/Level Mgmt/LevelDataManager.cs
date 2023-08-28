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
    }

    public void LoadLevel(Level level)
    {
        currentLevel = level;
        SceneManager.LoadScene("City");
        if(!(playerData.levelTimeDict.ContainsKey(level.Name)))
        {
            playerData.AddLevel(currentLevel);
        }
        currentLevelTimeData = playerData.levelTimeDict[level.Name];
        currentLevelBestTime = currentLevelTimeData.bestTime;
        currentLevelMedal = currentLevelTimeData.medal;
    }

    public void UpdateTime(float timeInSeconds)
    {
        currentLevelTimeData.UpdateTime(timeInSeconds, out Medal? newMedal, out Medal? oldMedal);
        currentLevelBestTime = currentLevelTimeData.bestTime;
        currentLevelMedal = currentLevelTimeData.medal;
    }
}
