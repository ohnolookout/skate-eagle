using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileControlManager : MonoBehaviour
{
    private InputEventController controller;
    void Start()
    {
        controller = LevelManager.GetPlayer.InputEvents;
    }

    public void OnUpPress()
    {
        controller.OnJumpPress?.Invoke();
    }
    public void OnUpRelease()
    {
        controller.OnJumpRelease?.Invoke();
    }
    public void OnDownPress()
    {
        controller.OnDownPress?.Invoke();

    }
    public void OnDownRelease()
    {
        controller.OnDownRelease?.Invoke();
    }

    public void OnLeftPress()
    {
        controller.OnRotate?.Invoke(new Vector2(-1, 0));
    }
    public void OnRightPress()
    {
        controller.OnRotate?.Invoke(new Vector2(1, 0));
    }

    public void OnRotationRelease()
    {
        controller.OnRotateRelease?.Invoke();
    }

    public void OnRestart()
    {
        controller.OnRestart?.Invoke();
    }
}
