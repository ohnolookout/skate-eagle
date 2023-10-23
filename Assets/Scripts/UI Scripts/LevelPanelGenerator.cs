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


    public void Generate(LevelNode node, PlayerRecord record, PlayerRecord previousRecord)
    {
        selectedLevel = node.level;
        levelName.text = selectedLevel.Name;
        ActivateObjects(record.status);
        if (record.status == CompletionStatus.Locked)
        {
            playButton.SetActive(false);
            lockedText.text = LockedMessage(node, record, previousRecord);
            return;
        }
        playButton.SetActive(true);
        attemptsCount.text = record.attemptsCount.ToString();
        if (record.status == CompletionStatus.Incomplete)
        {
            attemptsCount.color = Color.gray;
            attemptsTitle.color = Color.gray;
            return;
        }
        attemptsCount.color = Color.white;
        attemptsTitle.color = Color.white;
        bestTime.text = record.bestTime.ToString();
        medalImage.sprite = medalSprites[(int)record.medal];




    }

    private string LockedMessage(LevelNode node, PlayerRecord record, PlayerRecord previousRecord)
    {        
        if(previousRecord.status == CompletionStatus.Locked)
        {
            return "Locked";
        }
        int goldRequired = node.goldRequired - GameManager.Instance.Session.GoldPlusCount;
        if (goldRequired < 1)
        {
            return "Complete previous level to unlock.";
        }
        string pluralizedMedal = "medal";
        if (goldRequired > 1)
        {
            pluralizedMedal += "s";
        }
        if (previousRecord.status == CompletionStatus.Incomplete)
        {
            return $"Complete previous level and earn {goldRequired} more gold {pluralizedMedal} or better to unlock.";
        }
        return $"Earn {goldRequired} more gold {pluralizedMedal} or better to unlock.";


    }

    private void ActivateObjects(CompletionStatus panelType)
    {
        if(panelType == CompletionStatus.Locked)
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
        if (panelType == CompletionStatus.Incomplete)
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
