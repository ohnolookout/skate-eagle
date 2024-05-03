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
    public static async Task<LoginStatus> GuestLogin()
    {
        var response = await StartGuestSessionTask();
        if (response != null && response.success)
        {
            Debug.Log("Player logged in with ID " + response.player_id);
            PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
            return LoginStatus.Guest;
        }
        else
        {
            Debug.Log("Could not start player account session.");
            return LoginStatus.Offline;
        }
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


    public static async Task<LoginStatus> RefreshLogin(GameManager gameManager)
    {
        if (gameManager.LoginStatus != LoginStatus.Offline)
        {
            return gameManager.LoginStatus;
        }

        Debug.Log("Initializing login...");
        var loginStatus = await GuestLogin();
        //Will need to change to not be guest later;
        if (loginStatus != LoginStatus.Offline)
        {
            await SubmitDirtyRecords(gameManager.Session, gameManager.Leaderboard);
        }
        //Save serial to update removed dirty records
        SaveLoadUtility.SaveGame(gameManager.Session, loginStatus);
        return loginStatus;
    }


    private static async Task SubmitDirtyRecords(SessionData sessionData, LeaderboardManager leaderboardManager)
    {
        var dirtyUIDs = sessionData.SaveData.dirtyRecords.Keys.ToList();
        foreach (string levelUID in dirtyUIDs)
        {
            Debug.Log("Uploading dirty record for level " + sessionData.NodeDict[levelUID].Level.Name);
            bool uploadSuccessful = await leaderboardManager.UpdateRecord(sessionData.SaveData.dirtyRecords[levelUID]);
            if (uploadSuccessful)
            {
                Debug.Log("Dirty record upload successful!");
                sessionData.SaveData.dirtyRecords.Remove(levelUID);
            }
        }
    }
}
