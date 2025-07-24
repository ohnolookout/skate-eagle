using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.Events;
using UnityEditor;

public class PlayFabTester : MonoBehaviour
{
    public GetPlayerCombinedInfoRequestParams InfoRequestParams;
    private static List<TestPlayer> _testPlayers = new(){
        new("Bart", "001"),
        new("Homer", "002" ),
        new("Marge", "003" ),
        new("Lisa", "004" ),
        new("Maggie", "005" ),
        new("Moe", "006" ),
        new("MrBurns", "007" ),
        new("Smithers", "008"),
        new("Flanders", "009"),
        new("Otto", "010"),
        new("Skinner", "011"),
        new("Apu", "012"),
        new("Barney", "013"),
        new("Nelson", "014"),
        new("Milhouse", "015"),
        new("Krusty", "016"),
        new("ComicGuy", "017"),
        new("Ralph", "018"),
        new("Willie", "019"),
        new("FatTony", "020"),
        new("Wiggum", "021"),
        new("Martin", "022"),
        new("Hans", "023"),
        new("Selma", "024"),
        new("Carl", "025"),
        new("MadamWu", "026"),
        new("Lenny", "027"),
        new("Quimby", "028"),
        new("Grampa", "029"),
        new("Hibbert", "030"),
        new("Patty", "031"),
        new("Lovejoy", "032"),
        new("Todd", "033"),
        new("Manjula", "034"),
        new("SideshowBob", "035"),
        new("Rod", "036"),
        new("Agnes", "037"),
        new("Troy", "038"),
        new("Artie", "039"),
        new("Mona", "040"),
        new("Itchy", "041"),
        new("Scratchy", "042"),
        new("DuffMan", "043"),
        new("Blinky", "044"),
        };
    private static Dictionary<string, string> _usernamesAndCustomIDs = new() { 
        { "Bart", "001" }, 
        { "Homer", "002" },
        { "Marge", "003" },
        { "Lisa", "004" },
        { "Maggie", "005" },
        { "Moe", "006" },
        { "MrBurns", "007" },
        { "Smithers", "008" },
        { "Flanders", "009" },
        { "Otto", "010" },
        { "Skinner", "011" },
        { "Apu", "012" },
        { "Barney", "013" },
        { "Nelson", "014" },
        { "Milhouse", "015" },
        { "Krusty", "016" },
        { "ComicGuy", "017" },
        { "Ralph", "018" },
        { "Willie", "019" },
        { "FatTony", "020" },
        { "Wiggum", "021" },
        { "Martin", "022" },
        { "Hans", "023" },
        { "Selma", "024" },
        { "Carl", "025" },
        { "MadamWu", "026" },
        { "Lenny", "027" },
        { "Quimby", "028" },
        { "Grampa", "029" },
        { "Hibbert", "030" },
        { "Patty", "031" },
        { "Lovejoy", "032" },
        { "Todd", "033" },
        { "Manjula", "034" },
        { "SideshowBob", "035" },
        { "Rod", "036" },
        { "Agnes", "037" },
        { "Troy", "038" },
        { "Artie", "039" },
        { "Mona", "040" },
        { "Itchy", "041" },
        { "Scratchy", "042" },
        { "DuffMan", "043" },
        { "Blinky", "044" },
    };

    public Button SeedAccountsButton;
    public Button SeedLeaderboardsButton;
    private bool _isAwaitingLogin = false;
    private bool _isAwaitingLeaderboard = false;
    private const int playFabWaitTime = 4;


    void Start()
    {
        var levelDB = Resources.Load<LevelDatabase>("LevelDB");        
        SeedLeaderboardsButton.onClick.AddListener(
                () => StartCoroutine(SeedLevelLeaderboardsRoutine(levelDB))
            );
    }

    private IEnumerator SeedLevelLeaderboardsRoutine(LevelDatabase levelDB)
    {
        GameManager.Instance.OnLoading?.Invoke(true);
        foreach (var testPlayer in _testPlayers)
        {
            _isAwaitingLogin = true;
            CreateAccountAndUserName(testPlayer);
            yield return new WaitWhile(() => _isAwaitingLogin);
            yield return new WaitForSeconds(playFabWaitTime/2);
            foreach (var name in levelDB.LevelOrder)
            {
                var level = levelDB.LoadByName(name);
                _isAwaitingLeaderboard = true;
                AddLeaderboardRecord(level);
                yield return new WaitWhile(() => _isAwaitingLeaderboard);
                Debug.Log("Record created in level " + level.Name + " for player " + testPlayer.Name);
                yield return new WaitForSeconds(playFabWaitTime/2);
            }
            Debug.Log($"Leaderboard seeding complete for " + testPlayer.Name);
            yield return new WaitForSeconds(playFabWaitTime);
        }
        GameManager.Instance.OnLoading?.Invoke(false);
        ReturnToPlayer();
    }


    private void CreateAccountAndUserName(TestPlayer testPlayer)
    {
        //Create/log in account
        PlayFabClientAPI.LoginWithCustomID(
            new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = testPlayer.CustomID,
                CreateAccount = true,
                InfoRequestParameters = InfoRequestParams
            },
            // Success
            (LoginResult result) =>
            {
                if(result.InfoResultPayload == null || string.IsNullOrEmpty(result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName))
                {
                    PlayFabClientAPI.UpdateUserTitleDisplayName(
                    new UpdateUserTitleDisplayNameRequest()
                    {
                        DisplayName = testPlayer.Name + "#",
                    },

                    (result) => 
                    {
                        _isAwaitingLogin = false;
                        Debug.Log("Name and account successfully created for: " + result.DisplayName);
                    }, 
                    OnTestError
                    );
                }
                else
                {
                    Debug.Log(testPlayer.Name + " logged in without creating new display name.");
                    _isAwaitingLogin = false;
                }
            },
            OnTestError
        );
    }
    private void AddLeaderboardRecord(Level level)
    {
        List<StatisticUpdate> stats = new()
        {
            new StatisticUpdate()
            {
                StatisticName = level.LeaderboardKey,
                Value = UnityEngine.Random.Range((int)(level.MedalTimes.Silver * 1000), (int)(level.MedalTimes.Red * 1000)) * -1
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(
            new UpdatePlayerStatisticsRequest()
            {
                Statistics = stats
            },
            OnUpdateLeaderboardRecordSuccess,
            OnTestError
        );
    }

    private void OnUpdateLeaderboardRecordSuccess(UpdatePlayerStatisticsResult result)
    {
        _isAwaitingLeaderboard = false;
    }

    private void OnTestError(PlayFabError error)
    {
        Debug.Log("Test error: " + error.ErrorMessage);
        _isAwaitingLogin = false;
        _isAwaitingLeaderboard = false;
    }

    private void ReturnToPlayer()
    {
        GameManager.Instance.PlayFabManager.OnInitializationComplete += GameManager.Instance.OnInitializationComplete;
        StartCoroutine(GameManager.Instance.PlayFabManager.Initialize(GameManager.Instance, false));
    }

}

public class TestPlayer
{
    public string Name;
    public string CustomID;
    public string SessionTicket = null;
    public string PlayFabID;

    public TestPlayer(string name, string id, string sessionTicket, string playFabID)
    {
        Name = name;
        CustomID = id;
        SessionTicket = sessionTicket;
        PlayFabID = playFabID;
    }

    public TestPlayer(string name, string id)
    {
        Name = name;
        CustomID = id;
    }
}
