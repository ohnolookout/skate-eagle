using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelMenu : MonoBehaviour
{
    private GameManager gameManager;
    public LevelPanelGenerator levelPanel;
    public MedalCounts medalCounts;
    void Awake()
    {
        gameManager = GameManager.Instance;
    }
    void Start()
    {
        PopulateMedalCounts();
        ActivateNodes();
    }
    private void ActivateNodes()
    {
        NodeContainer[] nodeContainers = GetComponentsInChildren<NodeContainer>();
        gameManager.levelNodes = new LevelNode[nodeContainers.Length];
        int selectIndex = nodeContainers.Length - 1;
        for(int i = 0; i < nodeContainers.Length; i++)
        {
            LevelNode node = nodeContainers[i].Node;
            gameManager.levelNodes[i] = node;
            if (i > 0)
            {
                node.previous = nodeContainers[i-1].Node;
            }
            if (i < nodeContainers.Length - 1)
            {
                node.next = nodeContainers[i + 1].Node;
            }
            nodeContainers[i].Setup();
            if (node.status == LevelNodeStatus.Incomplete ||
                (node.status == LevelNodeStatus.Locked && node.previous.status == LevelNodeStatus.Complete))
            {
                selectIndex = i;
            }
            gameManager.levelNodes[i] = node;
        }
        SelectNode(nodeContainers[selectIndex]);
    }

    public void SetLevelPanel(LevelNode node)
    {
        gameManager.currentLevelNode = node;
        PlayerRecord record = gameManager.RecordFromLevel(node.level.Name);
        levelPanel.Generate(node, record);
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
            int count = gameManager.sessionData.medalCount[medals[i]];
            medalCounts.SetMedalCount(medals[i], count);
        }
        
    }
}
