using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using System.Threading.Tasks;
using TMPro;

public class LeaderboardManager
{
    private const int TimeoutInSeconds = 5;

    #region Uploading Records
    public async Task<bool> UpdateLeaderboardRecord(PlayerRecord record)
    {
        string playerID = PlayerPrefs.GetString("PlayerID");
        if (record.leaderboardKey != "None")
        {
            LootLockerSubmitScoreResponse response = await UploadScoreTask(record, playerID);
            if (response != null && response.success == true)
            {
                Debug.Log("Score successfully uploaded to leaderboard");
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private async Task<LootLockerSubmitScoreResponse> UploadScoreTask(PlayerRecord record, string playerID)
    {
        float timeElapsed = 0;
        LootLockerSubmitScoreResponse returnResponse = null;
        LootLockerSDKManager.SubmitScore(playerID, (int)(record.bestTime * 1000), record.leaderboardKey, (response) =>
        {
            returnResponse = response;
        });

        while (returnResponse == null)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > TimeoutInSeconds)
            {
                Debug.Log("Submit score timed out.");
                break;
            }
            await Task.Delay(10);
        }
        return returnResponse;
    }
    #endregion

    #region Downloading Records

    public async Task<LootLockerLeaderboardMember[]> GetLeaderboardMembers(PlayerRecord currentRecord)
    {
        var leaderboardKey = currentRecord.leaderboardKey;
        if (leaderboardKey == "None")
        {
            Debug.Log("Leaderboard does not have a LootLocker key.");
            return new LootLockerLeaderboardMember[0];
        }
        var response = await GetLeaderboardMembersTask(leaderboardKey, 10, 0);
        if (response.success)
        {
            Debug.Log("Leaderboard members retrieved successfully!");
            return response.items;
        }
        else
        {
            Debug.Log("Unable to retrieve leaderboard members :(");
            return new LootLockerLeaderboardMember[0];
        }
    }

    private async Task<LootLockerGetScoreListResponse> GetLeaderboardMembersTask(string leaderboardKey, int memberCount, int startPosition)
    {
        float timeElapsed = 0;
        LootLockerGetScoreListResponse returnResponse = null;
		LootLockerSDKManager.GetScoreList(leaderboardKey, memberCount, startPosition, (response) =>
		{
			returnResponse = response;
		});
		while(returnResponse == null)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed > TimeoutInSeconds)
			{
				Debug.Log("Get leaderboard timed out.");
				break;
			}
			await Task.Delay(10);
        }
		return returnResponse;
    }

    public async Task<LootLockerGetMemberRankResponse> GetPlayerRank(string leaderboardKey, string playerID)
    {
        float timeElapsed = 0;
        LootLockerGetMemberRankResponse returnResponse = null;
        Debug.Log("Requesting member rank for playerID: " + playerID);
        LootLockerSDKManager.GetMemberRank(leaderboardKey, playerID, (response) =>
        {
            returnResponse = response;
        });
        while (returnResponse == null)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > TimeoutInSeconds)
            {
                Debug.Log("Get player rank timed out.");
                break;
            }
            await Task.Delay(10);
        }
        return returnResponse;
    }

    public async Task<LootLockerLeaderboardMember[]> GetLeaderboardFromRank(string leaderboardKey, int startIndex, int length)
    {
        var membersResponse = await GetLeaderboardMembersTask(leaderboardKey, length, startIndex);
        
        if (membersResponse.success)
        {
            Debug.Log("Leaderboard members retrieved successfully!");
            return membersResponse.items;
        }
        else
        {
            Debug.Log("Unable to retrieve leaderboard members :(");
            return new LootLockerLeaderboardMember[0];
        }
    }

    #endregion

}
