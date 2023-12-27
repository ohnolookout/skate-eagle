using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using UnityEngine.SceneManagement;
using System.Linq;

[ExecuteAlways]
public class GameManager : MonoBehaviour
{
    private SessionData sessionData;
    public Level currentLevel;
    private static GameManager instance;
    private SaveData saveData;
    public bool goToLevelMenu = false;
    private bool levelIsLoaded = false;
    
    private void Awake()
    {
        //Check to see if other instance exists that has already 
        if (instance != null && instance != this)
        {
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
        instance = this;
        if (SceneManager.GetActiveScene().name == "Level_Editor")
        {
            currentLevel = Resources.Load<Level>("EditorLevel");
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {            
            return;
        }
#endif
        DontDestroyOnLoad(gameObject);
        saveData = SaveSerial.LoadGame();
        Debug.Log($"Loaded data file with {saveData.PlayerRecords().Count} entries, first created on {saveData.startDate}");
        int loadedRecordCount = saveData.recordDict.Count;
        sessionData = new(saveData);
        //If new records were created during session setup, save game.
        if (loadedRecordCount < Session.RecordDict.Count)
        {
            SaveSerial.SaveGame(saveData);
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
        levelIsLoaded = true;
        SceneManager.LoadScene("City");
        AudioManager.Instance.ClearLoops();
    }


    public void BackToLevelMenu()
    {
        goToLevelMenu = true;
        levelIsLoaded = false;
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
            //If sessionData hasn't been created, it means that GM is being run in editor mode, so Awake may need to be called manually.
            if(sessionData == null)
            {
                Awake();
            }
            return sessionData.Record(currentLevel);
        }
    }

    public LevelNode CurrentLevelNode
    {
        get
        {

            return sessionData.Node(currentLevel.UID);
        }
    }


    public static GameManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            Debug.Log("No instance found. Creating instance.");
            GameObject managerObject = new GameObject("GameManager");
            instance = managerObject.AddComponent<GameManager>();
            return instance;
        }
    }

    public SessionData Session
    {
        get
        {
            if(sessionData == null)
            {
                Awake();
            }
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

    public bool LevelIsLoaded { get => levelIsLoaded; set => levelIsLoaded = value; }
}
