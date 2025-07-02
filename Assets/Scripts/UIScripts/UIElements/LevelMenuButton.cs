using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMenuButton : MonoBehaviour
{
    public GameObject lockedFade;
    public Button Button;
    public int containerIndex;
    public int goldRequired = 0;
    private Level _level;
    //private LevelNode _node;
    //public LevelNode Node { get => _node; set => _node = value; }
    public int GoldRequired => _level.GoldRequired;
    public Level Level { get  => _level; set => _level = value; }


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
