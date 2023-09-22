using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class LandingScreenLoader : MonoBehaviour
{
    public TMP_Text levelName, playerTime, blueTime, goldTime, silverTime;
    public Image playerMedal;
    public Sprite[] medalSprites;
    public GameObject globalLeaderGrid, friendLeaderGrid, nextLevelButton, bestTimeBadge, levelTimes, levelMedals;
    public GameObject[] badgeGrays, levelTimeGrays;
    private LevelTimeData playerData;
    public bool nextLevelUnlocked = false;

    public void GenerateLanding(LevelTimeData playerInfo)
    {
        Debug.Log(playerInfo.level);
        levelName.text = playerInfo.level.Name;
        GenerateLevelTimes(playerInfo.level.MedalTimes);
        GeneratePlayerBadge(playerInfo);
        nextLevelButton.SetActive(nextLevelUnlocked);
    }

    public void GenerateLeaderboards(LevelTimeData[] globalTimes, LevelTimeData[] friendTimes)
    {

    }

    private void GenerateLevelTimes(MedalTimes times)
    {
        blueTime.text = OverlayUtility.TimeToString(times.Blue);
        goldTime.text = OverlayUtility.TimeToString(times.Gold);
        silverTime.text = OverlayUtility.TimeToString(times.Silver);
    }

    private void StrikeThruTimes(Medal bestMedal)
    {
        if ((int)bestMedal <= 1) 
        {
            blueTime.fontStyle = FontStyles.Strikethrough;
            levelTimeGrays[0].SetActive(true);
            levelTimeGrays[1].SetActive(true);

        }
        if ((int)bestMedal <= 2)
        {
            goldTime.fontStyle = FontStyles.Strikethrough;
            levelTimeGrays[2].SetActive(true);
            levelTimeGrays[3].SetActive(true);
        }
        if ((int)bestMedal <= 3)
        {
            silverTime.fontStyle = FontStyles.Strikethrough;
            levelTimeGrays[4].SetActive(true);
            levelTimeGrays[5].SetActive(true);
        }
    }

    private void GeneratePlayerBadge(LevelTimeData playerInfo)
    {
        if(Single.IsPositiveInfinity(playerInfo.bestTime))
        {
            playerTime.text = "--:--";
            playerMedal.sprite = medalSprites[5];
            DeactivatePlayerBadge();
        } else { 
            playerTime.text = OverlayUtility.TimeToString((float)playerInfo.bestTime);
            playerMedal.sprite = medalSprites[(int)playerInfo.medal];
            nextLevelUnlocked = (int)playerInfo.medal <= 3;
        }
        StrikeThruTimes(playerInfo.medal);
    }
    
    private void DeactivatePlayerBadge()
    {
        foreach(GameObject grayOut in badgeGrays)
        {
            grayOut.SetActive(true);
        }
    }

}
