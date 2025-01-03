using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using PlayFab.ClientModels;
using PlayFab;
using PlayFab.AdminModels;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public struct InitializationResult
{
    public bool isLoggedIn;
    public bool isFirstTime;
    public bool hasEmail;
    public bool savedToCloud;
    public bool doAskEmail;
    public bool doResetAccount;
    public string displayName;
    public SessionData sessionData;
    public int loginCount;
}

public class PlayFabManager : MonoBehaviour
{
    #region Declarations
    public InitializationResult InitializationResult;
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

    public Action<bool, bool, bool, float> OnSilentAuthComplete;
    public Action<bool, PlayFabError> OnEmailLoginComplete;
    public Action<bool, PlayFabError> OnAddEmailComplete;
    public Action<bool, string, string, PlayFabError> OnSetNameComplete;
    public Action<string, string> OnUpdateStoredName;
    public Action<PlayerRecord, bool> OnLeadboardUpdateComplete;
    public Action<InitializationResult> OnInitializationComplete;
    public Action<bool> OnCheckCloudSaveComplete;
    public Action<bool> OnSaveToCloudComplete;
    public System.Action OnAccountReset;

    public const string SeenBeforeKey = "SeenBefore";
    public const string LastSeenKey = "LastSeen";
    public const string CreateAccountOnSuccessfulAuthKey = "CreateAccountOnSuccessfulAuth";
    public const string RegisteredEmailKey = "RegisteredEmail";
    public const string LoginCountKey = "LoginCount";
    public const string DontAskEmailKey = "DontAskEmail";
    public const string DisplayNameKey = "DisplayName";
    public const string FormattedDisplayNameKey = "FormattedDisplayName";

    public const int AskEmailEveryX = 5;
    const int delayCount = 7;
    #endregion

    #region Initializer
    public IEnumerator Initialize(GameManager gameManager, bool isReset)
    {
        //Debug.Log("Starting PlayFab initialization...");
        _gameManager = gameManager;
        _authService.InfoRequestParams = new()
        {
            GetUserAccountInfo = true
        };
        InitializationResult = new();

        if (isReset)
        {
            InitializationResult.doResetAccount = true;
            yield return new WaitForSeconds(delayCount);
        }
        else
        {
            InitializationResult.doResetAccount = false;
        }

        //Silent authentication
        _isAwaitingInitialize = true;
        OnSilentAuthComplete += OnSilentAuthComplete;
        SilentAuth();

        yield return new WaitWhile(() => _isAwaitingInitialize);
        //Debug.Log("Silent auth complete.");

        //Load local save
        _gameManager.SessionData = _saveLoadUtility.LoadGame();
        //Debug.Log("Local save loaded.");

        //Submit any dirty records if logged in
        //NEEDS TO BE FIXED
        if (InitializationResult.isLoggedIn && _gameManager.SessionData.SaveData.dirtyRecords.Count > 0)
        {
            //Debug.Log("Submitting dirty records...");
            //_isAwaitingInitialize = true;
            //StartCoroutine(SubmitDirtyRecords());
        }
        else
        {
            //Debug.Log("No dirty records to submit.");
        }

        yield return new WaitWhile(() => _isAwaitingInitialize);
        //Debug.Log("Submit dirty records complete.");

        //Merge cloud save if cloud save is more recent than local save
        var lastSeenLocal = _gameManager.SessionData.SaveData.lastSaved.ToBinary();
        _isAwaitingInitialize = true;
        CheckCloudLoad(lastSeenLocal, _lastSeenCloud);

        yield return new WaitWhile(() => _isAwaitingInitialize);
        //Debug.Log("Load cloud save complete.");

        //Save game locally and to cloud

        var saveDataString = _saveLoadUtility.SaveGame(_gameManager.SessionData);
        if (InitializationResult.isLoggedIn)
        {
            _isAwaitingInitialize = true;
            SaveToCloud(saveDataString);
        }

        yield return new WaitWhile(() => _isAwaitingInitialize);
        //Debug.Log("Save to cloud complete.");

        //Return result with updated session data
        InitializationResult.sessionData = _gameManager.SessionData;

        //Increment login count
        int loginCount = PlayerPrefs.GetInt(LoginCountKey, 0) + 1;
        PlayerPrefs.SetInt(LoginCountKey, loginCount);
        InitializationResult.loginCount = loginCount;

        //Determine whether to ask for email
        if(!InitializationResult.hasEmail && PlayerPrefs.GetInt(DontAskEmailKey, 0) == 0 && loginCount % AskEmailEveryX == 0)
        {
            //Debug.Log("Setting ask email to true...");
            InitializationResult.doAskEmail = true;
        }
        else
        {
            //Debug.Log("Setting ask email to false...");
            InitializationResult.doAskEmail = false;
        }

        if(InitializationResult.isLoggedIn && InitializationResult.displayName != PlayerPrefs.GetString(DisplayNameKey) && !string.IsNullOrEmpty(InitializationResult.displayName))
        {
            UpdateStoredName(InitializationResult.displayName);
        }

        //Debug.Log("Ending initialization...");
        OnInitializationComplete?.Invoke(InitializationResult);
    }
    #endregion

    #region Silent Auth
    public void SilentAuth()
    {
        PlayFabAuthService.OnLoginSuccess += OnSilentAuthSuccess;
        PlayFabAuthService.OnPlayFabError += OnSilentAuthFailure;
        if (_authService.RememberMe)
        {
            //Debug.Log("Authenticating via saved email id...");
            _authService.Authenticate(Authtypes.EmailAndPassword);
        }
        else
        {
            //Debug.Log("Authenticating via device id...");
            _authService.Authenticate(Authtypes.Silent);
        }
    }
    private void OnSilentAuthSuccess(LoginResult result)
    {
        PlayFabAuthService.OnLoginSuccess -= OnSilentAuthSuccess;
        PlayFabAuthService.OnPlayFabError -= OnSilentAuthFailure;

        //Debug.Log("Silent auth success.");
        var isFirstTime = CheckFirstTimeUser(true);
        var hasEmail = false;
        string displayName = null;
        long lastLogin;
        if (result.LastLoginTime != null)
        {
            lastLogin = ((DateTime)result.LastLoginTime).ToBinary();
        }
        else
        {
            lastLogin = DateTime.Now.ToBinary();
        }

        if (result.InfoResultPayload != null)
        {
            hasEmail = !String.IsNullOrEmpty(result.InfoResultPayload.AccountInfo.Username);
            displayName = result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        }

        SilentAuthComplete(true, isFirstTime, hasEmail, displayName, lastLogin);
    }

    private void OnSilentAuthFailure(PlayFabError error)
    {
        PlayFabAuthService.OnLoginSuccess -= OnSilentAuthSuccess;
        PlayFabAuthService.OnPlayFabError -= OnSilentAuthFailure;
        //Debug.Log("Silent auth error: " + error.ErrorMessage);
        var isFirstTime = CheckFirstTimeUser(false);
        SilentAuthComplete(false, isFirstTime, false, null, float.PositiveInfinity);
    }

    private void SilentAuthComplete(bool isLoggedIn, bool isFirstTime, bool hasEmail, string displayName, float cloudLastSeen)
    {

        InitializationResult.isLoggedIn = isLoggedIn;
        InitializationResult.isFirstTime = isFirstTime;
        InitializationResult.hasEmail = hasEmail;
        InitializationResult.displayName = displayName;
        _lastSeenCloud = cloudLastSeen;
        _isAwaitingInitialize = false;
        OnSilentAuthComplete?.Invoke(isLoggedIn, isFirstTime, hasEmail, cloudLastSeen);
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

    public void AddEmail(string email, string password)
    {
        _authService.Email = email;
        _authService.Password = password;
        _authService.RememberMe = true;

        PlayFabAuthService.OnLoginSuccess += OnAddEmailSuccess;
        PlayFabAuthService.OnPlayFabError += OnAddEmailFailure;

        _authService.Authenticate(Authtypes.RegisterPlayFabAccount);
    }

    private void OnAddEmailSuccess(LoginResult result)
    {
        PlayFabAuthService.OnLoginSuccess -= OnAddEmailSuccess;
        PlayFabAuthService.OnPlayFabError -= OnAddEmailFailure;
        var isFirstTime = CheckFirstTimeUser(true);
        Debug.Log("Email successfully added!");

        AddEmailComplete(true);
    }

    private void OnAddEmailFailure(PlayFabError error)
    {
        PlayFabAuthService.OnLoginSuccess -= OnAddEmailSuccess;
        PlayFabAuthService.OnPlayFabError -= OnAddEmailFailure;

        Debug.Log("Add email failed: " + error.ErrorMessage);
        var isFirstTime = CheckFirstTimeUser(false);
        AddEmailComplete(false, error);
    }

    private void AddEmailComplete(bool isSuccess, PlayFabError error = null)
    {
        OnAddEmailComplete?.Invoke(isSuccess, error);
    }

    #endregion

    #region Email Login
    //Need to add functionality to merge load and pull username.
    public void EmailLogin(string email, string password)
    {
        _authService.Email = email;
        _authService.Password = password;
        _authService.RememberMe = true;

        PlayFabAuthService.OnLoginSuccess += OnEmailLoginSuccess;
        PlayFabAuthService.OnPlayFabError += OnEmailLoginFailure;

        _authService.Authenticate(Authtypes.EmailAndPassword);
    }
    private void OnEmailLoginSuccess(LoginResult result)
    {
        PlayFabAuthService.OnLoginSuccess -= OnEmailLoginSuccess;
        PlayFabAuthService.OnPlayFabError -= OnEmailLoginFailure;
        var isFirstTime = CheckFirstTimeUser(true);
        EmailLoginComplete(true);
    }

    private void OnEmailLoginFailure(PlayFabError error)
    {
        PlayFabAuthService.OnLoginSuccess -= OnEmailLoginSuccess;
        PlayFabAuthService.OnPlayFabError -= OnEmailLoginFailure;
        var isFirstTime = CheckFirstTimeUser(false);
        EmailLoginComplete(false, error);
    }

    private void EmailLoginComplete(bool isSuccess, PlayFabError error = null)
    {
        OnEmailLoginComplete?.Invoke(isSuccess, error);
    }
    #endregion

    #region Set Name
    public void SetDisplayName(string displayName)
    {
        _submittedName = displayName;
        PlayFabClientAPI.UpdateUserTitleDisplayName(
        new PlayFab.ClientModels.UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = displayName + "#",
        },

        OnSetNameSuccess,
        OnSetNameFailure
        );
    }

    private void OnSetNameSuccess(PlayFab.ClientModels.UpdateUserTitleDisplayNameResult result)
    {
        PlayerPrefs.SetString(DisplayNameKey, result.DisplayName);
        SetNameComplete(true, _submittedName, result.DisplayName);
    }

    private void OnSetNameFailure(PlayFabError error)
    {
        SetNameComplete(false, _submittedName, null, error);
    }

    private void SetNameComplete(bool isSuccess, string submittedName = null, string displayName = null, PlayFabError error = null)
    {
        string formattedDisplayName = "";
        if (isSuccess)
        {
            UpdateStoredName(displayName);
        }
        OnSetNameComplete?.Invoke(isSuccess, submittedName, displayName, error);
        OnSetNameComplete = null;
    }

    public void GenerateName()
    {
        var name = "SkateEagle";
        SetDisplayName(name);
    }

    private void UpdateStoredName(string submittedName)
    {
        PlayerPrefs.SetString(DisplayNameKey, submittedName);
        var formattedDisplayName = FormatDisplayName(submittedName);
        PlayerPrefs.SetString(FormattedDisplayNameKey, formattedDisplayName);
        OnUpdateStoredName?.Invoke(submittedName, formattedDisplayName);
    }

    public static string FormatDisplayName(string name)
    {
        if(name == null)
        {
            return "";
        }
        var splitName = name.Split('#', 2);
        return splitName[0] + "<color=#8A8A8A>#" + splitName[1];
    }

    #endregion

    #region Save To Cloud

    public void SaveToCloud(string data)
    {
        PlayFabClientAPI.UpdateUserData(
            new PlayFab.ClientModels.UpdateUserDataRequest()
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

    private void OnUpdateUserDataSuccess(PlayFab.ClientModels.UpdateUserDataResult result)
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
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString(RegisteredEmailKey)))
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
            new PlayFab.ClientModels.GetUserDataRequest()
            {
                PlayFabId = PlayFabAuthService.PlayFabId
            },
            OnLoadFromCloudSuccess,
            OnLoadFromCloudFailure
            );
    }

    private void OnLoadFromCloudSuccess(PlayFab.ClientModels.GetUserDataResult result)
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
                Value = (int)(record.bestTime * -1000)
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

    #region New Player

    public void DeletePlayerAccount(GameManager gameManager)
    {
        if (!InitializationResult.isLoggedIn)
        {
            Debug.Log("Can't delete account while offline.");
            return;
        }
        _gameManager.OnLoading(true);

        PlayFabAdminAPI.DeletePlayer(
            new DeletePlayerRequest(){ PlayFabId = PlayFabAuthService.PlayFabId },
            (result) =>
            {
                Debug.Log("Player account successfully deleted.");
                gameManager.OnResetAccount();
            },
            (error) =>
            {
                Debug.Log("Error deleting player account: " + error.ErrorMessage);
                gameManager.OnLoading?.Invoke(false);
            }
        );


    }

    #endregion
}
