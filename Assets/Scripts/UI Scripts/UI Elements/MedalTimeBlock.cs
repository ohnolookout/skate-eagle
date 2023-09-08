using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MedalTimeBlock : MonoBehaviour
{
    public GameObject[] medalTimeBlocks;
    private Sprite[] smallMedals;
    private MedalTimes medalTimes;

    private void Awake()
    {
        smallMedals = Resources.LoadAll<Sprite>("Sprites/Medals/Small Medal Sheet.png");
    }


    public void PopulateTimes(string[] times, Sprite[] medalSprites, int strikeThroughIndex)
    {
        for(int i = 0; i < times.Length; i++)
        {
            TMP_Text timeText = medalTimeBlocks[i].GetComponentInChildren<TextMeshPro>();
            if (i >= strikeThroughIndex)
            {
                timeText.fontStyle = FontStyles.Strikethrough;
            }
            medalTimeBlocks[i].GetComponent<Image>().sprite = medalSprites[i];
        }
    }

}
