using System;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using LootLocker.Requests;
using Newtonsoft.Json;

public static class LoginUtility
{
    private const int TimeoutInSeconds = 50;
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
                Debug.Log("Login timed out.");
                return null;
            }
            Debug.Log("Checking login... Time elapsed: " + timeElapsed);
            await Task.Delay(10);
        }
        return returnResponse;
    }
}
