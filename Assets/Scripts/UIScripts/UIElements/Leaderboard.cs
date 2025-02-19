using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine.UI;

public enum LeaderboardActivation { MedalLocked, LoginLocked, NoLeaderboard, Active}
public class Leaderboard : MonoBehaviour
{
    [SerializeField] private LeaderboardRow[] _leaderboardRows;
    [SerializeField] private GameObject _medalLockPanel, _loginLockPanel, _noLeaderboardPanel;
    [SerializeField] private GameObject[] _grayOuts;
    private int _displayCount, _playerRank = -1, _startRank = 0, _highlightedRowIndex = -1, _lastRank = -1;
    private string _leaderboardKey;
    private bool _isFirstPage = false, _isLastPage = false;
    private LeaderboardActivation _activationStatus;
    public Button FirstPageButton;
    public Button PreviousPageButton;
    public Button PlayerPageButton;
    public Button NextPageButton;
    private static Color _buttonColorEnabled = new(1, 1, 1, 1);
    private static Color _buttonColorDisabled = new(160 / 255f, 160 / 255f, 160 / 255f, 61 / 255f);

    private bool IsLastPage { 
        get => _isLastPage; 
        set{
            _isLastPage = value;
            FormatLastPageButtons(!value);
        }
    }

    private bool IsFirstPage
    {
        get => _isFirstPage;
        set
        {
            _isFirstPage = value;
            FormatFirstPageButtons(!value);
        }
    }

    private int _playerLowerBound => _playerRank - _displayCount;

    void Awake()
    {
        _displayCount = _leaderboardRows.Length;
        LevelManager.OnLanding += _ => Initialize();

        FirstPageButton.onClick.AddListener(GoToFirstPage);
        PreviousPageButton.onClick.AddListener(PreviousPage);
        PlayerPageButton.onClick.AddListener(GoToPlayerRank);
        NextPageButton.onClick.AddListener(NextPage);

    }

    private void Initialize()
    {
        ValidateActivation();
        if (_activationStatus == LeaderboardActivation.Active)
        {
            PopulateLeaderboard(GameManager.Instance.CurrentLevel.LeaderboardKey);
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
            _startRank = leaderboardResult.Leaderboard[0].Position;
        } else if (leaderboardResult.Leaderboard.Count == 0 && _startRank >= _displayCount)
        {
            _lastRank = _startRank - 1;
            PreviousPage();
            return;
        }
        else
        {
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
        //If row is highlighted, unhighlight on member update
        if(_highlightedRowIndex > -1)
        {
            _leaderboardRows[_highlightedRowIndex].Unhighlight(_highlightedRowIndex);
        }

        bool doLookForPlayerId = _playerRank == -1;
        bool doLookForPlayerRank;
        if (doLookForPlayerId)
        {
            doLookForPlayerRank = false;
        }
        else
        {
            doLookForPlayerRank = _startRank >= _playerLowerBound && _startRank <= _playerRank;

            //If not looking for player rank, reset _highlightedRowIndex
            if (!doLookForPlayerRank)
            {
                _highlightedRowIndex = -1;
            }
        }

        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            var displayName = PlayFabManager.FormatDisplayName(leaderboardEntries[i].DisplayName);
            _leaderboardRows[i].PanelIsActive(true);
            _leaderboardRows[i].SetValues(leaderboardEntries[i].Position + 1, displayName, leaderboardEntries[i].StatValue * -1);

            //Check for player
            if (doLookForPlayerId)
            {
                if (leaderboardEntries[i].PlayFabId == PlayFabAuthService.PlayFabId)
                {
                    doLookForPlayerId = false;
                    _playerRank = _startRank + i;
                    _highlightedRowIndex = i;
                }
            } else if (doLookForPlayerRank && _playerRank == _startRank + i)
            {
                doLookForPlayerRank = false;
                _highlightedRowIndex = i;
            }
        }

        if(_highlightedRowIndex != -1)
        {
            _leaderboardRows[_highlightedRowIndex].Highlight();
        }

        IsFirstPage = _startRank == 0;
        IsLastPage = leaderboardEntries.Count < _displayCount || 
            (_lastRank != -1 && _startRank + _displayCount >= _lastRank) ||
            _highlightedRowIndex > 5;
        if (IsLastPage)
        {
            _lastRank = _startRank + leaderboardEntries.Count;
            for (int i = leaderboardEntries.Count; i < _displayCount; i++)
            {
                _leaderboardRows[i].PanelIsActive(false);
            }
        }
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
            _startRank = leaderboardResult.Leaderboard[0].Position;
        }
        else
        {
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
        RequestLeaderboardAroundPlayer(_leaderboardKey, _displayCount);
    }

    public void GoToFirstPage()
    {
        _startRank = 0;
        RequestLeaderboardFromRank(_leaderboardKey, _startRank, _displayCount);
    }
    public void NextPage()
    {
        _startRank += 10;
        RequestLeaderboardFromRank(_leaderboardKey, _startRank, _displayCount);
    }

    public void PreviousPage()
    {
        if (_startRank <= 10)
        {
            _startRank = 0;
        } else
        {
            _startRank -= 10;
        }

        RequestLeaderboardFromRank(_leaderboardKey, _startRank);
    }


    private void FormatLastPageButtons(bool doEnable)
    {
        if (doEnable)
        {
            NextPageButton.image.color = _buttonColorEnabled;
            NextPageButton.interactable = true;
        }
        else
        {
            NextPageButton.image.color = _buttonColorDisabled;
            NextPageButton.interactable = false;
        }
    }

    private void FormatFirstPageButtons(bool doEnable)
    {
        if (doEnable)
        {
            PreviousPageButton.image.color = _buttonColorEnabled;
            PreviousPageButton.interactable = true;
            FirstPageButton.image.color = _buttonColorEnabled;
            FirstPageButton.interactable = true;
        }
        else
        {
            PreviousPageButton.image.color = _buttonColorDisabled;
            PreviousPageButton.interactable = false;
            FirstPageButton.image.color = _buttonColorDisabled;
            FirstPageButton.interactable = false;
        }
    }
    #endregion
}
