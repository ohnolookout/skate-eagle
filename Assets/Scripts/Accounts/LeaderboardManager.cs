using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using System.Threading.Tasks;

public class LeaderboardManager
{
    private const int TimeoutInSeconds = 5;
    public async Task<bool> UpdateRecord(PlayerRecord record)
    {
        string playerID = PlayerPrefs.GetString("PlayerID");
        if(record.leaderboardKey != "None")
        {
            LootLockerSubmitScoreResponse response = await UploadScoreTask(record, playerID);
            if(response != null && response.success == true)
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



}
