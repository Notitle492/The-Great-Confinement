using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    private Vector2 moveDirection = Vector2.zero;
    private bool interactPressed = false;
    private bool submitPressed = false;

    private static InputManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Found more than one Input Manager in the scene.");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static InputManager GetInstance()
    {
        return instance;
    }

    // ✅ 外部調用的讀取方法
    public Vector2 GetMovementDirection()
    {
        return moveDirection;
    }

    public bool GetInteractPressed()
    {
        return interactPressed;
    }

    public bool GetSubmitPressed()
    {
        if (submitPressed)
        {
            submitPressed = false; // 吃一次就清空
            return true;
        }
        return false;
    }

    // ✅ 這三個是給 Player Input 事件綁定用的，必須 public
    public void MovePressed(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
            moveDirection = context.ReadValue<Vector2>();
    }

    public void InteractButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
            interactPressed = true;
        else if (context.canceled)
            interactPressed = false;
    }

    public void SubmitButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
            submitPressed = true;
        else if (context.canceled)
            submitPressed = false;
    }
}
