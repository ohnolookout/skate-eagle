using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEventController : MonoBehaviour
{
    private PlayerControls _inputs;
    private LiveRunManager _runManager;
    private Vector2 _rotation;
    public event Action DoubleTap;
    private int _doubleTapCount = 0;
    private float _doubleTapTimer = -1, _doubleTapLimit = 0.25f;

    private void Awake()
    {
        _inputs = new();
        _runManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        _runManager.EnterLanding += _ => EnterUI();
        _runManager.EnterStandby += _ => EnterPlayer();
        _runManager.EnterFinish += _ => EnterUI();
        _runManager.EnterGameOver += _ => EnterUI(); 
        _inputs.Player.Jump.started += DoJump;
        _inputs.Player.Jump.canceled += JumpRelease;
        _inputs.Player.Down.started += OnDown;
        _inputs.Player.Down.canceled += StandUp;
        _inputs.Player.Rotate.started += DoRotate;
        _inputs.Player.RestartLevel.started += DoRestartLevel;
    }

    private void Update()
    {
    }

    private void OnEnable()
    {
        _inputs.UI.Enable();   
    }

    private void OnDisable()
    {
        _inputs.Player.Disable();
        _inputs.UI.Disable();
    }

    public void EnterUI()
    {
        _inputs.Player.Disable();
        _inputs.UI.Enable();
    }

    public void EnterPlayer()
    {
        _inputs.UI.Disable();
        _inputs.Player.Enable();
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void JumpRelease(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void OnDown(InputAction.CallbackContext obj)
    {
        if (_runManager.Player.Collided)
        {
            return;
        }
        _doubleTapCount++;
        if (_doubleTapCount >= 2)
        {
            DoubleTap?.Invoke();
            _doubleTapCount = 0;
        }
        else
        {
            StartCoroutine(DoubleTapCountdown());
        }
    }

    private void StandUp(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void DoRotate(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void DoRestartLevel(InputAction.CallbackContext obj)
    {
        _runManager.RestartGame();
    }

    private IEnumerator DoubleTapCountdown()
    {
        yield return new WaitForSeconds(_doubleTapLimit);
        _doubleTapCount = Mathf.Clamp(_doubleTapCount - 1, 0, 3);
    }

    public PlayerControls Inputs { get => _inputs; }

    public Vector2 Rotation { get => _inputs.Player.Rotate.ReadValue<Vector2>(); }
}
