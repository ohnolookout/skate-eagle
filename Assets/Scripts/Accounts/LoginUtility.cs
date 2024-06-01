using System;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using LootLocker.Requests;
using System.Linq;

public static class LoginUtility
{
    private const int TimeoutInSeconds = 5;
    public static async Task<LootLockerSessionResponse> RefreshLogin(GameManager gameManager)
    {
        Debug.Log("Initializing login...");
        var sessionStatus = await StartSession();
        //Will need to change to not be guest later;
        if (sessionStatus != null)
        {
            await SubmitDirtyRecords(gameManager.Session, gameManager.Leaderboard);
        }
        //Save serial to update removed dirty records
        SaveLoadUtility.SaveGame(gameManager.Session, gameManager);
        return sessionStatus;
    }

    private static async Task SubmitDirtyRecords(SessionData sessionData, LeaderboardManager leaderboardManager)
    {
        var dirtyUIDs = sessionData.SaveData.dirtyRecords.Keys.ToList();
        foreach (string levelUID in dirtyUIDs)
        {
            Debug.Log("Uploading dirty record for level " + sessionData.NodeDict[levelUID].Level.Name);
            bool uploadSuccessful = await leaderboardManager.UpdateLeaderboardRecord(sessionData.SaveData.dirtyRecords[levelUID]);
            if (uploadSuccessful)
            {
                Debug.Log("Dirty record upload successful!");
                sessionData.SaveData.dirtyRecords.Remove(levelUID);
            }
        }
    }

    public static async Task<LootLockerSessionResponse> StartSession()
    {
        return await StartGuestSessionTask();
    }

    private static async Task<LootLockerGuestSessionResponse> StartGuestSessionTask()
    {
        float timeElapsed = 0;
        LootLockerGuestSessionResponse returnResponse = null;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            returnResponse = response;
        });
        while (returnResponse == null)
        {
            
            timeElapsed += Time.deltaTime;
            if (timeElapsed > TimeoutInSeconds)
            {
                Debug.Log($"Login timed out after {timeElapsed} seconds.");
                return null;
            }
            await Task.Delay(10);
        }
        return returnResponse;
    }
}
