using System;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using LootLocker.Requests;
using Newtonsoft.Json;

public static class LoginUtility
{
    public static async Task<LoginStatus> GuestLogin()
    {
        var response = await StartGuestSessionTask();
        if (response.success)
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
        LootLockerGuestSessionResponse returnResponse = null;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            returnResponse = response;
        });

        while (returnResponse == null)
        {
            await Task.Delay(25);
        }
        return returnResponse;
    }
}
