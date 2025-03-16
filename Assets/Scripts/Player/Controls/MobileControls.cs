using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileControls : MonoBehaviour, IOverlayScreen
{
    private InputEventController _controller;
    [SerializeField] private GameObject _display;
    void Awake()
    {
        LevelManager.OnPlayerCreated += SetController;
    }

    public void OnUpPress()
    {
        _controller.OnJumpPress?.Invoke();
    }
    public void OnUpRelease()
    {
        _controller.OnJumpRelease?.Invoke();
    }
    public void OnDownPress()
    {
        _controller.OnDownPress?.Invoke();

    }
    public void OnDownRelease()
    {
        _controller.OnDownRelease?.Invoke();
    }

    public void OnLeftPress()
    {
        _controller.OnRotate?.Invoke(new Vector2(-1, 0));
    }
    public void OnRightPress()
    {
        _controller.OnRotate?.Invoke(new Vector2(1, 0));
    }

    public void OnRotationRelease()
    {
        _controller.OnRotateRelease?.Invoke();
    }

    public void OnRestart()
    {
        _controller.OnRestart?.Invoke();
    }

    private void SetController(IPlayer player)
    {
        _controller = player.InputEvents;
    }

    public void ActivateDisplay(bool doActivate)
    {
        _display.SetActive(doActivate);
    }
}
