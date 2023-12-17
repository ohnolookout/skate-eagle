using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public enum FinishScreenType { NewMedal, NewBestTime, Participant}

public class FinishScreenLoader : MonoBehaviour 
{ 
    public GameObject medal, newMedal, newBest, previousBlock;
    public TMP_Text previousTime, playerTime;
    public Sprite[] medalSprites;
    public GameObject[] statusTexts;


    public void GenerateFinishScreen(LiveRunManager runManager)
    {
        FinishScreenData finishData = (FinishScreenData)runManager.FinishData;
        ClearOptionalText();
        playerTime.text = OverlayUtility.TimeToString(finishData.attemptTime);
        statusTexts[(int)finishData.finishType].SetActive(true);
        if (!Single.IsPositiveInfinity(finishData.previousBest))
        {
            previousTime.text = OverlayUtility.TimeToString(finishData.previousBest);
            previousBlock.SetActive(true);
        }
        if (finishData.finishType == FinishScreenType.NewMedal)
        {
            medal.GetComponent<Image>().sprite = medalSprites[(int)finishData.medal];
            medal.SetActive(true);
        }

    }

    public void GenerateFinishScreen(FinishScreenData finishData)
    {
        ClearOptionalText();
        playerTime.text = OverlayUtility.TimeToString(finishData.attemptTime);
        statusTexts[(int)finishData.finishType].SetActive(true);
        if (!Single.IsPositiveInfinity(finishData.previousBest))
        {
            previousTime.text = OverlayUtility.TimeToString(finishData.previousBest);
            previousBlock.SetActive(true);
        }
        if (finishData.finishType == FinishScreenType.NewMedal)
        {
            medal.GetComponent<Image>().sprite = medalSprites[(int)finishData.medal];
            medal.SetActive(true);
        }

    }

    public void ClearOptionalText()
    {
        previousBlock.SetActive(false);
        newMedal.SetActive(false);
        newBest.SetActive(false);
        medal.SetActive(false);

    }
}


