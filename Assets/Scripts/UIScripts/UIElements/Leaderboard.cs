using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;

public enum LeaderboardActivation { MedalLocked, LoginLocked, NoLeaderboard, Active}
public class Leaderboard : MonoBehaviour
{
    [SerializeField] private LeaderboardRow[] _leaderboardRows;
    [SerializeField] private GameObject _medalLockPanel, _loginLockPanel, _noLeaderboardPanel;
    [SerializeField] private GameObject[] _grayOuts;
    private int _playerRank, _startRank, _displayCount;
    private string _leaderboardKey, _playerID;
    private bool _isFirstPage = false, _isLastPage;
    private LeaderboardActivation _activationStatus;

    void Start()
    {
        _displayCount = _leaderboardRows.Length;
        _playerID = PlayerPrefs.GetString("PlayerID");
        ValidateActivation();
        if (_activationStatus == LeaderboardActivation.Active)
        {
            PopulateLeaderboard(GameManager.Instance.CurrentLevel.leaderboardKey);
        }
        else
        {
            GrayOut();
        }
    }

    private void ValidateActivation()
    {
        if (_leaderboardKey == "None")
        {
            _activationStatus = LeaderboardActivation.NoLeaderboard;
            _noLeaderboardPanel.SetActive(true);
            return;
        }

        if (!GameManager.Instance.InitializationResult.isLoggedIn)
        {
            _activationStatus = LeaderboardActivation.LoginLocked;
            _loginLockPanel.SetActive(true);
            return;
        }

        if ((int)GameManager.Instance.CurrentPlayerRecord.medal > 3)
        {
            _activationStatus = LeaderboardActivation.MedalLocked;
            _medalLockPanel.SetActive(true);
            return;
        }

        _activationStatus = LeaderboardActivation.Active;

    }

    private void GrayOut()
    {
        foreach (var gray in _grayOuts)
        {
            gray.SetActive(true);
        }
    }

    #region Populate Members
    private void PopulateLeaderboard(string leaderboardKey)
    {
        _leaderboardKey = leaderboardKey;
        GoToPlayerRank();
    }

    private void OnLeaderboardRequestSuccess(GetLeaderboardResult leaderboardResult)
    {
        if (leaderboardResult.Leaderboard.Count > 0)
        {
            Debug.Log("Setting start rank to " + leaderboardResult.Leaderboard[0].Position);
            _startRank = leaderboardResult.Leaderboard[0].Position;
        }
        else
        {
            Debug.Log("No leaderboard members found, setting start rank to default of 0");
            _startRank = 0;
        }
        UpdateMembers(leaderboardResult.Leaderboard);
    }

    private void OnLeaderboardRequestFailure(PlayFabError leaderboardResult)
    {
        Debug.Log("Leaderboard request failed!");
        _activationStatus = LeaderboardActivation.LoginLocked;
        _loginLockPanel.SetActive(true);
    }

    private void UpdateMembers(List<PlayerLeaderboardEntry> leaderboardEntries)
    {
        Debug.Log($"Updating leaderboard with {leaderboardEntries.Count} members");
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            var displayName = PlayFabManager.FormatDisplayName(leaderboardEntries[i].DisplayName);
            _leaderboardRows[i].PanelIsActive(true);
            _leaderboardRows[i].SetValues(leaderboardEntries[i].Position + 1, displayName, leaderboardEntries[i].StatValue * -1);
        }

        _isFirstPage = _startRank == 0;
        _isLastPage = leaderboardEntries.Count < _displayCount;
        if (_isLastPage)
        {
            for (int i = leaderboardEntries.Count; i < _displayCount; i++)
            {
                _leaderboardRows[i].PanelIsActive(false);
            }
        }
        UpdateButtonFormat();
    }

    private void RequestLeaderboardFromRank(string leaderboardName, int startPosition, int maxResultsCount = 10)
    {
        PlayFabClientAPI.GetLeaderboard(
            new GetLeaderboardRequest()
            {
                StartPosition = startPosition,
                StatisticName = leaderboardName,
                CustomTags = null,
                MaxResultsCount = maxResultsCount
            },
            OnLeaderboardRequestSuccess,
            OnLeaderboardRequestFailure
            );
    }

    private void RequestLeaderboardAroundPlayer(string leaderboardName, int maxResultsCount = 10)
    {
        PlayFabClientAPI.GetLeaderboardAroundPlayer(
            new GetLeaderboardAroundPlayerRequest()
            {
                StatisticName = leaderboardName,
                CustomTags = null,
                MaxResultsCount = maxResultsCount
            },
            OnLeaderboardAroundPlayerRequestSuccess,
            OnLeaderboardAroundPlayerRequestFailure
            );
        
    }

    private void OnLeaderboardAroundPlayerRequestSuccess(GetLeaderboardAroundPlayerResult leaderboardResult)
    {
        if (leaderboardResult.Leaderboard.Count > 0)
        {
            Debug.Log("Setting start rank to " + leaderboardResult.Leaderboard[0].Position);
            _startRank = leaderboardResult.Leaderboard[0].Position;
        }
        else
        {
            Debug.Log("No leaderboard members found, setting start rank to default of 0");
            _startRank = 0;
        }
        UpdateMembers(leaderboardResult.Leaderboard);
    }

    private void OnLeaderboardAroundPlayerRequestFailure(PlayFabError error)
    {
        Debug.Log("Leaderboard around player request failed!");
        _activationStatus = LeaderboardActivation.LoginLocked;
        _loginLockPanel.SetActive(true);
    }

    #endregion

    #region Navigation

    public void GoToPlayerRank()
    {
        RequestLeaderboardAroundPlayer(_leaderboardKey);
    }

    public void GoToFirstPage()
    {
        _startRank = 0;
        RequestLeaderboardFromRank(_leaderboardKey, _startRank);
    }
    public void NextPage()
    {
        _startRank += 10;
        RequestLeaderboardFromRank(_leaderboardKey, _startRank);
    }

    public void PreviousPage()
    {
        if (_startRank == 0)
        {
            return;
        } else if (_startRank <= 10)
        {
            _startRank = 0;
        } else
        {
            _startRank -= 10;
        }

        RequestLeaderboardFromRank(_leaderboardKey, _startRank);
    }

    private void UpdateButtonFormat()
    {
        if(_activationStatus != LeaderboardActivation.Active)
        {

        }

        if (_isFirstPage)
        {

        }

        if (_isLastPage)
        {

        }

        if (_startRank < _playerRank && _playerRank < _startRank + _displayCount)
        {

        }
    }
    #endregion
}
