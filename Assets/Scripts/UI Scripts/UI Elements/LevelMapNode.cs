using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMapNode : MonoBehaviour
{
    public GameObject lockedFade;
    public Level level;
    public LevelMenu menu;
    private LevelNodeStatus nodeStatus = LevelNodeStatus.Locked;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetStatus(LevelNodeStatus status, Medal medal = Medal.Participant)
    {
        nodeStatus = status;
        if(nodeStatus == LevelNodeStatus.Locked)
        {
            return;
        }
        lockedFade.SetActive(false);
        if(nodeStatus == LevelNodeStatus.Completed)
        {

        }

    }

    public void SendToLevelPanel()
    {
        menu.SetLevelPanel(level, nodeStatus);
    }

    public void SelectButton()
    {
        gameObject.GetComponent<Button>().Select();
    }
}
