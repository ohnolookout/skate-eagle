using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



public class LevelPanelGenerator : MonoBehaviour
{
    public GameObject bestBlock, incompleteText, attemptsBlock, medal, lockedBlock, playButton;
    public TMP_Text attemptsCount, attemptsTitle, bestTime, levelName, lockedText;
    public Image medalImage;
    public Sprite[] medalSprites;
    public MainMenu menu;
    private Level selectedLevel;


    public void Generate(LevelNode node, LevelRecords records)
    {
        selectedLevel = node.level;
        levelName.text = selectedLevel.Name;
        ActivateObjects(node.status);
        if (node.status == LevelNodeStatus.Locked)
        {
            playButton.SetActive(false);
            lockedText.text = LockedMessage(node);
            return;
        }
        playButton.SetActive(true);
        if (records != null)
        {
            attemptsCount.text = records.attemptsCount.ToString();
        }
        else
        {
            attemptsCount.text = "0";
        }
        if (node.status == LevelNodeStatus.Incomplete)
        {
            attemptsCount.color = Color.gray;
            attemptsTitle.color = Color.gray;
            return;
        }
        attemptsCount.color = Color.white;
        attemptsTitle.color = Color.white;
        bestTime.text = records.bestTime.ToString();
        medalImage.sprite = medalSprites[(int)records.medal];




    }

    private string LockedMessage(LevelNode node)
    {        
        if(node.previous.status == LevelNodeStatus.Locked)
        {
            return "Locked";
        }
        int goldRequired = node.goldRequired - LevelDataManager.Instance.sessionData.GoldPlusCount;
        if (goldRequired < 1)
        {
            return "Complete previous level to unlock.";
        }
        string pluralizedMedal = "medal";
        if (goldRequired > 1)
        {
            pluralizedMedal += "s";
        }
        if (node.previous.status == LevelNodeStatus.Incomplete)
        {
            return $"Complete previous level and earn {goldRequired} more gold {pluralizedMedal} or better to unlock.";
        }
        return $"Earn {goldRequired} more gold {pluralizedMedal} or better to unlock.";


    }

    private void ActivateObjects(LevelNodeStatus panelType)
    {
        if(panelType == LevelNodeStatus.Locked)
        {
            bestBlock.SetActive(false);
            incompleteText.SetActive(false);
            attemptsBlock.SetActive(false);
            medal.SetActive(false);
            lockedBlock.SetActive(true);
            return;
        }
        attemptsBlock.SetActive(true);
        lockedBlock.SetActive(false);
        if (panelType == LevelNodeStatus.Incomplete)
        {
            bestBlock.SetActive(false);
            incompleteText.SetActive(true);
            medal.SetActive(false);
            return;
        }
        bestBlock.SetActive(true);
        incompleteText.SetActive(false);
        medal.SetActive(true);
    }

    public void PlayLevel()
    {
        menu.LoadLevel(selectedLevel);
    }
}
