using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public enum FinishScreenType { NewMedal, NewBestTime, Participant}

public class FinishScreenLoader : MonoBehaviour
{
    [SerializeField] private GameObject display, medal, newMedal, newBest, previousBlock;
    [SerializeField] private TMP_Text previousTime, playerTime;
    [SerializeField] private Sprite[] medalSprites;
    [SerializeField] private GameObject[] statusTexts;


    public void GenerateFinishScreen(FinishData finishData)
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

    public void ActivateDisplay()
    {
        display.SetActive(true);
    }
    public void DeactivateDisplay()
    {
        display.SetActive(false);
    }
}


