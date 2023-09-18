using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinishScreenLoader : MonoBehaviour
{
    public GameObject medal, newMedal, newBest;
    public TMP_Text previousTime, playerTime;
    public Sprite[] medalSprites;

    public void GenerateFinishScreen(LevelTimeData playerData, float attemptTime)
    {
        Medal attemptMedal = playerData.level.MedalFromTime(attemptTime);
        if ((int)attemptMedal < (int)playerData.medal)
        {
            Populate(attemptMedal, attemptTime, (float)playerData.bestTime);
        }
        else
        {
            Populate(attemptTime, (float)playerData.bestTime);
        }
    }

    public void Populate(Medal attemptMedal, float attemptTime, float previousBestTime)
    {
        newMedal.SetActive(true);
        medal.GetComponent<Image>().sprite = medalSprites[(int)attemptMedal];
        medal.SetActive(true);
        playerTime.text = OverlayUtility.TimeToString(attemptTime);
        previousTime.text = OverlayUtility.TimeToString(previousBestTime);
    }

    public void Populate(float attemptTime, float previousBestTime)
    {
        if (attemptTime > previousBestTime)
        {
            newBest.SetActive(true);
        }
        playerTime.text = OverlayUtility.TimeToString(attemptTime);
        previousTime.text = OverlayUtility.TimeToString(previousBestTime);
    }
}
