using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{
    private GameManager _gameManager;
    public LevelPanelGenerator levelPanel;
    public MedalCounts medalCounts;
    public EventSystem eventSystem;
    private NodeContainer[] nodeContainers;
    private int selectIndex = 0;
    public void Start()
    {
        _gameManager = GameManager.Instance;
        eventSystem = EventSystem.current;
        PopulateMedalCounts();
        ActivateNodes();
    }

    void Update()
    {
        if(Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            if (eventSystem.currentSelectedGameObject == null || eventSystem.currentSelectedGameObject.transform.tag != "MapNode")
            {
                SelectNode(nodeContainers[selectIndex]);
            }
        }    
    }
    public void ActivateNodes()
    {
        nodeContainers = GetComponentsInChildren<NodeContainer>();
        List<LevelNode> nodes = new(_gameManager.Session.NodeDict.Values);
        selectIndex = nodeContainers.Length - 1;
        for (int i = 0; i < nodeContainers.Length; i++)
        {
            if(i >= nodes.Count) 
            {
                break;
            }
            PlayerRecord record = _gameManager.Session.Record(nodes[i].UID);
            nodeContainers[i].Node = nodes[i];
            nodeContainers[i].containerIndex = i;
            nodeContainers[i].Setup(record.status);
            if (record.status == CompletionStatus.Incomplete ||
                (record.status == CompletionStatus.Locked 
                && _gameManager.Session.PreviousLevelRecord(record.UID).status == CompletionStatus.Complete))
            {
                selectIndex = i;
            }
        }
        SelectNode(nodeContainers[selectIndex]);
    }

    public void SetLevelPanel(LevelNode node, int containerIndex)
    {
        PlayerRecord record = _gameManager.Session.Record(node.UID);
        levelPanel.Generate(node, record, _gameManager.Session.PreviousLevelRecord(node.UID));
        selectIndex = containerIndex;
    }

    public void SelectNode(NodeContainer node)
    {
        node.SendToLevelPanel();
        node.SelectButton();
    }

    public void PopulateMedalCounts()
    {
        Medal[] medals = (Medal[])Enum.GetValues(typeof(Medal));
        for(int i = 0; i < medals.Length - 1; i++)
        {
            int count = _gameManager.Session.MedalCount[medals[i]];
            medalCounts.SetMedalCount(medals[i], count);
        }
        
    }
}
