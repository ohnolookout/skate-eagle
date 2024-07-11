using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeContainer : MonoBehaviour
{
    public GameObject lockedFade;
    public Button Button;
    public int containerIndex;
    public int goldRequired = 0;
    public Level _level;
    private LevelNode _node;
    public LevelNode Node { get => _node; set => _node = value; }

    void Awake()
    {
        _node = new(_level, goldRequired);
    }
    /*
    public void SendToLevelPanel()
    {
        menu.SetLevelPanel(node, containerIndex);
    }

    public void SelectButton()
    {
        gameObject.GetComponent<Button>().Select();
    }*/

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

}
