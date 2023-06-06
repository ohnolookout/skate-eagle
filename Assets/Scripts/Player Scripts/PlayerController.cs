using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum KeyState { Off, Down, Held, Up, Released };
    public KeyState jumpState = KeyState.Off, downState = KeyState.Off, stompState = KeyState.Off;
    public bool jump = false, down = false, stomp = false;
    public Vector2 rotation = new(0,0);
    private EagleScript eagleScript;
    private int downCount;
    private LogicScript logic;
    private int fake;

    void Awake()
    {
    }

    void Start()
    {
        eagleScript = GameObject.FindGameObjectWithTag("Player").GetComponent<EagleScript>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
    }

    public void OnJump(InputValue value = null)
    {
        if (value.isPressed)
        {
            eagleScript.JumpValidation();
            return;
        }
        eagleScript.JumpRelease();
    }

    public void OnJump(bool isPressed)
    {
        if (isPressed)
        {
            eagleScript.JumpValidation();
            return;
        }
        eagleScript.JumpRelease();
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
        if (eagleScript.Airborne && down)
        {
            downCount++;
            StartCoroutine(DoubleTapWindow());
        }
        if (downCount >= 2)
        {
            if (eagleScript.Airborne)
            {
                stomp = true;
            }
            downCount = 0;
        }
    }
    public void OnDown(bool isPressed)
    {
        down = isPressed;
        if (eagleScript.Airborne && down)
        {
            downCount++;
            StartCoroutine(DoubleTapWindow());
        }
        if (downCount >= 2)
        {
            if (eagleScript.Airborne)
            {
                stomp = true;
            }
            downCount = 0;
        }
    }

    public void OnRestartLevel(InputValue value = null)
    {
        if (logic.Started)
        {
            logic.RestartGame();
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
