using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileControlManager : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject eagle;
    private PlayerController controller;
    void Start()
    {
        eagle = GameObject.FindGameObjectWithTag("Player");
        controller = eagle.GetComponent<PlayerController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnUpPress()
    {
        controller.OnJump(true);
    }
    public void OnUpRelease()
    {
        controller.OnJump(false);
    }
    public void OnDownPress()
    {
        controller.OnDown(true);

    }
    public void OnDownRelease()
    {
        controller.OnDown(false);

    }

    public void OnLeftPress()
    {
        controller.OnRotate(new Vector2(-1, 0));
    }

    public void OnRotationRelease()
    {
        controller.OnRotate(new Vector2(0, 0));
    }

    public void OnRightPress()
    {
        controller.OnRotate(new Vector2(1, 0));
    }

    public void OnRestart()
    {
        controller.OnRestartLevel();
    }
}
