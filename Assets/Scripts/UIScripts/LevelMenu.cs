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
    private NodeContainer[] _nodeContainers;
    private SessionData _sessionData;
    private EventSystem _eventSystem;
    private int _selectIndex = 0;
    public Button PlayLevelButton;
    public GameObject bestBlock, incompleteBlock;
    public TMP_Text bestTime, levelName, lockedText;
    public Image medalImage;
    public Sprite[] medalSprites;
    //public MainMenu menu;
    public Level SelectedLevel;

    #region Monobehaviours/Setup
    public void Start()
    {
        _sessionData = GameManager.Instance.SessionData;
        PlayLevelButton.onClick.AddListener(() => GameManager.Instance.LoadLevel(SelectedLevel));

        _eventSystem = EventSystem.current;
        PopulateMedalCounts();
        ActivateNodeContainers();
    }

    void Update()
    {
        if(Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            if (_eventSystem.currentSelectedGameObject == null || _eventSystem.currentSelectedGameObject.transform.tag != "MapNode")
            {
                SelectNode(_nodeContainers[_selectIndex]);
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

    #region Node Management
    private void ActivateNodeContainers()
    {
        _nodeContainers = GetComponentsInChildren<NodeContainer>();
        List<LevelNode> nodes = new(_sessionData.NodeDict.Values);
        _selectIndex = _nodeContainers.Length - 1;
        for (int i = 0; i < _nodeContainers.Length; i++)
        {
            if(i >= nodes.Count) 
            {
                break;
            }

            var isLastUnlockedNode = SetUpNodeContainer(_nodeContainers[i], nodes[i], i);
            if (isLastUnlockedNode)
            {
                _selectIndex = i;
            }
        }
        SelectNode(_nodeContainers[_selectIndex]);
    }

    //returns true if node is last unlocked node
    private bool SetUpNodeContainer(NodeContainer container, LevelNode node, int index)
    {
        container.Node = node;
        container.containerIndex = index;
        PlayerRecord record = _sessionData.GetRecordByUID(node.levelUID);
        container.Setup(record.status);
        container.Button.onClick.AddListener(() => SelectNode(container));

        if (record.status == CompletionStatus.Incomplete ||
            (record.status == CompletionStatus.Locked
            && _sessionData.PreviousLevelRecord(record.levelUID).status == CompletionStatus.Complete))
        {
            return true;
        }

        return false;
    }

    private void SelectNode(NodeContainer nodeContainer)
    {
        nodeContainer.Button.Select();

        var levelUID = nodeContainer.Node.levelUID;
        PlayerRecord record = _sessionData.GetRecordByUID(levelUID);
        GenerateLevelPanel(nodeContainer.Node, record, _sessionData.PreviousLevelRecord(levelUID));
        _selectIndex = nodeContainer.containerIndex;
    }
    #endregion

    #region Level Selection Panel
    public void GenerateLevelPanel(LevelNode node, PlayerRecord record, PlayerRecord previousRecord)
    {
        SelectedLevel = node.level;
        levelName.text = SelectedLevel.Name;
        ActivateLevelPanelObjects(record.status);
        if (record.status == CompletionStatus.Locked)
        {
            PlayLevelButton.gameObject.SetActive(false);
            lockedText.text = GetLockedLevelMessage(node, record, previousRecord);
            return;
        }
        PlayLevelButton.gameObject.SetActive(true);
        bestTime.text = record.bestTime.ToString();
        medalImage.sprite = medalSprites[(int)record.medal];
    }

    private string GetLockedLevelMessage(LevelNode node, PlayerRecord record, PlayerRecord previousRecord)
    {
        if (previousRecord.status == CompletionStatus.Locked)
        {
            return "Locked";
        }
        int goldRequired = node.goldRequired - GameManager.Instance.SessionData.GoldPlusCount;
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

    private void ActivateLevelPanelObjects(CompletionStatus panelType)
    {
        if (panelType == CompletionStatus.Locked)
        {
            bestBlock.SetActive(false);
            incompleteBlock.SetActive(false);
            medalImage.gameObject.SetActive(false);
            lockedText.gameObject.SetActive(true);
            return;
        }
        lockedText.gameObject.SetActive(false);
        if (panelType == CompletionStatus.Incomplete)
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

    /*
    public void PlayLevel()
    {
        menu.LoadLevel(SelectedLevel);
    }
    */
    #endregion
}
