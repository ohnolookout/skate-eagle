using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMenu : MonoBehaviour
{
    private LevelDataManager levelManager;
    public LevelPanelGenerator levelPanel;
    void Awake()
    {
        levelManager = LevelDataManager.Instance;
    }
    void Start()
    {
        ActivateNodes();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ActivateNodes()
    {
        LevelMapNode[] nodes = GetComponentsInChildren<LevelMapNode>();
        foreach(LevelMapNode node in nodes)
        {
            LevelRecords records = levelManager.RecordFromLevel(node.level.Name);
            if(records is null)
            {
                node.SetStatus(LevelNodeStatus.Incomplete);
                //Activate most recent node
                SelectNode(node);
                return;
            }
            if(records.medal == Medal.Participant)
            {
                node.SetStatus(LevelNodeStatus.Incomplete);
                continue;
            }
            node.SetStatus(LevelNodeStatus.Completed, records.medal);
        }
        //If no incomplete node has been found, activate last node;
        SelectNode(nodes[nodes.Length - 1]);
    }

    public void SetLevelPanel(Level level, LevelNodeStatus nodeStatus)
    {
        LevelRecords records = levelManager.RecordFromLevel(level.Name);
        levelPanel.Generate(level, nodeStatus, records, 1);
    }

    public void SelectNode(LevelMapNode node)
    {
        node.SendToLevelPanel();
        node.SelectButton();
    }
}
