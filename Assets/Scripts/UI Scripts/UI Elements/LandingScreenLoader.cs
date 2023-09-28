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
    public GameObject globalLeaderGrid, friendLeaderGrid, nextLevelButton, bestTimeBadge, bestTimeBadgeGrayOut;
    public GameObject[] levelTimeGrays, strikeThroughs;
    public bool nextLevelUnlocked = false;

    public void GenerateLanding(Level level, LevelRecords playerInfo)
    {
        levelName.text = playerInfo.levelName;
        GenerateLevelTimes(level.MedalTimes);
        GeneratePlayerBadge(playerInfo);
        nextLevelButton.SetActive(nextLevelUnlocked);
    }

    public void GenerateLeaderboards(LevelRecords[] globalTimes, LevelRecords[] friendTimes)
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
        //Use bestMedal - 1 as start index because grays and strikethroughs start at blue rather than red. Use max with 0 to ensure that a red medal also starts at 0 instead of -1;
        int startIndex = Mathf.Max((int)bestMedal - 1, 0);
        for (int i = startIndex; i < 3; i++)
        {
            levelTimeGrays[i].SetActive(true);
            strikeThroughs[i].SetActive(true);
        }
    }

    private void GeneratePlayerBadge(LevelRecords playerInfo)
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
        bestTimeBadgeGrayOut.SetActive(true);
    }

}
