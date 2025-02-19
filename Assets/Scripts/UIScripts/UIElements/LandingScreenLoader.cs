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
    [SerializeField] private GameObject display, globalLeaderGrid, friendLeaderGrid, nextLevelButton, bestTimeBadge, bestTimeBadgeGrayOut;
    public GameObject[] levelTimeGrays, strikeThroughs;
    public bool nextLevelUnlocked = false;
    private Action<ILevelManager> OnLanding;

    private void OnEnable()
    {
        OnLanding += _ => GenerateLanding();
        OnLanding += _ => ActivateDisplay();
        LevelManager.OnLanding += OnLanding;
    }
    public void GenerateLanding(Level level, PlayerRecord playerRecord)
    {
        levelName.text = playerRecord.levelName;
        GenerateLevelTimes(level.MedalTimes);
        GeneratePlayerBadge(playerRecord);
        nextLevelButton.SetActive(GameManager.Instance.SessionData.NextLevelUnlocked(GameManager.Instance.CurrentLevel));
    }
    public void GenerateLanding()
    {
        var level = GameManager.Instance.CurrentLevel;
        var playerRecord = GameManager.Instance.CurrentPlayerRecord;
        levelName.text = playerRecord.levelName;
        GenerateLevelTimes(level.MedalTimes);
        GeneratePlayerBadge(playerRecord);
        nextLevelButton.SetActive(GameManager.Instance.SessionData.NextLevelUnlocked(GameManager.Instance.CurrentLevel));
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

    private void GeneratePlayerBadge(PlayerRecord playerInfo)
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

    private void ActivateDisplay()
    {
        display.SetActive(true);
    }

    private void DeactivateDisplay()
    {
        display.SetActive(false);
    }

}
