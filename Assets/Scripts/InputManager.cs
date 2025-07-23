using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    private Controls controls;
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
        

        controls = new Controls(); // ✅ 修正：初始化 Controls


        // ✅ 綁定 Move
        controls.Player.Move.performed += MovePressed;
        controls.Player.Move.canceled += MovePressed;

        // ✅ 綁定 Interact
        controls.Player.Interact.performed += InteractButtonPressed;
        controls.Player.Interact.canceled += InteractButtonPressed;

        // ✅ 綁定 Submit
        controls.Player.Submit.performed += SubmitButtonPressed;
        controls.Player.Submit.canceled += SubmitButtonPressed;
    }

    private void OnEnable()
    {
        controls.Enable(); // ✅ 修正
    }

    private void OnDisable()
    {
        controls.Disable(); // ✅ 修正
    }

    public static InputManager GetInstance()
    {
        return instance;
    }

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
            submitPressed = false;
            return true;
        }
        return false;
    }

    // ✅ 給 PlayerInput 事件用
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
