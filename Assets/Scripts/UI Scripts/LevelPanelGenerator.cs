using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public enum LevelNodeStatus { Completed, Incomplete, Locked }
public class LevelPanelGenerator : MonoBehaviour
{
    public GameObject bestBlock, incompleteText, attemptsBlock, medal, lockedBlock;
    public TMP_Text attemptsCount, attemptsTitle, bestTime, levelName, lockedText;
    public Image medalImage;
    public Sprite[] medalSprites;
    public MainMenu menu;
    private Level selectedLevel;


    public void Generate(Level level, LevelNodeStatus panelType, LevelRecords records, int requiredToUnlock = 0)
    {
        selectedLevel = level;
        levelName.text = level.Name;
        ActivateObjects(panelType);
        if (panelType == LevelNodeStatus.Locked)
        {
            lockedText.text = $"Earn {requiredToUnlock} more points to unlock.";
            return;
        }
        if (records != null)
        {
            attemptsCount.text = records.attemptsCount.ToString();
        }
        else
        {
            attemptsCount.text = "0";
        }
        if (panelType == LevelNodeStatus.Incomplete)
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
        Debug.Log($"Playing current level {selectedLevel}");
        menu.LoadLevel(selectedLevel);
    }
}
