using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;

public class PlayerAccountManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoginRoutine());
    }

    IEnumerator LoginRoutine()
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("Play logged in with ID " + response.player_id);
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                done = false;
            }
            else
            {
                Debug.Log("Could not start player account session.");
                done = true;
            }
        });

        yield return new WaitWhile(() => done == false);
    }
}
