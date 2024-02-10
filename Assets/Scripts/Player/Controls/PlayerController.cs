using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum KeyState { Off, Down, Held, Up, Released };
    public KeyState jumpState = KeyState.Off, downState = KeyState.Off, stompState = KeyState.Off;
    public bool down = false, stomp = false;
    public Vector2 rotation = new(0,0);
    private Player _player;
    private int downCount;
    private ILevelManager _levelManager;

    void Awake()
    {
        _player = GetComponent<Player>();
    }

    void Start()
    {
        _levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ILevelManager>();
    }

    public void OnJump(InputValue value = null)
    {
        if (value.isPressed)
        {
            _player.JumpValidation();
            return;
        }
        _player.JumpRelease();
    }

    public void OnJump(bool isPressed)
    {
        if (isPressed)
        {
            _player.JumpValidation();
            return;
        }
        _player.JumpRelease();
    }



    public void OnRotate(InputValue value)
    {
        rotation = value.Get<Vector2>();
    }

    public void OnRotate(Vector2 rotationValue)
    {
        rotation = rotationValue;
    }

    public void OnDown(InputValue value)
    {
        down = value.isPressed;
        if (!_player.Collided && down)
        {
            downCount++;
            StartCoroutine(DoubleTapWindow());
        }
        if (downCount >= 2)
        {
            if (!_player.Collided)
            {
                stomp = true;
            }
            downCount = 0;
        }
    }
    public void OnDown(bool isPressed)
    {
        down = isPressed;
        if (!_player.Collided && down)
        {
            downCount++;
            StartCoroutine(DoubleTapWindow());
        }
        if (downCount >= 2)
        {
            if (!_player.Collided)
            {
                stomp = true;
            }
            downCount = 0;
        }
    }

    public void OnRestartLevel(InputValue value = null)
    {
        if ((int)_levelManager.RunState > 1)
        {
            _levelManager.RestartGame();
            return;
        }
        if(_levelManager.RunState == RunState.Landing)
        {
            _levelManager.GoToStandby();
        }
    }

    public void OnRagdoll(InputValue value = null)
    {
        if ((int)_levelManager.RunState > 1)
        {
            _player.Ragdoll();
        }
    }



    public KeyState ProcessKeyState(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            return KeyState.Down;
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            return KeyState.Released;
        }
        if (context.phase == InputActionPhase.Performed)
        {
            return KeyState.Off;
        }
        return KeyState.Off;
    }

    private IEnumerator DoubleTapWindow()
    {
        float doubleTapDelay = 0.25f;
        yield return new WaitForSeconds(doubleTapDelay);
        downCount = Mathf.Clamp(downCount - 1, 0, 3);

    }
    
}
