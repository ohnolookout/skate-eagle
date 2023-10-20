using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MedalCounts : MonoBehaviour
{
    public TMP_Text[] medalCountTexts;
    public GameObject[] medalCountBlocks;

    public void SetMedalCount(Medal medal, int count)
    {
        int medalIndex = (int)medal;
        if (count < 1)
        {
            medalCountBlocks[medalIndex].SetActive(false);
            return;
        }
        medalCountBlocks[medalIndex].SetActive(true);
        medalCountTexts[medalIndex].text = $"x {count}";
    }
}
