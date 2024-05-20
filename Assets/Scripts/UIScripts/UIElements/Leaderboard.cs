using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;
using System.Threading.Tasks;

public class Leaderboard: MonoBehaviour
{
    [SerializeField] private LeaderboardRow[] _leaderboardRows;
    private int _playerRank, _startRank, _displayCount;
    private LootLockerGetMemberRankResponse _playerRankResponse;
    private LeaderboardManager _leaderboardManager;
    private string _leaderboardKey, _playerID;
    private bool _isFirstPage = false, _isLastPage;

    void Start()
    {
        _leaderboardManager = GameManager.Instance.Leaderboard;
        _displayCount = _leaderboardRows.Length;
        _leaderboardKey = GameManager.Instance.CurrentLevel.leaderboardKey;
        _playerID = PlayerPrefs.GetString("PlayerID");
        PopulateLeaderboard(_leaderboardKey, _playerID);
    }

    #region Populate Members
    private async Task PopulateLeaderboard(string leaderboardKey, string playerID)
    {
        //Need to change to logged in
        if(GameManager.Instance.LoginStatus == LoginStatus.Offline)
        {
            Debug.Log("Player is offline. Leaderboard disabled.");
            return;
        }
        _leaderboardKey = leaderboardKey;

        if(_leaderboardKey == "None")
        {
            Debug.Log("Level does not have leaderboard.");
            return;
        }

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
