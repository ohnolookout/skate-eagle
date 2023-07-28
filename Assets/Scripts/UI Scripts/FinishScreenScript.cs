using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FinishScreenScript : MonoBehaviour
{
    public TMP_Text goldTimeText, silverTimeText, bronzeTimeText, timeText;
    public Sprite[] medalImages = new Sprite[5];
    public GameObject medal;

    //Generates the finish screen based on a LevelData object
    public void GenerateFinishScreen(Level levelData, float finishTime) 
    {
        SetLevelTimes(levelData.MedalTimes);
        SetMedalImage(3);
        for(int i = levelData.MedalTimes.TimesArray.Length - 1; i >= 0 ; i--)
        {
            if(finishTime <= levelData.MedalTimes.TimesArray[i])
            {
                SetMedalImage(i);
                if (i == 0) bronzeTimeText.fontStyle = FontStyles.Bold;
                else if (i == 1) silverTimeText.fontStyle = FontStyles.Bold;
                else if (i == 2) goldTimeText.fontStyle = FontStyles.Bold;
                break;
            }
        }
        SetTime(finishTime);

    }

    //Sets the medal times for the level
    void SetLevelTimes(MedalTimes times)
    {
        goldTimeText.text = FormatTime(times.Gold);
        silverTimeText.text = FormatTime(times.Silver);
        bronzeTimeText.text = FormatTime(times.Bronze);

    }

    //Sets the medal image and bolds the relevant text
    void SetMedalImage(int finishPosition)
    {
        Image medalImage = medal.GetComponent<Image>();
        medalImage.sprite = medalImages[finishPosition];
    }

    void SetTime(float time)
    {
        timeText.text = FormatTime(time);
    }

    public static string FormatTime(float time)
    {
        int milliseconds = (int) (time * 1000);
        int totalSeconds = (int) time;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int remainingMilliseconds = milliseconds % 1000;

        return string.Format("{0}:{1:D2}.{2:D3}", minutes, seconds, remainingMilliseconds);
    }

}
