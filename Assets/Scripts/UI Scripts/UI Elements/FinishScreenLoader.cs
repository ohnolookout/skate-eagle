using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FinishScreenLoader : MonoBehaviour
{
    public GameObject medal, newMedal, newBest, previousBlock;
    public TMP_Text previousTime, playerTime;
    public Sprite[] medalSprites;

    public void GenerateFinishScreen(LevelTimeData playerData, float attemptTime)
    {
        ClearOptionalText();
        Medal attemptMedal = playerData.level.MedalFromTime(attemptTime);
        bool isNewMedal = false;
        if ((int)attemptMedal < (int)playerData.medal)
        {
            PopulateMedal(attemptMedal);
            isNewMedal = true;
        }
        PopulateTimes(attemptTime, playerData.bestTime, isNewMedal);
    }

    public void PopulateMedal(Medal attemptMedal)
    {
        newMedal.SetActive(true);
        medal.GetComponent<Image>().sprite = medalSprites[(int)attemptMedal];
        medal.SetActive(true);
    }

    public void PopulateTimes(float attemptTime, float previousBestTime, bool hasNewMedal)
    {
        newBest.SetActive(attemptTime < previousBestTime && !hasNewMedal);
        if (Single.IsPositiveInfinity(previousBestTime))
        {
            previousBlock.SetActive(false);
        }
        else
        {
            previousTime.text = OverlayUtility.TimeToString(previousBestTime);
            previousBlock.SetActive(true);
        }
        playerTime.text = OverlayUtility.TimeToString(attemptTime);
    }

    public void ClearOptionalText()
    {
        previousBlock.SetActive(false);
        newMedal.SetActive(false);
        newBest.SetActive(false);
        medal.SetActive(false);

    }
}
