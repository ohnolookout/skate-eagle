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
    private Image _medalImage;
    private OverlayManager _overlay;


    //Generates the finish screen based on a LevelData object
    public void GenerateFinishScreen(Level level, float finishTime, OverlayManager overlay) 
    {
        Debug.Log("Generating finish screen...");
        _medalImage = medal.GetComponent<Image>();
        _overlay = overlay;
        SetLevelTimes(level.MedalTimes);
        float[] times = level.MedalTimes.TimesArray;
        for (int i = 0; i < times.Length; i++)
        {
            if(finishTime <= times[i])
            {
                SetMedalImage(i);
                if (i == times.Length - 1) bronzeTimeText.fontStyle = FontStyles.Bold;
                else if (i == times.Length - 2) silverTimeText.fontStyle = FontStyles.Bold;
                else if (i == times.Length - 3) goldTimeText.fontStyle = FontStyles.Bold;
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
        _medalImage.sprite = medalImages[finishPosition];
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
