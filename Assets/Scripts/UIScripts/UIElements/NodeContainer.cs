using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeContainer : MonoBehaviour
{
    public GameObject lockedFade;
    public LevelMenu menu;
    private LevelNode node;
    public int containerIndex;
    public Level level;
    public int goldRequired = 0;

    void Awake()
    {
        node = new(level, goldRequired);
    }

    public void SendToLevelPanel()
    {
        menu.SetLevelPanel(node, containerIndex);
    }

    public void SelectButton()
    {
        gameObject.GetComponent<Button>().Select();
    }

    public void Setup(CompletionStatus status)
    {
        if (status == CompletionStatus.Locked)
        {
            lockedFade.SetActive(true);
        }
        else
        {
            lockedFade.SetActive(false);
        }
    }

    public LevelNode Previous
    {
        get
        {
            return node.previous;
        }
        set
        {
            node.previous = value;
        }
    }

    public LevelNode Next
    {
        get
        {
            return node.next;
        }
        set
        {
            node.next = value;
        }
    }

    public LevelNode Node
    {
        get
        {
            return node;
        }
        set
        {
            node = value;
        }
    }
}
