using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMapNode : MonoBehaviour
{
    public GameObject lockedFade;
    public Button button;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetStatus(LevelNodeStatus status, Medal medal = Medal.Participant)
    {
        if(status == LevelNodeStatus.Locked)
        {
            return;
        }
        lockedFade.SetActive(false);
        if(status == LevelNodeStatus.Completed)
        {

        }

    }
}
