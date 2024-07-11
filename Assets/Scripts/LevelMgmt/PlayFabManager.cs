using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using PlayFab.ClientModels;
using PlayFab;
using AYellowpaper.SerializedCollections;
using Newtonsoft.Json;

public struct InitializationResult
{
    public bool isLoggedIn;
    public bool isFirstTime;
    public bool savedToCloud;
    public SessionData sessionData;

}

public class PlayFabManager : MonoBehaviour
{
    #region Declarations
    private InitializationResult result;
    private bool _isAwaitingInitialize = false;
    private bool _isAwaitingDirtyRecord = false;
    private float _lastSeenCloud = float.PositiveInfinity;
    const string _SeenBeforeKey = "SeenBefore";
    const string _CreateAccountOnSuccessfulAuth = "CreateAccountOnSuccessfulAuth";
    private string _submittedName = null;
    private GameManager _gameManager;
    private SaveLoadUtility _saveLoadUtility = SaveLoadUtility.Instance;
    private PlayerRecord _currentRecord;
    PlayFabAuthService _authService = PlayFabAuthService.Instance;

    public Action<bool, bool, float> OnSilentAuthComplete;
    public Action<bool> OnEmailAuthComplete;
    public Action<bool, string, string> OnSetNameComplete;
    public Action<PlayerRecord, bool> OnLeadboardUpdateComplete;
    public Action<InitializationResult> OnInitializationComplete;
    public Action<bool> OnCheckCloudSaveComplete;
    public Action<bool> OnSaveToCloudComplete;
    #endregion

    #region Initializer
    public IEnumerator Initialize(GameManager gameManager)
    {
        Debug.Log("Staring PlayFab initialization...");
        _gameManager = gameManager;
        result = new();

        //Silent authentication
        _isAwaitingInitialize = true;
        OnSilentAuthComplete += OnSilentAuthComplete;
        SilentAuth();

        yield return new WaitWhile(() => _isAwaitingInitialize);
        Debug.Log("Silent auth complete.");

        //Load local save
        _gameManager.SessionData = _saveLoadUtility.LoadGame();
        Debug.Log("Local save loaded.");

        //Submit any dirty records if logged in
        if (result.isLoggedIn && _gameManager.SessionData.SaveData.dirtyRecords.Count > 0)
        {
            Debug.Log("Submitting dirty records...");
            _isAwaitingInitialize = true;
            StartCoroutine(SubmitDirtyRecords());
        }
        else
        {
            Debug.Log("No dirty records to submit.");
        }

        yield return new WaitWhile(() => _isAwaitingInitialize);
        Debug.Log("Submit dirty records complete.");

        //Merge cloud save if cloud save is more recent than local save
        var lastSeenLocal = _gameManager.SessionData.SaveData.lastSaved.ToBinary();
        _isAwaitingInitialize = true;
        CheckCloudLoad(lastSeenLocal, _lastSeenCloud);

        yield return new WaitWhile(() => _isAwaitingInitialize);
        Debug.Log("Load cloud save complete.");

        //Save game locally and to cloud

        var saveDataString = _saveLoadUtility.SaveGame(_gameManager.SessionData);
        if (result.isLoggedIn)
        {
            _isAwaitingInitialize = true;
            SaveToCloud(saveDataString);
        }

        yield return new WaitWhile(() => _isAwaitingInitialize);
        Debug.Log("Save to cloud complete.");

        //Return result with updated session data
        result.sessionData = _gameManager.SessionData;
        OnInitializationComplete?.Invoke(result);
        OnInitializationComplete = null;
    }
    #endregion

    #region Silent Auth
    public void SilentAuth()
    {
        PlayFabAuthService.OnLoginSuccess += OnSilentAuthSuccess;
        PlayFabAuthService.OnPlayFabError += OnSilentAuthFailure;
        _authService.Authenticate(Authtypes.Silent);
    }
    private void OnSilentAuthSuccess(LoginResult result)
    {
        PlayFabAuthService.OnLoginSuccess -= OnSilentAuthSuccess;
        PlayFabAuthService.OnPlayFabError -= OnSilentAuthFailure;
        var isFirstTime = CheckFirstTimeUser(true);
        SilentAuthComplete(true, isFirstTime, ((DateTime)result.LastLoginTime).ToBinary());
    }

    private void OnSilentAuthFailure(PlayFabError error)
    {
        PlayFabAuthService.OnLoginSuccess -= OnSilentAuthSuccess;
        PlayFabAuthService.OnPlayFabError -= OnSilentAuthFailure;
        var isFirstTime = CheckFirstTimeUser(false);
        SilentAuthComplete(false, isFirstTime, float.PositiveInfinity);
    }

    private void SilentAuthComplete(bool isLoggedIn, bool isFirstTime, float cloudLastSeen)
    {

        result.isLoggedIn = isLoggedIn;
        result.isFirstTime = isFirstTime;
        _lastSeenCloud = cloudLastSeen;
        _isAwaitingInitialize = false;
        OnSilentAuthComplete?.Invoke(isLoggedIn, isFirstTime, cloudLastSeen);
        OnSilentAuthComplete = null;
    }
    private bool CheckFirstTimeUser(bool isLoggedIn)
    {
        if (PlayerPrefs.GetInt(_SeenBeforeKey) != 1)
        {
            PlayerPrefs.SetInt(_SeenBeforeKey, 1);

            if (isLoggedIn)
            {
                PlayerPrefs.SetInt(_CreateAccountOnSuccessfulAuth, 0);
            }
            else
            {
                PlayerPrefs.SetInt(_CreateAccountOnSuccessfulAuth, 1);
            }

            return true;
        }

        return false;
    }
    #endregion

    #region Add Email
    public void EmailAuth(string email, string password, string confirmPassword)
    {
        _authService.Email = email;
        _authService.Password = password;

        PlayFabAuthService.OnLoginSuccess += OnEmailAuthSuccess;
        PlayFabAuthService.OnPlayFabError += OnEmailAuthFailure;

        _authService.Authenticate(Authtypes.EmailAndPassword);
    }

    private void OnEmailAuthSuccess(LoginResult result)
    {
        PlayFabAuthService.OnLoginSuccess -= OnEmailAuthSuccess;
        PlayFabAuthService.OnPlayFabError -= OnEmailAuthFailure;
        var isFirstTime = CheckFirstTimeUser(true);
        OnEmailAuthComplete(true);
    }

    private void OnEmailAuthFailure(PlayFabError error)
    {
        PlayFabAuthService.OnLoginSuccess -= OnEmailAuthSuccess;
        PlayFabAuthService.OnPlayFabError -= OnEmailAuthFailure;
        var isFirstTime = CheckFirstTimeUser(false);
        OnEmailAuthComplete(false);
    }

    private void EmailAuthComplete(bool isSuccess)
    {
        OnEmailAuthComplete?.Invoke(isSuccess);
    }

    #endregion

    #region Set Name
    public void SetDisplayName(string displayName)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(
        new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = displayName,
        },

        OnSetNameSuccess,
        OnSetNameFailure
        );
    }

    private void OnSetNameSuccess(UpdateUserTitleDisplayNameResult result)
    {
        SetNameComplete(true, _submittedName, result.DisplayName);
    }

    private void OnSetNameFailure(PlayFabError error)
    {
        SetNameComplete(false, _submittedName, null);
    }

    private void SetNameComplete(bool isSuccess, string submittedName = null, string displayName = null)
    {
        OnSetNameComplete?.Invoke(isSuccess, submittedName, displayName);
        OnSetNameComplete = null;
    }

    public void GenerateName()
    {
        var name = "SkateEagle";
        SetDisplayName(name);
    }
    #endregion

    #region Save To Cloud

    public void SaveToCloud(string data)
    {
        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest()
            {
                Data = new Dictionary<string, string>()
                {
                    {"SaveData", data}
                }
            },
            OnUpdateUserDataSuccess,
            OnUpdateUserDataFailure
        );
    }

    private void OnUpdateUserDataSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Save successfully backed up on PlayFab.");
        SaveToCloudComplete(true);
    }

    private void OnUpdateUserDataFailure(PlayFabError error)
    {
        Debug.Log("PlayFab save failed.");
        SaveToCloudComplete(false);
    }

    private void SaveToCloudComplete(bool isSuccess)
    {
        _isAwaitingInitialize = false;
        OnSaveToCloudComplete?.Invoke(isSuccess);
        OnSaveToCloudComplete = null;
    }
    #endregion

    #region Load From Cloud
    public void CheckCloudLoad(float lastSeenLocal, float lastSeenCloud)
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString(GameManager.RegisteredEmailKey)))
        {
            if (!float.IsPositiveInfinity(lastSeenCloud) && lastSeenCloud > lastSeenLocal)
            {
                Debug.Log("Local save out of date. Syncing with cloud save...");
                LoadFromCloud();
            }
        }
        else
        {
            Debug.Log("Local save is up to date with cloud.");
            CheckCloudSaveComplete(false);
        }
    }
    public void LoadFromCloud()
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest()
            {
                PlayFabId = PlayFabAuthService.PlayFabId
            },
            OnLoadFromCloudSuccess,
            OnLoadFromCloudFailure
            );
    }

    private void OnLoadFromCloudSuccess(GetUserDataResult result)
    {
        SaveData cloudSaveData = JsonConvert.DeserializeObject<SaveData>(result.Data["SaveData"].Value);
        var mergedSave = SaveLoadUtility.MergeSaveData(GameManager.Instance.SessionData.SaveData, cloudSaveData);
        GameManager.Instance.LoadExternalGame(mergedSave);
        CheckCloudSaveComplete(true);
        Debug.Log("Save successfully loaded from cloud and merged.");

    }

    private void OnLoadFromCloudFailure(PlayFabError error)
    {
        Debug.Log("Cloud load failed.");
        CheckCloudSaveComplete(false);
    }

    private void CheckCloudSaveComplete(bool isSuccess)
    {
        OnCheckCloudSaveComplete?.Invoke(isSuccess);
        OnCheckCloudSaveComplete = null;
        _isAwaitingInitialize = false;
    }
    #endregion

    #region Dirty Records Handling
    private IEnumerator SubmitDirtyRecords()
    {
        var dirtyUIDs = _gameManager.SessionData.SaveData.dirtyRecords.Keys.ToList();
        foreach (string levelUID in dirtyUIDs)
        {
            _isAwaitingDirtyRecord = true;
            Debug.Log("Uploading dirty record for level " + _gameManager.SessionData.NodeDict[levelUID].Level.Name);
            OnLeadboardUpdateComplete += OnSubmitDirtyRecordComplete;
            UpdateLeaderboardRecord(_gameManager.SessionData.SaveData.dirtyRecords[levelUID]);
            yield return new WaitWhile(() => _isAwaitingDirtyRecord);
        }

        _isAwaitingInitialize = false;
    }

    private void OnSubmitDirtyRecordComplete(PlayerRecord record, bool isSuccess)
    {
        OnLeadboardUpdateComplete -= OnSubmitDirtyRecordComplete;
        if (isSuccess)
        {
            Debug.Log("Dirty record upload successful!");
        }
        _isAwaitingDirtyRecord = false;
    }
    #endregion

    #region Uploading Records
    public void UpdateLeaderboardRecord(PlayerRecord record)
    {
        _currentRecord = record;
        List<StatisticUpdate> stats = new()
        {
            new StatisticUpdate()
            {
                StatisticName = record.leaderboardKey,
                Value = (int)(record.bestTime * 1000)
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(
            new UpdatePlayerStatisticsRequest()
            {
                Statistics = stats
            },
            OnUpdateLeaderboardRecordSuccess,
            OnUpdateLeaderboardRecordFailure
        );
    }

    private void OnUpdateLeaderboardRecordSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Record successfully uploaded to leaderboard!");
        LeaderboardUpdateCompelte(_currentRecord, false);
    }

    private void OnUpdateLeaderboardRecordFailure(PlayFabError error)
    {
        Debug.Log("Failed to upload record to leaderboard!");
        Debug.Log("Setting record to dirty because player is not logged in.");
        //Check to see if error is because of lack of connection, set isloggedin to false if no connection?
        LeaderboardUpdateCompelte(_currentRecord, false);
    }

    private void LeaderboardUpdateCompelte(PlayerRecord record, bool isSuccess)
    {
        OnLeadboardUpdateComplete?.Invoke(record, isSuccess);
        OnLeadboardUpdateComplete = null;

    }
    #endregion

}
