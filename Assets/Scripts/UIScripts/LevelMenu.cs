using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using TMPro;

public class LevelMenu : MonoBehaviour
{
    //public LevelPanelGenerator levelPanel;
    public MedalCounts medalCounts;
    private LevelMenuButton[] _levelButtons;
    private SessionData _sessionData;
    private EventSystem _eventSystem;
    private int _selectIndex = 0;
    public Button PlayLevelButton;
    public GameObject bestBlock, incompleteBlock;
    public TMP_Text bestTime, levelName, lockedText;
    public Image medalImage;
    public Sprite[] medalSprites;
    //public MainMenu menu;
    public Level selectedLevel;

    #region Monobehaviours/Setup
    public void Start()
    {
        _sessionData = GameManager.Instance.SessionData;
        PlayLevelButton.onClick.AddListener(() => GameManager.Instance.LoadLevel(selectedLevel));

        _eventSystem = EventSystem.current;
        PopulateMedalCounts();
        ActivateButtons();
    }

    void Update()
    {
        if(Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            if (_eventSystem.currentSelectedGameObject == null || _eventSystem.currentSelectedGameObject.transform.tag != "MapNode")
            {
                SelectButton(_levelButtons[_selectIndex]);
            }
        }    
    }

    public void PopulateMedalCounts()
    {
        Medal[] medals = (Medal[])Enum.GetValues(typeof(Medal));
        for (int i = 0; i < medals.Length - 1; i++)
        {
            int count = _sessionData.MedalCount[medals[i]];
            medalCounts.SetMedalCount(medals[i], count);
        }
    }
    #endregion

    #region Level Button Management
    private void ActivateButtons()
    {
        _levelButtons = GetComponentsInChildren<LevelMenuButton>();
        _selectIndex = _levelButtons.Length - 1;
        for (int i = 0; i < _levelButtons.Length; i++)
        {
            if(i >= _sessionData.LevelDB.LevelOrder.Count) 
            {
                break;
            }

            var isLastUnlockedButton = SetUpButton(_levelButtons[i], _sessionData.LevelDB.GetLevelByIndex(i), i);
            if (isLastUnlockedButton)
            {
                _selectIndex = i;
            }
        }
        SelectButton(_levelButtons[_selectIndex]);
    }

    //returns true if node is last unlocked node
    private bool SetUpButton(LevelMenuButton container, Level level, int index)
    {
        container.Level = level;
        container.containerIndex = index;

        // Log all relevant variables to debug NullReferenceException
        var recordStatus = _sessionData.GetRecordByUID(level.UID).status;

        container.Setup(recordStatus);
        container.Button.onClick.AddListener(() => SelectButton(container));

        if (recordStatus == CompletionStatus.Incomplete)
        {
            return true;
        }

        return false;
    }

    private void SelectButton(LevelMenuButton levelButton)
    {
        levelButton.Button.Select();

        var levelUID = levelButton.Level.UID;
        PlayerRecord record = _sessionData.GetRecordByUID(levelUID);
        GenerateLevelPanel(levelButton.Level, record, _sessionData.PreviousLevelRecord(levelUID));
        _selectIndex = levelButton.containerIndex;
    }
    #endregion

    #region Level Selection Panel
    public void GenerateLevelPanel(Level level, PlayerRecord record, PlayerRecord previousRecord)
    {
        var previousRecordStatus = previousRecord != null ? previousRecord.status : CompletionStatus.Complete;
        selectedLevel = level;
        levelName.text = selectedLevel.Name;

        ActivateLevelPanelObjects(level, record.status, previousRecordStatus);
        if (record.status == CompletionStatus.Locked)
        {
            return;
        }
        PlayLevelButton.gameObject.SetActive(true);
        bestTime.text = record.bestTime.ToString();
        medalImage.sprite = medalSprites[(int)record.medal];
    }

    private string GetLockedLevelMessage(Level level, CompletionStatus recordStatus, CompletionStatus previousRecordStatus)
    {
        if (previousRecordStatus == CompletionStatus.Locked)
        {
            return "Locked";
        }
        int goldRequired = level.GoldRequired - GameManager.Instance.SessionData.GoldPlusCount;
        if (goldRequired < 1)
        {
            return "Complete previous level to unlock.";
        }
        string pluralizedMedal = "medal";
        if (goldRequired > 1)
        {
            pluralizedMedal += "s";
        }
        if (previousRecordStatus == CompletionStatus.Incomplete)
        {
            return $"Complete previous level and earn {goldRequired} more gold {pluralizedMedal} or better to unlock.";
        }
        return $"Earn {goldRequired} more gold {pluralizedMedal} or better to unlock.";


    }

    private void ActivateLevelPanelObjects(Level level, CompletionStatus recordStatus, CompletionStatus previousRecordStatus)
    {
        if (recordStatus == CompletionStatus.Locked)
        {
            bestBlock.SetActive(false);
            incompleteBlock.SetActive(false);
            medalImage.gameObject.SetActive(false);
            lockedText.gameObject.SetActive(true);
            PlayLevelButton.gameObject.SetActive(false);
            lockedText.text = GetLockedLevelMessage(level, recordStatus, previousRecordStatus);
            return;
        }
        lockedText.gameObject.SetActive(false);
        if (recordStatus == CompletionStatus.Incomplete)
        {
            bestBlock.SetActive(false);
            incompleteBlock.SetActive(true);
            medalImage.gameObject.SetActive(false);
            return;
        }
        bestBlock.SetActive(true);
        incompleteBlock.SetActive(false);
        medalImage.gameObject.SetActive(true);
    }
    #endregion
}
