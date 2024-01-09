using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public enum InputType { Player, UI, Both};

public class InputEventController
{
    private PlayerControls _inputActions;
    private InputType _inputType;
    public Action OnJumpPress, OnJumpRelease, OnDownPress, OnDownRelease, OnRotateRelease, OnRestart, OnRagdoll, OnSubmit;
    public Action<Vector2> OnRotate, OnNavigate;
    private Vector2 _rotateValue = new(0, 0);

    //Add enum to allow called to select UI or player inputs on construction. Create separate instance for levelManager and player.
    public InputEventController(InputType inputType)
    {
        _inputActions = new();
        _inputType = inputType;
        if(inputType == InputType.Player)
        {
            InitializePlayerInputs();
        }else if(inputType == InputType.UI)
        {
            InitializeUIInputs();
        }
        else
        {
            InitializePlayerInputs();
            InitializeUIInputs();
        }

    }

    private void InitializePlayerInputs()
    {
        _inputActions.Player.Enable();
        _inputActions.Player.Jump.started += DoJump;
        _inputActions.Player.Jump.canceled += DoJumpCanceled;
        _inputActions.Player.Down.started += DoDown;
        _inputActions.Player.Down.canceled += DoDownCanceled;
        _inputActions.Player.Rotate.started += DoRotate;
        _inputActions.Player.Rotate.canceled += DoRotateCanceled;
        _inputActions.Player.Ragdoll.started += DoRagdoll;
    }

    private void DoRagdoll(InputAction.CallbackContext obj)
    {
        OnRagdoll?.Invoke();
    }

    private void InitializeUIInputs()
    {
        _inputActions.UI.Enable();
        _inputActions.UI.Restart.started += DoRestart;
        _inputActions.UI.Navigate.started += DoNavigate;
        _inputActions.UI.Submit.started += DoSubmit;
    }

    public void DisableInputs()
    {
        if (_inputType == InputType.Player)
        {
            DisablePlayerInputs();
        }
        else if (_inputType == InputType.UI)
        {
            DisableUIInputs();
        }
        else
        {
            DisablePlayerInputs();
            DisableUIInputs();
        }
    }

    private void DoRotateCanceled(InputAction.CallbackContext obj)
    {
        _rotateValue = new(0, 0);
        OnRotateRelease?.Invoke();
    }

    private void DoRotate(InputAction.CallbackContext obj)
    {
        OnRotate?.Invoke(obj.ReadValue<Vector2>());
    }

    private void DoDownCanceled(InputAction.CallbackContext obj)
    {
        OnDownRelease?.Invoke();
    }

    private void DoDown(InputAction.CallbackContext obj)
    {
        OnDownPress?.Invoke();
    }

    private void DoJumpCanceled(InputAction.CallbackContext obj)
    {
        OnJumpRelease?.Invoke();
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        OnJumpPress?.Invoke();
    }

    public void DisablePlayerInputs()
    {
        _inputActions.Player.Disable();
    }

    public void DisableUIInputs()
    {
        _inputActions.UI.Disable();
    }
    private void DoSubmit(InputAction.CallbackContext obj)
    {
        OnSubmit?.Invoke();
    }

    private void DoNavigate(InputAction.CallbackContext obj)
    {
        OnNavigate.Invoke(obj.ReadValue<Vector2>());
    }

    private void DoRestart(InputAction.CallbackContext obj)
    {
        OnRestart?.Invoke();
    }

    public Vector2 RotateValue { get => _rotateValue; }
}
