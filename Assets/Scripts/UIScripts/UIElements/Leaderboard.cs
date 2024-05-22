using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;
using System.Threading.Tasks;

public enum LeaderboardActivation { MedalLocked, LoginLocked, NoLeaderboard, Active}
public class Leaderboard: MonoBehaviour
{
    [SerializeField] private LeaderboardRow[] _leaderboardRows;
    [SerializeField] private GameObject _medalLockPanel, _loginLockPanel, _noLeaderboardPanel;
    [SerializeField] private GameObject[] _grayOuts;
    private int _playerRank, _startRank, _displayCount;
    private LootLockerGetMemberRankResponse _playerRankResponse;
    private LeaderboardManager _leaderboardManager;
    private string _leaderboardKey, _playerID;
    private bool _isFirstPage = false, _isLastPage;
    private LeaderboardActivation _activationStatus;

    void Start()
    {
        _leaderboardManager = GameManager.Instance.Leaderboard;
        _displayCount = _leaderboardRows.Length;
        _leaderboardKey = GameManager.Instance.CurrentLevel.leaderboardKey;
        _playerID = PlayerPrefs.GetString("PlayerID");
        ValidateActivation();
        if (_activationStatus == LeaderboardActivation.Active)
        {
            PopulateLeaderboard(_leaderboardKey, _playerID);
        }
        else
        {
            GrayOut();
        }
    }

    private void ValidateActivation()
    {
        if(_leaderboardKey == "None")
        {
            _activationStatus = LeaderboardActivation.NoLeaderboard;
            _noLeaderboardPanel.SetActive(true);
            return;
        }

        LoginStatus loginStatus = GameManager.Instance.LoginStatus;

        //Need to change to logged in
        if (loginStatus == LoginStatus.Offline)
        {
            _activationStatus = LeaderboardActivation.LoginLocked;
            _loginLockPanel.SetActive(true);
            return;
        }

        if((int)GameManager.Instance.CurrentPlayerRecord.medal > 3)
        {
            _activationStatus = LeaderboardActivation.MedalLocked;
            _medalLockPanel.SetActive(true);
            return;
        }

        _activationStatus = LeaderboardActivation.Active;
        
    }

    private void GrayOut()
    {
        foreach(var gray in _grayOuts)
        {
            gray.SetActive(true);
        }
    }

    #region Populate Members
    private async Task PopulateLeaderboard(string leaderboardKey, string playerID)
    {
        _leaderboardKey = leaderboardKey;
        _playerRank = await GetPlayerRank(leaderboardKey, playerID);
        GoToPlayerRank();
    }

    private async Task UpdateMembers(int startRank)
    {
        LootLockerLeaderboardMember[] leaderboardMembers = await _leaderboardManager.GetLeaderboardFromRank(_leaderboardKey, startRank, _displayCount);
        Debug.Log($"Updating leaderboard with {leaderboardMembers.Length} members");
        for (int i = 0; i < leaderboardMembers.Length; i++)
        {
            _leaderboardRows[i].PanelIsActive(true);
            _leaderboardRows[i].SetValues(startRank + i + 1, leaderboardMembers[i].player.name, leaderboardMembers[i].score);
        }

        _isFirstPage = _startRank == 0;
        _isLastPage = leaderboardMembers.Length < _displayCount;
        if (_isLastPage)
        {
            for(int i = leaderboardMembers.Length; i < _displayCount; i++)
            {
                _leaderboardRows[i].PanelIsActive(false);
            }
        }
        UpdateButtonFormat();
    }

    private async Task<int> GetPlayerRank(string leaderboardKey, string playerID)
    {
        _playerRankResponse = await _leaderboardManager.GetPlayerRank(leaderboardKey, playerID);
        return _playerRankResponse.rank;
    }
    #endregion

    #region Navigation

    public void GoToPlayerRank()
    {
        _startRank = _playerRank < 6 ? 0 : _playerRank - (_displayCount / 2);
        UpdateMembers(_startRank);
    }

    public void GoToFirstPage()
    {
        _startRank = 0;
        UpdateMembers(_startRank);
    }

    public void GoToLastPage()
    {
        //_startRank = leaderboardLength - _length
        UpdateMembers(_startRank);
    }

    public void NextPage()
    {
        _startRank += 10;
        UpdateMembers(_startRank);
    }

    public void LastPage()
    {
        if (_startRank <= 10)
        {
            return;
        }

        _startRank -= 10;
        UpdateMembers(_startRank);
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
