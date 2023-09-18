using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LandingScreenLoader : MonoBehaviour
{
    public TMP_Text levelName, playerTime, blueTime, goldTime, silverTime;
    public Image playerMedal;
    public Sprite[] medalSprites;
    public GameObject globalLeaderGrid, friendLeaderGrid, nextLevelButton;
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

    private void GeneratePlayerBadge(LevelTimeData playerInfo)
    {
        if(playerInfo.bestTime is null)
        {
            playerTime.text = "--:--";
            playerMedal.sprite = medalSprites[5];
        } else { 
            playerTime.text = OverlayUtility.TimeToString((float)playerInfo.bestTime);
            playerMedal.sprite = medalSprites[(int)playerInfo.medal];
            nextLevelUnlocked = (int)playerInfo.medal <= 3;
        }
    }
    
}
